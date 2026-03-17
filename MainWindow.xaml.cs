using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Szamologep
{
    public partial class MainWindow : Window
    {
        private List<MathToken> _tokens = new();
        private bool _shiftActive = false;
        private bool _isRadians = false;
        private double _memory = 0;
        private bool _justCalculated = false;
        private bool _sdMode = false;
        private double _lastResult = 0;
        private string _lastExprText = "";

        private enum InputMode { Normal, RootDegree, RootArg, PowerBase, PowerExp, LogBase, LogArg }
        private InputMode _inputMode = InputMode.Normal;
        private MathToken? _pendingToken = null;
        private string _pendingBuffer = "";

        private enum BaseInputState { Idle, WaitingForFromBase, WaitingForToBase }
        private BaseInputState _baseState = BaseInputState.Idle;
        private string _baseNumber = "";
        private string _baseFrom = "";
        private string _baseToInput = "";

        private static readonly SolidColorBrush ShiftOnBrush = new(Color.FromRgb(0xf5, 0x9e, 0x0b));
        private static readonly SolidColorBrush ShiftOffBrush = new(Color.FromRgb(0x3d, 0x22, 0x00));

        public MainWindow()
        {
            InitializeComponent();
            MathRenderer.Tokens = _tokens;
            MathRenderer.Render();
        }

        private void RefreshUI()
        {
            AngleLabel.Text = _isRadians ? "RAD" : "DEG";
            ShiftBadge.Visibility = _shiftActive ? Visibility.Visible : Visibility.Collapsed;
            MemBadge.Visibility = _memory != 0 ? Visibility.Visible : Visibility.Collapsed;
            ShiftBtn.Background = _shiftActive ? ShiftOnBrush : ShiftOffBrush;

            switch (_baseState)
            {
                case BaseInputState.WaitingForFromBase:
                    StatusBlock.Text = $"Sz\u00e1m: [{_baseNumber}]  Add meg az eredeti alapot, majd BASE";
                    break;
                case BaseInputState.WaitingForToBase:
                    StatusBlock.Text = $"Sz\u00e1m: [{_baseNumber}]  Alap: {_baseFrom}  C\u00e9lrendszer, majd BASE";
                    break;
                default:
                    StatusBlock.Text = GetInputModeHint();
                    break;
            }
        }

        private string GetInputModeHint()
        {
            return _inputMode switch
            {
                InputMode.RootDegree => "Add meg a gyok fokszamat, majd nyomj egy operatort",
                InputMode.RootArg => "Add meg a gyokjel alatti szamot",
                InputMode.PowerBase => "Add meg az alapot",
                InputMode.PowerExp => "Add meg a kitevot",
                InputMode.LogBase => "Add meg a logaritmus alapjat, majd nyomj egy operatort",
                InputMode.LogArg => "Add meg a logaritmusalus szamot",
                _ => ""
            };
        }

        private void Rerender()
        {
            MathRenderer.Tokens = null;
            MathRenderer.Tokens = _tokens;
            MathRenderer.Render();
            DisplayScroller.ScrollToRightEnd();
        }

        private void AppendDigit(string d)
        {
            if (_justCalculated && _inputMode == InputMode.Normal)
            {
                _tokens.Clear();
                _justCalculated = false;
                _sdMode = false;
            }

            switch (_inputMode)
            {
                case InputMode.RootDegree:
                    _pendingBuffer += d;
                    _pendingToken!.Degree = _pendingBuffer;
                    break;
                case InputMode.RootArg:
                    _pendingBuffer += d;
                    _pendingToken!.Value = _pendingBuffer;
                    break;
                case InputMode.PowerBase:
                    _pendingBuffer += d;
                    _pendingToken!.Value = _pendingBuffer;
                    break;
                case InputMode.PowerExp:
                    _pendingBuffer += d;
                    _pendingToken!.Degree = _pendingBuffer;
                    break;
                case InputMode.LogBase:
                    _pendingBuffer += d;
                    _pendingToken!.Degree = _pendingBuffer;
                    break;
                case InputMode.LogArg:
                    _pendingBuffer += d;
                    _pendingToken!.Value = _pendingBuffer;
                    break;
                default:
                    _tokens.Add(new MathToken { Type = TokenType.Number, Value = d });
                    MergeLastNumbers();
                    break;
            }
            Rerender();
            RefreshUI();
        }

        private void MergeLastNumbers()
        {
            if (_tokens.Count < 2) return;
            var last = _tokens[^1];
            var prev = _tokens[^2];
            if (last.Type == TokenType.Number && prev.Type == TokenType.Number)
            {
                prev.Value += last.Value;
                _tokens.RemoveAt(_tokens.Count - 1);
            }
        }

        private void AppendDot()
        {
            if (_inputMode == InputMode.RootArg || _inputMode == InputMode.PowerBase
                || _inputMode == InputMode.LogArg || _inputMode == InputMode.Normal)
            {
                string buf = _inputMode == InputMode.Normal ? "" : _pendingBuffer;
                if (buf.Contains('.')) return;
                if (_inputMode == InputMode.Normal)
                {
                    if (_tokens.Count > 0 && _tokens[^1].Type == TokenType.Number)
                        _tokens[^1].Value += ".";
                    else
                        _tokens.Add(new MathToken { Type = TokenType.Number, Value = "." });
                }
                else
                {
                    _pendingBuffer += ".";
                    _pendingToken!.Value = _pendingBuffer;
                }
                Rerender();
                RefreshUI();
            }
        }

        private void AdvancePendingMode()
        {
            if (_inputMode == InputMode.RootDegree)
            {
                _pendingBuffer = "";
                _inputMode = InputMode.RootArg;
                RefreshUI();
            }
            else if (_inputMode == InputMode.LogBase)
            {
                _pendingBuffer = "";
                _inputMode = InputMode.LogArg;
                RefreshUI();
            }
            else if (_inputMode == InputMode.PowerBase)
            {
                _pendingBuffer = "";
                _inputMode = InputMode.PowerExp;
                RefreshUI();
            }
        }

        private void AppendOp(string op)
        {
            if (_baseState != BaseInputState.Idle) return;

            if (_inputMode == InputMode.RootDegree || _inputMode == InputMode.LogBase || _inputMode == InputMode.PowerBase)
            {
                AdvancePendingMode();
                return;
            }
            if (_inputMode == InputMode.RootArg || _inputMode == InputMode.LogArg || _inputMode == InputMode.PowerExp)
            {
                _inputMode = InputMode.Normal;
                _pendingToken = null;
                _pendingBuffer = "";
            }

            _justCalculated = false;
            string display = op switch { "+" => "+", "-" => "\u2212", "*" => "\u00d7", "/" => "\u00f7", _ => op };
            _tokens.Add(new MathToken { Type = TokenType.Op, Value = display });
            Rerender();
            RefreshUI();
        }

        private void AppendParen(string p)
        {
            if (_inputMode == InputMode.RootDegree || _inputMode == InputMode.LogBase)
            { AdvancePendingMode(); return; }
            if (_inputMode == InputMode.RootArg || _inputMode == InputMode.LogArg || _inputMode == InputMode.PowerExp)
            { _inputMode = InputMode.Normal; _pendingToken = null; _pendingBuffer = ""; }
            _tokens.Add(new MathToken { Type = TokenType.Paren, Value = p });
            Rerender();
            RefreshUI();
        }

        private void DeleteLast()
        {
            if (_baseState != BaseInputState.Idle) return;
            if (_justCalculated) { ClearAll(); return; }

            switch (_inputMode)
            {
                case InputMode.RootDegree:
                case InputMode.RootArg:
                case InputMode.PowerBase:
                case InputMode.PowerExp:
                case InputMode.LogBase:
                case InputMode.LogArg:
                    if (_pendingBuffer.Length > 0)
                    {
                        _pendingBuffer = _pendingBuffer[..^1];
                        if (_inputMode is InputMode.RootDegree or InputMode.PowerExp or InputMode.LogBase)
                            _pendingToken!.Degree = _pendingBuffer;
                        else
                            _pendingToken!.Value = _pendingBuffer;
                    }
                    else
                    {
                        if (_pendingToken != null) _tokens.Remove(_pendingToken);
                        _inputMode = InputMode.Normal;
                        _pendingToken = null;
                    }
                    break;
                default:
                    if (_tokens.Count > 0)
                    {
                        var last = _tokens[^1];
                        if (last.Type == TokenType.Number && last.Value.Length > 1)
                            last.Value = last.Value[..^1];
                        else
                            _tokens.RemoveAt(_tokens.Count - 1);
                    }
                    break;
            }
            Rerender();
            RefreshUI();
        }

        private void ClearAll()
        {
            _tokens.Clear();
            _baseState = BaseInputState.Idle;
            _baseNumber = _baseFrom = _baseToInput = "";
            _shiftActive = false;
            _justCalculated = false;
            _sdMode = false;
            _inputMode = InputMode.Normal;
            _pendingToken = null;
            _pendingBuffer = "";
            ExpressionBlock.Text = "";
            SyncShiftButtons();
            Rerender();
            RefreshUI();
        }

        private void BtnNum_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState == BaseInputState.WaitingForFromBase)
            {
                string d = ((Button)sender).Tag.ToString()!;
                _baseFrom += d;
                MathRenderer.Tokens = new List<MathToken> { new MathToken { Type = TokenType.Number, Value = _baseFrom } };
                MathRenderer.Render();
                StatusBlock.Text = $"Sz\u00e1m: [{_baseNumber}]  Alap: {_baseFrom}";
                return;
            }
            if (_baseState == BaseInputState.WaitingForToBase)
            {
                string d = ((Button)sender).Tag.ToString()!;
                _baseToInput += d;
                MathRenderer.Tokens = new List<MathToken> { new MathToken { Type = TokenType.Number, Value = _baseToInput } };
                MathRenderer.Render();
                StatusBlock.Text = $"Sz\u00e1m: [{_baseNumber}]  Alap: {_baseFrom}  C\u00e9l: {_baseToInput}";
                return;
            }
            AppendDigit(((Button)sender).Tag.ToString()!);
        }

        private void BtnDot_Click(object sender, RoutedEventArgs e) => AppendDot();
        private void BtnOp_Click(object sender, RoutedEventArgs e) => AppendOp(((Button)sender).Tag.ToString()!);
        private void BtnParen_Click(object sender, RoutedEventArgs e) => AppendParen(((Button)sender).Tag.ToString()!);
        private void BtnDel_Click(object sender, RoutedEventArgs e) => DeleteLast();
        private void BtnAC_Click(object sender, RoutedEventArgs e) => ClearAll();

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            _shiftActive = !_shiftActive;
            SyncShiftButtons();
            RefreshUI();
        }

        private void SyncShiftButtons()
        {
            SinBtn.Content = _shiftActive ? "sin\u207b\u00b9" : "sin";
            CosBtn.Content = _shiftActive ? "cos\u207b\u00b9" : "cos";
            TanBtn.Content = _shiftActive ? "tan\u207b\u00b9" : "tan";
            LnBtn.Content = _shiftActive ? "e\u02e3" : "ln";
        }

        private void DeactivateShift()
        {
            _shiftActive = false;
            SyncShiftButtons();
            RefreshUI();
        }

        private void BtnDegRad_Click(object sender, RoutedEventArgs e) { _isRadians = !_isRadians; RefreshUI(); }

        private void BtnTrig_Click(object sender, RoutedEventArgs e)
        {
            string fn = ((Button)sender).Tag.ToString()!;
            if (_shiftActive)
            {
                fn = fn switch { "sin" => "asin", "cos" => "acos", "tan" => "atan", _ => fn };
                DeactivateShift();
            }
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Func, Value = fn });
            Rerender(); RefreshUI();
        }

        private void BtnPi_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Symbol, Value = "\u03c0" });
            Rerender(); RefreshUI();
        }

        private void BtnEuler_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Symbol, Value = "e" });
            Rerender(); RefreshUI();
        }

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                StartPower("10");
                DeactivateShift();
                return;
            }
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            var tok = new MathToken { Type = TokenType.Log, Degree = "", Value = "" };
            _tokens.Add(tok);
            _pendingToken = tok;
            _pendingBuffer = "";
            _inputMode = InputMode.LogBase;
            Rerender(); RefreshUI();
        }

        private void BtnLn_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                StartPower("e");
                DeactivateShift();
                return;
            }
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Func, Value = "ln" });
            Rerender(); RefreshUI();
        }

        private void BtnNthRoot_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                StartPower("");
                DeactivateShift();
                return;
            }
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            var tok = new MathToken { Type = TokenType.Root, Degree = "", Value = "" };
            _tokens.Add(tok);
            _pendingToken = tok;
            _pendingBuffer = "";
            _inputMode = InputMode.RootDegree;
            Rerender(); RefreshUI();
        }

        private void BtnPow_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { StartPowerCube(); DeactivateShift(); return; }
            StartPower("");
        }

        private void BtnSq_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { StartReciprocal(); DeactivateShift(); return; }
            StartSquare();
        }

        private void StartPower(string preset)
        {
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            var tok = new MathToken { Type = TokenType.Power, Value = "", Degree = preset };
            _tokens.Add(tok);
            _pendingToken = tok;
            _pendingBuffer = "";
            _inputMode = InputMode.PowerBase;
            Rerender(); RefreshUI();
        }

        private void StartSquare()
        {
            if (_tokens.Count > 0)
            {
                var last = _tokens[^1];
                if (last.Type == TokenType.Number || last.Type == TokenType.Symbol)
                {
                    string baseVal = last.Value;
                    _tokens.RemoveAt(_tokens.Count - 1);
                    _tokens.Add(new MathToken { Type = TokenType.Power, Value = baseVal, Degree = "2" });
                    Rerender(); RefreshUI();
                    return;
                }
            }
            StartPower("");
            _pendingToken!.Degree = "2";
            _inputMode = InputMode.PowerBase;
        }

        private void StartPowerCube()
        {
            if (_tokens.Count > 0)
            {
                var last = _tokens[^1];
                if (last.Type == TokenType.Number || last.Type == TokenType.Symbol)
                {
                    string baseVal = last.Value;
                    _tokens.RemoveAt(_tokens.Count - 1);
                    _tokens.Add(new MathToken { Type = TokenType.Power, Value = baseVal, Degree = "3" });
                    Rerender(); RefreshUI();
                    return;
                }
            }
            StartPower("");
            _pendingToken!.Degree = "3";
            _inputMode = InputMode.PowerBase;
        }

        private void StartReciprocal()
        {
            if (_tokens.Count > 0)
            {
                var last = _tokens[^1];
                if (last.Type == TokenType.Number || last.Type == TokenType.Symbol)
                {
                    string baseVal = last.Value;
                    _tokens.RemoveAt(_tokens.Count - 1);
                    _tokens.Add(new MathToken { Type = TokenType.Power, Value = baseVal, Degree = "-1" });
                    Rerender(); RefreshUI();
                    return;
                }
            }
        }

        private void BtnRecip_Click(object sender, RoutedEventArgs e) => StartReciprocal();
        private void BtnAbs_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Func, Value = "abs" });
            Rerender(); RefreshUI();
        }

        private void BtnMC_Click(object sender, RoutedEventArgs e) { _memory = 0; RefreshUI(); }

        private void BtnMR_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _tokens.Clear(); _justCalculated = false; }
            _tokens.Add(new MathToken { Type = TokenType.Number, Value = _memory.ToString("G15") });
            Rerender(); RefreshUI();
        }

        private void BtnMPlus_Click(object sender, RoutedEventArgs e) { _memory += _justCalculated ? _lastResult : TryEvalCurrent(); RefreshUI(); }
        private void BtnMMinus_Click(object sender, RoutedEventArgs e) { _memory -= _justCalculated ? _lastResult : TryEvalCurrent(); RefreshUI(); }
        private void BtnMS_Click(object sender, RoutedEventArgs e) { _memory = _justCalculated ? _lastResult : TryEvalCurrent(); RefreshUI(); }

        private double TryEvalCurrent()
        {
            try { return Evaluate(TokensToExpr(_tokens)); }
            catch { return 0; }
        }

        private void BtnBase_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState == BaseInputState.Idle)
            {
                _baseNumber = TokensToExpr(_tokens).Trim();
                if (string.IsNullOrEmpty(_baseNumber) && _justCalculated)
                    _baseNumber = _lastResult.ToString("G15");
                _baseFrom = ""; _baseToInput = "";
                _tokens.Clear();
                _baseState = BaseInputState.WaitingForFromBase;
                Rerender(); RefreshUI();
            }
            else if (_baseState == BaseInputState.WaitingForFromBase)
            {
                if (string.IsNullOrEmpty(_baseFrom)) return;
                _baseToInput = "";
                _tokens.Clear();
                _baseState = BaseInputState.WaitingForToBase;
                Rerender(); RefreshUI();
            }
            else if (_baseState == BaseInputState.WaitingForToBase)
            {
                string toStr = _baseToInput.Trim();
                if (string.IsNullOrEmpty(toStr)) return;
                try
                {
                    int fromB = int.Parse(_baseFrom);
                    int toB = int.Parse(toStr);
                    if (fromB < 2 || fromB > 36 || toB < 2 || toB > 36)
                        throw new Exception("Alap 2 \u00e9s 36 k\u00f6z\u00e9 kell essen");
                    bool neg = _baseNumber.StartsWith("-");
                    string numStr = neg ? _baseNumber[1..] : _baseNumber;
                    long number = Convert.ToInt64(numStr, fromB);
                    if (neg) number = -number;
                    string result = neg
                        ? "-" + Convert.ToString(Math.Abs(number), toB).ToUpper()
                        : Convert.ToString(number, toB).ToUpper();
                    ExpressionBlock.Text = $"{_baseNumber}({_baseFrom})\u2192({toB})";
                    _tokens.Clear();
                    _tokens.Add(new MathToken { Type = TokenType.Number, Value = result });
                    _lastResult = number;
                    _justCalculated = true;
                }
                catch (Exception ex)
                {
                    _tokens.Clear();
                    _tokens.Add(new MathToken { Type = TokenType.Symbol, Value = "Hiba" });
                    ExpressionBlock.Text = ex.Message;
                }
                _baseState = BaseInputState.Idle;
                _baseNumber = _baseFrom = _baseToInput = "";
                Rerender(); RefreshUI();
            }
        }

        private void BtnSD_Click(object sender, RoutedEventArgs e)
        {
            if (!_justCalculated) return;
            _sdMode = !_sdMode;
            _tokens.Clear();
            _tokens.Add(new MathToken { Type = TokenType.Number, Value = _sdMode ? ToFraction(_lastResult) : FormatResult(_lastResult) });
            Rerender();
        }

        private string ToFraction(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return value.ToString();
            int sign = value < 0 ? -1 : 1;
            double absVal = Math.Abs(value);
            long bestNum = (long)Math.Round(absVal), bestDen = 1;
            double bestErr = Math.Abs(absVal - bestNum);
            for (long den = 2; den <= 100000; den++)
            {
                long num = (long)Math.Round(absVal * den);
                double err = Math.Abs(absVal - (double)num / den);
                if (err < bestErr) { bestErr = err; bestNum = num; bestDen = den; }
                if (bestErr < 1e-12) break;
            }
            long g = GCD(Math.Abs(bestNum), bestDen);
            bestNum /= g; bestDen /= g;
            return bestDen == 1 ? (sign * bestNum).ToString() : $"{sign * bestNum}/{bestDen}";
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);

        private void BtnEquals_Click(object sender, RoutedEventArgs e) => Calculate();

        private void Calculate()
        {
            if (_tokens.Count == 0) return;
            try
            {
                string expr = TokensToExpr(_tokens);
                double result = Evaluate(expr);
                _lastResult = result;
                _sdMode = false;
                _lastExprText = expr;
                ExpressionBlock.Text = expr + " =";
                _tokens.Clear();
                _tokens.Add(new MathToken { Type = TokenType.Number, Value = FormatResult(result) });
                _justCalculated = true;
                _inputMode = InputMode.Normal;
                _pendingToken = null;
                _pendingBuffer = "";
                Rerender(); RefreshUI();
            }
            catch (Exception ex)
            {
                _tokens.Clear();
                _tokens.Add(new MathToken { Type = TokenType.Symbol, Value = "Hiba" });
                ExpressionBlock.Text = ex.Message;
                Rerender();
            }
        }

        private string TokensToExpr(List<MathToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var tok in tokens)
            {
                switch (tok.Type)
                {
                    case TokenType.Number:
                    case TokenType.Symbol:
                        sb.Append(tok.Value);
                        break;
                    case TokenType.Op:
                        string opRaw = tok.Value switch
                        {
                            "\u2212" => "-",
                            "\u00d7" => "*",
                            "\u00f7" => "/",
                            _ => tok.Value
                        };
                        sb.Append(opRaw);
                        break;
                    case TokenType.Paren:
                        sb.Append(tok.Value);
                        break;
                    case TokenType.Root:
                        string deg = string.IsNullOrEmpty(tok.Degree) ? "2" : tok.Degree;
                        sb.Append($"nroot({deg},{tok.Value})");
                        break;
                    case TokenType.Power:
                        string exp = string.IsNullOrEmpty(tok.Degree) ? "2" : tok.Degree;
                        string bse = string.IsNullOrEmpty(tok.Value) ? "0" : tok.Value;
                        sb.Append($"({bse})^({exp})");
                        break;
                    case TokenType.Log:
                        string logBase = string.IsNullOrEmpty(tok.Degree) ? "10" : tok.Degree;
                        sb.Append($"log({logBase},{tok.Value})");
                        break;
                    case TokenType.Func:
                        sb.Append(tok.Value + "(");
                        break;
                }
            }
            return sb.ToString();
        }

        private static string FormatResult(double r)
        {
            if (double.IsNaN(r)) return "Hiba";
            if (double.IsPositiveInfinity(r)) return "+\u221e";
            if (double.IsNegativeInfinity(r)) return "-\u221e";
            return r.ToString("G15");
        }

        private double Evaluate(string expr)
        {
            var tokens = Tokenize(expr);
            int pos = 0;
            double val = ParseAddSub(tokens, ref pos);
            if (pos < tokens.Count) throw new Exception("V\u00e1ratlan: " + tokens[pos]);
            return val;
        }

        private static List<string> Tokenize(string expr)
        {
            var list = new List<string>();
            int i = 0;
            expr = expr.Trim();
            while (i < expr.Length)
            {
                char c = expr[i];
                if (c == ' ') { i++; continue; }
                if (c == '\u03c0') { list.Add("\u03c0"); i++; continue; }
                if (c == 'e' && (i + 1 >= expr.Length || !char.IsLetterOrDigit(expr[i + 1])))
                { list.Add("e"); i++; continue; }
                if (char.IsLetter(c))
                {
                    var sb = new StringBuilder();
                    while (i < expr.Length && char.IsLetter(expr[i])) sb.Append(expr[i++]);
                    list.Add(sb.ToString());
                    continue;
                }
                if (char.IsDigit(c) || c == '.')
                {
                    var sb = new StringBuilder();
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) sb.Append(expr[i++]);
                    list.Add(sb.ToString());
                    continue;
                }
                if (c == '-'
                    && (list.Count == 0 || list[^1] is "+" or "-" or "*" or "/" or "^" or "(" or ",")
                    && i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.' || expr[i + 1] == '('))
                {
                    if (expr[i + 1] == '(')
                    { list.Add("-1"); list.Add("*"); i++; continue; }
                    var sb = new StringBuilder("-");
                    i++;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) sb.Append(expr[i++]);
                    list.Add(sb.ToString());
                    continue;
                }
                list.Add(c.ToString());
                i++;
            }
            return list;
        }

        private double ParseAddSub(List<string> t, ref int p)
        {
            double left = ParseMulDiv(t, ref p);
            while (p < t.Count && (t[p] == "+" || t[p] == "-"))
            {
                bool add = t[p++] == "+";
                double right = ParseMulDiv(t, ref p);
                left = add ? left + right : left - right;
            }
            return left;
        }

        private double ParseMulDiv(List<string> t, ref int p)
        {
            double left = ParsePow(t, ref p);
            while (p < t.Count && (t[p] == "*" || t[p] == "/"))
            {
                bool mul = t[p++] == "*";
                double right = ParsePow(t, ref p);
                if (!mul && right == 0) throw new DivideByZeroException("Null\u00e1val val\u00f3 oszt\u00e1s");
                left = mul ? left * right : left / right;
            }
            return left;
        }

        private double ParsePow(List<string> t, ref int p)
        {
            double left = ParseUnary(t, ref p);
            if (p < t.Count && t[p] == "^")
            { p++; double right = ParsePow(t, ref p); left = Math.Pow(left, right); }
            return left;
        }

        private double ParseUnary(List<string> t, ref int p)
        {
            if (p < t.Count && t[p] == "-") { p++; return -ParsePrimary(t, ref p); }
            if (p < t.Count && t[p] == "+") p++;
            return ParsePrimary(t, ref p);
        }

        private double ParsePrimary(List<string> t, ref int p)
        {
            if (p >= t.Count) throw new Exception("V\u00e1ratlan v\u00e9g");
            string tok = t[p];
            if (tok == "\u03c0") { p++; return Math.PI; }
            if (tok == "e") { p++; return Math.E; }
            if (double.TryParse(tok, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double num))
            { p++; return num; }
            if (tok == "(")
            {
                p++;
                double v = ParseAddSub(t, ref p);
                if (p < t.Count && t[p] == ")") p++;
                return v;
            }
            string fn = tok.ToLower();
            if (fn is "sin" or "cos" or "tan" or "asin" or "acos" or "atan"
                or "ln" or "log" or "sqrt" or "abs" or "nroot")
            {
                p++;
                bool hasParen = p < t.Count && t[p] == "(";
                if (hasParen) p++;
                if (fn == "log")
                {
                    int saved = p;
                    try
                    {
                        double b = ParseAddSub(t, ref p);
                        if (p < t.Count && t[p] == ",")
                        {
                            p++;
                            double arg2 = ParseAddSub(t, ref p);
                            if (hasParen && p < t.Count && t[p] == ")") p++;
                            return Math.Log(arg2) / Math.Log(b);
                        }
                        p = saved;
                    }
                    catch { p = saved; }
                }
                if (fn == "nroot")
                {
                    double nVal = ParseAddSub(t, ref p);
                    if (p < t.Count && t[p] == ",") p++;
                    double xVal = ParseAddSub(t, ref p);
                    if (hasParen && p < t.Count && t[p] == ")") p++;
                    return Math.Pow(xVal, 1.0 / nVal);
                }
                double arg = ParseAddSub(t, ref p);
                if (hasParen && p < t.Count && t[p] == ")") p++;
                return ApplyFn(fn, arg);
            }
            throw new Exception($"Ismeretlen: {tok}");
        }

        private double ApplyFn(string fn, double x)
        {
            double toRad = _isRadians ? 1.0 : Math.PI / 180.0;
            double fromRad = _isRadians ? 1.0 : 180.0 / Math.PI;
            return fn switch
            {
                "sin" => Math.Sin(x * toRad),
                "cos" => Math.Cos(x * toRad),
                "tan" => Math.Tan(x * toRad),
                "asin" => Math.Asin(x) * fromRad,
                "acos" => Math.Acos(x) * fromRad,
                "atan" => Math.Atan(x) * fromRad,
                "ln" => Math.Log(x),
                "log" => Math.Log10(x),
                "sqrt" => Math.Sqrt(x),
                "abs" => Math.Abs(x),
                _ => throw new Exception($"Ismeretlen fn: {fn}")
            };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (!shift && e.Key >= Key.D0 && e.Key <= Key.D9)
            { AppendDigit(((int)(e.Key - Key.D0)).ToString()); e.Handled = true; return; }
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            { AppendDigit(((int)(e.Key - Key.NumPad0)).ToString()); e.Handled = true; return; }

            if (shift && e.Key == Key.D8) { AppendParen("("); e.Handled = true; return; }
            if (shift && e.Key == Key.D9) { AppendParen(")"); e.Handled = true; return; }
            if (shift && e.Key == Key.D6) { StartPower(""); e.Handled = true; return; }
            if (shift && e.Key == Key.OemPlus) { AppendOp("+"); e.Handled = true; return; }

            if (e.Key == Key.Add) { AppendOp("+"); e.Handled = true; return; }
            if (e.Key == Key.Subtract || (!shift && e.Key == Key.OemMinus)) { AppendOp("-"); e.Handled = true; return; }
            if (e.Key == Key.Multiply) { AppendOp("*"); e.Handled = true; return; }
            if (e.Key == Key.Divide) { AppendOp("/"); e.Handled = true; return; }
            if (!shift && e.Key == Key.OemQuestion) { AppendOp("/"); e.Handled = true; return; }
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.OemComma)
            { AppendDot(); e.Handled = true; return; }
            if (e.Key == Key.Enter || e.Key == Key.Return) { Calculate(); e.Handled = true; return; }
            if (e.Key == Key.Back) { DeleteLast(); e.Handled = true; return; }
            if (e.Key == Key.Escape || e.Key == Key.Delete) { ClearAll(); e.Handled = true; return; }
            if (!shift && e.Key == Key.E) { BtnEuler_Click(sender, e); e.Handled = true; return; }
            if (shift && e.Key == Key.P) { BtnPi_Click(sender, e); e.Handled = true; return; }
            if (shift && e.Key == Key.S) { BtnShift_Click(sender, e); e.Handled = true; return; }
        }
    }
}