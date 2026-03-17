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
        private string _expression = "";
        private bool _shiftActive = false;
        private bool _isRadians = false;
        private double _memory = 0;
        private bool _justCalculated = false;
        private bool _sdMode = false;
        private double _lastResult = 0;

        private enum BaseInputState { Idle, WaitingForFromBase, WaitingForToBase }
        private BaseInputState _baseState = BaseInputState.Idle;
        private string _baseNumber = "";
        private string _baseFrom = "";
        private string _baseToInput = "";

        private static readonly SolidColorBrush ShiftActiveColor = new SolidColorBrush(Color.FromRgb(0xf5, 0x9e, 0x0b));
        private static readonly SolidColorBrush ShiftInactiveColor = new SolidColorBrush(Color.FromRgb(0x3d, 0x22, 0x00));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RefreshUI()
        {
            tbAngle.Text = _isRadians ? "RAD" : "DEG";
            borderShift.Visibility = _shiftActive ? Visibility.Visible : Visibility.Collapsed;
            borderMem.Visibility = _memory != 0 ? Visibility.Visible : Visibility.Collapsed;
            btnShift.Background = _shiftActive ? ShiftActiveColor : ShiftInactiveColor;

            if (_baseState == BaseInputState.WaitingForFromBase)
                tbStatus.Text = $"Szám: [{_baseNumber}]  →  Add meg az eredeti alapot, majd BASE";
            else if (_baseState == BaseInputState.WaitingForToBase)
                tbStatus.Text = $"Szám: [{_baseNumber}]  Alap: {_baseFrom}  →  Add meg a célrendszert, majd BASE";
            else
                tbStatus.Text = "";
        }

        private void PutToDisplay(string val) => tbDisplay.Text = val;

        private void Append(string s)
        {
            if (_justCalculated)
            {
                bool isOp = s is "+" or "-" or "*" or "/" or "^";
                if (!isOp) _expression = "";
                _justCalculated = false;
                _sdMode = false;
            }
            _expression += s;
            PutToDisplay(_expression);
            RefreshUI();
        }

        private void BtnNum_Click(object sender, RoutedEventArgs e)
        {
            string d = ((Button)sender).Tag.ToString()!;

            if (_baseState == BaseInputState.WaitingForFromBase)
            {
                _baseFrom += d;
                PutToDisplay(_baseFrom);
                tbExpression.Text = $"{_baseNumber} [z alap: {_baseFrom}]";
                RefreshUI();
                return;
            }
            if (_baseState == BaseInputState.WaitingForToBase)
            {
                _baseToInput += d;
                PutToDisplay(_baseToInput);
                tbExpression.Text = $"{_baseNumber} [{_baseFrom}→{_baseToInput}]";
                RefreshUI();
                return;
            }

            if (_justCalculated) { _expression = ""; _justCalculated = false; _sdMode = false; }
            Append(d);
        }

        private void BtnDot_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState != BaseInputState.Idle) return;
            if (_justCalculated) { _expression = "0"; _justCalculated = false; _sdMode = false; }
            int lastOp = _expression.LastIndexOfAny(new[] { '+', '-', '*', '/' });
            string currentNum = lastOp >= 0 ? _expression[(lastOp + 1)..] : _expression;
            if (!currentNum.Contains('.')) Append(".");
        }

        private void BtnOp_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState != BaseInputState.Idle) return;
            string op = ((Button)sender).Tag.ToString()!;
            _justCalculated = false;
            Append(op);
        }

        private void BtnParen_Click(object sender, RoutedEventArgs e)
        {
            Append(((Button)sender).Tag.ToString()!);
        }

        private void BtnAC_Click(object sender, RoutedEventArgs e)
        {
            _expression = "";
            _baseState = BaseInputState.Idle;
            _baseNumber = _baseFrom = _baseToInput = "";
            _shiftActive = false;
            _justCalculated = false;
            _sdMode = false;
            PutToDisplay("0");
            tbExpression.Text = "";
            SyncShiftButtons();
            RefreshUI();
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState != BaseInputState.Idle) return;
            if (_justCalculated) { BtnAC_Click(sender, e); return; }
            if (_expression.Length > 0)
            {
                _expression = _expression[..^1];
                PutToDisplay(_expression.Length > 0 ? _expression : "0");
                tbExpression.Text = _expression;
                RefreshUI();
            }
        }

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            _shiftActive = !_shiftActive;
            SyncShiftButtons();
            RefreshUI();
        }

        private void SyncShiftButtons()
        {
            btnSin.Content = _shiftActive ? "sin⁻¹" : "sin";
            btnCos.Content = _shiftActive ? "cos⁻¹" : "cos";
            btnTan.Content = _shiftActive ? "tan⁻¹" : "tan";
            btnLog.Content = _shiftActive ? "10ˣ" : "log";
            btnLn.Content = _shiftActive ? "eˣ" : "ln";
            btnNthRoot.Content = _shiftActive ? "x²" : "ⁿ√x";
            btnPow.Content = _shiftActive ? "x³" : "xʸ";
            btnSq.Content = _shiftActive ? "1/x" : "x²";
        }

        private void DeactivateShift()
        {
            _shiftActive = false;
            SyncShiftButtons();
            RefreshUI();
        }

        private void BtnDegRad_Click(object sender, RoutedEventArgs e)
        {
            _isRadians = !_isRadians;
            RefreshUI();
        }

        private void BtnTrig_Click(object sender, RoutedEventArgs e)
        {
            string fn = ((Button)sender).Tag.ToString()!;
            if (_shiftActive)
            {
                fn = fn switch { "sin" => "asin", "cos" => "acos", "tan" => "atan", _ => fn };
                DeactivateShift();
            }
            Append(fn + "(");
        }

        private void BtnPi_Click(object sender, RoutedEventArgs e) => Append("π");
        private void BtnEuler_Click(object sender, RoutedEventArgs e) => Append("e");

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { Append("10^"); DeactivateShift(); }
            else Append("log(");
        }

        private void BtnLn_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { Append("e^"); DeactivateShift(); }
            else Append("ln(");
        }

        private void BtnNthRoot_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                Append("^2");
                DeactivateShift();
            }
            else
            {
                Append("nroot(");
            }
        }

        private void BtnPow_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { Append("^3"); DeactivateShift(); }
            else Append("^");
        }

        private void BtnSq_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive) { Append("^(-1)"); DeactivateShift(); }
            else Append("^2");
        }

        private void BtnRecip_Click(object sender, RoutedEventArgs e) => Append("^(-1)");
        private void BtnAbs_Click(object sender, RoutedEventArgs e) => Append("abs(");

        private void BtnMC_Click(object sender, RoutedEventArgs e) { _memory = 0; RefreshUI(); }

        private void BtnMR_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _expression = ""; _justCalculated = false; _sdMode = false; }
            Append(_memory.ToString("G15"));
        }

        private void BtnMPlus_Click(object sender, RoutedEventArgs e)
        {
            _memory += _justCalculated ? _lastResult : TryParseDisplay();
            RefreshUI();
        }

        private void BtnMMinus_Click(object sender, RoutedEventArgs e)
        {
            _memory -= _justCalculated ? _lastResult : TryParseDisplay();
            RefreshUI();
        }

        private void BtnMS_Click(object sender, RoutedEventArgs e)
        {
            _memory = _justCalculated ? _lastResult : TryParseDisplay();
            RefreshUI();
        }

        private double TryParseDisplay()
        {
            if (double.TryParse(tbDisplay.Text,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v))
                return v;
            return 0;
        }

        private void BtnBase_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState == BaseInputState.Idle)
            {
                _baseNumber = _expression.Trim();
                if (string.IsNullOrEmpty(_baseNumber) && _justCalculated)
                    _baseNumber = _lastResult.ToString("G15");
                _baseFrom = "";
                _baseToInput = "";
                _expression = "";
                _baseState = BaseInputState.WaitingForFromBase;
                PutToDisplay("");
                tbExpression.Text = $"Szám: [{_baseNumber}]";
                RefreshUI();
            }
            else if (_baseState == BaseInputState.WaitingForFromBase)
            {
                if (string.IsNullOrEmpty(_baseFrom)) return;
                _baseToInput = "";
                _expression = "";
                _baseState = BaseInputState.WaitingForToBase;
                PutToDisplay("");
                tbExpression.Text = $"{_baseNumber} [{_baseFrom}→?]";
                RefreshUI();
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
                        throw new Exception("Alap 2 és 36 közé kell essen");
                    bool negative = _baseNumber.StartsWith("-");
                    string numStr = negative ? _baseNumber[1..] : _baseNumber;
                    long number = Convert.ToInt64(numStr, fromB);
                    if (negative) number = -number;
                    string result = negative
                        ? "-" + Convert.ToString(Math.Abs(number), toB).ToUpper()
                        : Convert.ToString(number, toB).ToUpper();
                    tbExpression.Text = $"{_baseNumber}({_baseFrom})→({toB})";
                    PutToDisplay(result);
                    _expression = result;
                    _lastResult = number;
                    _justCalculated = true;
                }
                catch (Exception ex)
                {
                    PutToDisplay("Hiba");
                    tbExpression.Text = ex.Message;
                }
                _baseState = BaseInputState.Idle;
                _baseNumber = _baseFrom = _baseToInput = "";
                RefreshUI();
            }
        }

        private void BtnSD_Click(object sender, RoutedEventArgs e)
        {
            if (!_justCalculated) return;
            _sdMode = !_sdMode;
            PutToDisplay(_sdMode ? ToFraction(_lastResult) : FormatResult(_lastResult));
        }

        private string ToFraction(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return value.ToString();
            int sign = value < 0 ? -1 : 1;
            double absVal = Math.Abs(value);
            long bestNum = (long)Math.Round(absVal);
            long bestDen = 1;
            double bestErr = Math.Abs(absVal - bestNum);
            for (long d = 2; d <= 100000; d++)
            {
                long n = (long)Math.Round(absVal * d);
                double err = Math.Abs(absVal - (double)n / d);
                if (err < bestErr) { bestErr = err; bestNum = n; bestDen = d; }
                if (bestErr < 1e-12) break;
            }
            long g = GCD(Math.Abs(bestNum), bestDen);
            bestNum /= g; bestDen /= g;
            if (bestDen == 1) return (sign * bestNum).ToString();
            return $"{sign * bestNum}/{bestDen}";
        }

        private static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);

        private void BtnEquals_Click(object sender, RoutedEventArgs e) => Calculate();

        private void Calculate()
        {
            if (string.IsNullOrWhiteSpace(_expression)) return;
            try
            {
                double result = Evaluate(_expression);
                _lastResult = result;
                _sdMode = false;
                tbExpression.Text = _expression + " =";
                PutToDisplay(FormatResult(result));
                _justCalculated = true;
                RefreshUI();
            }
            catch (Exception ex)
            {
                PutToDisplay("Hiba");
                tbExpression.Text = ex.Message;
            }
        }

        private static string FormatResult(double r)
        {
            if (double.IsNaN(r)) return "Hiba";
            if (double.IsPositiveInfinity(r)) return "+∞";
            if (double.IsNegativeInfinity(r)) return "-∞";
            string s = r.ToString("G15");
            return s;
        }

        private double Evaluate(string expr)
        {
            var tokens = Tokenize(expr);
            int pos = 0;
            double val = ParseAddSub(tokens, ref pos);
            if (pos < tokens.Count) throw new Exception("Váratlan szimbólum: " + tokens[pos]);
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
                if (c == 'π') { list.Add("π"); i++; continue; }
                if (c == 'e' && (i + 1 >= expr.Length || !char.IsLetterOrDigit(expr[i + 1])))
                {
                    list.Add("e"); i++; continue;
                }
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
                if (c == '-' && i + 1 < expr.Length && char.IsDigit(expr[i + 1])
                    && (list.Count == 0 || list[^1] is "+" or "-" or "*" or "/" or "^" or "(" or ","))
                {
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
            while (p < t.Count && t[p] is "+" or "-")
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
            while (p < t.Count && t[p] is "*" or "/")
            {
                bool mul = t[p++] == "*";
                double right = ParsePow(t, ref p);
                if (!mul && right == 0) throw new DivideByZeroException("Nullával való osztás");
                left = mul ? left * right : left / right;
            }
            return left;
        }

        private double ParsePow(List<string> t, ref int p)
        {
            double left = ParseUnary(t, ref p);
            if (p < t.Count && t[p] == "^")
            {
                p++;
                double right = ParsePow(t, ref p);
                left = Math.Pow(left, right);
            }
            return left;
        }

        private double ParseUnary(List<string> t, ref int p)
        {
            if (p < t.Count && t[p] == "-") { p++; return -ParsePrimary(t, ref p); }
            if (p < t.Count && t[p] == "+") { p++; }
            return ParsePrimary(t, ref p);
        }

        private double ParsePrimary(List<string> t, ref int p)
        {
            if (p >= t.Count) throw new Exception("Váratlan vég");
            string tok = t[p];

            if (tok == "π") { p++; return Math.PI; }
            if (tok == "e") { p++; return Math.E; }

            if (double.TryParse(tok,
                System.Globalization.NumberStyles.Any,
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

            if (e.Key >= Key.D0 && e.Key <= Key.D9 && !shift)
            { Append(((int)(e.Key - Key.D0)).ToString()); e.Handled = true; return; }
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            { Append(((int)(e.Key - Key.NumPad0)).ToString()); e.Handled = true; return; }

            switch (e.Key)
            {
                case Key.Add: Append("+"); break;
                case Key.Subtract: Append("-"); break;
                case Key.Multiply: Append("*"); break;
                case Key.Divide: Append("/"); break;
                case Key.Decimal: BtnDot_Click(sender, e); break;
                case Key.OemPeriod: BtnDot_Click(sender, e); break;
                case Key.OemComma: BtnDot_Click(sender, e); break;
                case Key.Enter: Calculate(); break;
                case Key.Return: Calculate(); break;
                case Key.Back: BtnDel_Click(sender, e); break;
                case Key.Escape: BtnAC_Click(sender, e); break;
                case Key.Delete: BtnAC_Click(sender, e); break;
                case Key.OemOpenBrackets: Append("("); break;
                case Key.OemCloseBrackets: Append(")"); break;
                case Key.D9 when shift: Append("("); break;
                case Key.D0 when shift: Append(")"); break;
                case Key.OemPlus when !shift: Append("="); Calculate(); break;
                case Key.OemMinus: Append("-"); break;
                case Key.P when shift: Append("π"); break;
                case Key.E when !shift: Append("e"); break;
                case Key.S when shift: _shiftActive = !_shiftActive; SyncShiftButtons(); RefreshUI(); break;
            }
            e.Handled = true;
        }
    }
}