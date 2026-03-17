using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            tbExpression.Text = _expression;
            if (_baseState == BaseInputState.WaitingForFromBase)
                tbBaseStatus.Text = $"Szám: {_baseNumber} | Add meg az alapot (pl. 10)";
            else if (_baseState == BaseInputState.WaitingForToBase)
                tbBaseStatus.Text = $"Szám: {_baseNumber} | Alap: {_baseFrom} | Add meg a célrendszert";
            else
                tbBaseStatus.Text = "";
            tbShiftIndicator.Text = _shiftActive ? "SHIFT" : "";
            tbAngleMode.Text = _isRadians ? "RAD" : "DEG";
            tbMemIndicator.Text = _memory != 0 ? "M" : "";
        }

        private void SetDisplay(string val)
        {
            tbDisplay.Text = val;
        }

        private void AppendToExpression(string s)
        {
            if (_justCalculated)
            {
                bool isOp = s == "+" || s == "-" || s == "*" || s == "/" || s == "^";
                if (!isOp) _expression = "";
                _justCalculated = false;
            }
            _expression += s;
            SetDisplay(_expression);
            UpdateDisplay();
        }

        private void BtnNum_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string digit = btn.Tag.ToString()!;

            if (_baseState == BaseInputState.WaitingForFromBase)
            {
                _baseFrom += digit;
                tbDisplay.Text = _baseFrom;
                UpdateDisplay();
                return;
            }
            if (_baseState == BaseInputState.WaitingForToBase)
            {
                AppendToExpression(digit);
                return;
            }

            if (_justCalculated)
            {
                _expression = "";
                _justCalculated = false;
            }

            AppendToExpression(digit);
        }

        private void BtnDot_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _expression = ""; _justCalculated = false; }
            if (!_expression.Contains(".") || _expression.LastIndexOfAny(new[]{'+','-','*','/'}) > _expression.LastIndexOf('.'))
                AppendToExpression(".");
        }

        private void BtnOp_Click(object sender, RoutedEventArgs e)
        {
            string op = ((Button)sender).Tag.ToString()!;
            if (_baseState != BaseInputState.Idle) return;
            _justCalculated = false;
            AppendToExpression(op);
        }

        private void BtnParen_Click(object sender, RoutedEventArgs e)
        {
            string p = ((Button)sender).Tag.ToString()!;
            AppendToExpression(p);
        }

        private void BtnAC_Click(object sender, RoutedEventArgs e)
        {
            _expression = "";
            _baseState = BaseInputState.Idle;
            _baseNumber = "";
            _baseFrom = "";
            _shiftActive = false;
            _justCalculated = false;
            _sdMode = false;
            SetDisplay("0");
            UpdateDisplay();
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (_expression.Length > 0)
            {
                _expression = _expression[..^1];
                SetDisplay(_expression.Length > 0 ? _expression : "0");
                UpdateDisplay();
            }
        }

        private void BtnShift_Click(object sender, RoutedEventArgs e)
        {
            _shiftActive = !_shiftActive;
            btnShift.Background = _shiftActive
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xf5, 0x9e, 0x0b))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x78, 0x35, 0x0f));
            btnSin.Content = _shiftActive ? "sin⁻¹" : "sin";
            btnCos.Content = _shiftActive ? "cos⁻¹" : "cos";
            btnTan.Content = _shiftActive ? "tan⁻¹" : "tan";
            btnLog.Content = _shiftActive ? "10ˣ" : "log";
            btnLn.Content = _shiftActive ? "eˣ" : "ln";
            btnSqrt.Content = _shiftActive ? "ⁿ√x" : "ⁿ√x";
            btnPow.Content = _shiftActive ? "x³" : "xʸ";
            btnSq.Content = _shiftActive ? "x⁻¹" : "x²";
            UpdateDisplay();
        }

        private void BtnDegRad_Click(object sender, RoutedEventArgs e)
        {
            _isRadians = !_isRadians;
            UpdateDisplay();
        }

        private void BtnTrig_Click(object sender, RoutedEventArgs e)
        {
            string fn = ((Button)sender).Tag.ToString()!;
            if (_shiftActive)
            {
                fn = fn switch { "sin" => "asin", "cos" => "acos", "tan" => "atan", _ => fn };
                _shiftActive = false;
                BtnShift_Click(sender, e);
            }
            AppendToExpression(fn + "(");
        }

        private void BtnPi_Click(object sender, RoutedEventArgs e)
        {
            AppendToExpression("π");
        }

        private void BtnE_Click(object sender, RoutedEventArgs e)
        {
            AppendToExpression("e");
        }

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                AppendToExpression("10^");
                _shiftActive = false;
                BtnShift_Click(sender, e);
            }
            else
            {
                AppendToExpression("log(");
            }
        }

        private void BtnLn_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                AppendToExpression("e^");
                _shiftActive = false;
                BtnShift_Click(sender, e);
            }
            else
            {
                AppendToExpression("ln(");
            }
        }

        private void BtnSqrt_Click(object sender, RoutedEventArgs e)
        {
            AppendToExpression("sqrt(");
        }

        private void BtnPow_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                AppendToExpression("^");
                AppendToExpression("3");
                _shiftActive = false;
                BtnShift_Click(sender, e);
            }
            else
            {
                AppendToExpression("^");
            }
        }

        private void BtnSq_Click(object sender, RoutedEventArgs e)
        {
            if (_shiftActive)
            {
                AppendToExpression("^");
                AppendToExpression("-1");
                _shiftActive = false;
                BtnShift_Click(sender, e);
            }
            else
            {
                AppendToExpression("^2");
            }
        }

        private void BtnRecip_Click(object sender, RoutedEventArgs e)
        {
            AppendToExpression("^(-1)");
        }

        private void BtnAbs_Click(object sender, RoutedEventArgs e)
        {
            AppendToExpression("abs(");
        }

        private void BtnMC_Click(object sender, RoutedEventArgs e)
        {
            _memory = 0;
            UpdateDisplay();
        }

        private void BtnMR_Click(object sender, RoutedEventArgs e)
        {
            if (_justCalculated) { _expression = ""; _justCalculated = false; }
            AppendToExpression(_memory.ToString("G15"));
        }

        private void BtnMPlus_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(tbDisplay.Text, out double v))
                _memory += v;
            else if (_justCalculated)
                _memory += _lastResult;
            UpdateDisplay();
        }

        private void BtnMMinus_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(tbDisplay.Text, out double v))
                _memory -= v;
            else if (_justCalculated)
                _memory -= _lastResult;
            UpdateDisplay();
        }

        private void BtnMS_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(tbDisplay.Text, out double v))
                _memory = v;
            else if (_justCalculated)
                _memory = _lastResult;
            UpdateDisplay();
        }

        private void BtnBase_Click(object sender, RoutedEventArgs e)
        {
            if (_baseState == BaseInputState.Idle)
            {
                _baseNumber = _expression.Trim();
                _expression = "";
                _baseFrom = "";
                _baseState = BaseInputState.WaitingForFromBase;
                SetDisplay("");
                UpdateDisplay();
            }
            else if (_baseState == BaseInputState.WaitingForFromBase)
            {
                _baseFrom = tbDisplay.Text.Trim();
                _expression = "";
                _baseState = BaseInputState.WaitingForToBase;
                SetDisplay("");
                UpdateDisplay();
            }
            else if (_baseState == BaseInputState.WaitingForToBase)
            {
                string toBase = _expression.Trim();
                try
                {
                    int fromB = int.Parse(_baseFrom);
                    int toB = int.Parse(toBase);
                    long number = Convert.ToInt64(_baseNumber, fromB);
                    string result = Convert.ToString(number, toB).ToUpper();
                    SetDisplay(result);
                    _expression = result;
                    _justCalculated = true;
                }
                catch
                {
                    SetDisplay("Hiba");
                }
                _baseState = BaseInputState.Idle;
                _baseNumber = "";
                _baseFrom = "";
                UpdateDisplay();
            }
        }

        private void BtnSD_Click(object sender, RoutedEventArgs e)
        {
            if (!_justCalculated) return;
            _sdMode = !_sdMode;
            if (_sdMode)
            {
                var frac = ToFraction(_lastResult);
                SetDisplay(frac);
            }
            else
            {
                SetDisplay(FormatResult(_lastResult));
            }
        }

        private string ToFraction(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return value.ToString();
            const int maxDenominator = 10000;
            int sign = value < 0 ? -1 : 1;
            double absVal = Math.Abs(value);
            long bestNum = (long)Math.Round(absVal);
            long bestDen = 1;
            double bestError = Math.Abs(absVal - bestNum);
            for (long den = 2; den <= maxDenominator; den++)
            {
                long num = (long)Math.Round(absVal * den);
                double error = Math.Abs(absVal - (double)num / den);
                if (error < bestError)
                {
                    bestError = error;
                    bestNum = num;
                    bestDen = den;
                }
                if (bestError < 1e-10) break;
            }
            if (bestDen == 1) return (sign * bestNum).ToString();
            return $"{sign * bestNum}/{bestDen}";
        }

        private void BtnEquals_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            try
            {
                double result = EvaluateExpression(_expression);
                _lastResult = result;
                _sdMode = false;
                SetDisplay(FormatResult(result));
                _justCalculated = true;
                UpdateDisplay();
            }
            catch
            {
                SetDisplay("Hiba");
            }
        }

        private string FormatResult(double result)
        {
            if (double.IsNaN(result)) return "Hiba";
            if (double.IsInfinity(result)) return result > 0 ? "+∞" : "-∞";
            string s = result.ToString("G15");
            return s;
        }

        private double EvaluateExpression(string expr)
        {
            var tokens = Tokenize(expr);
            int pos = 0;
            return ParseAddSub(tokens, ref pos);
        }

        private List<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            int i = 0;
            expr = expr.Replace(" ", "");
            while (i < expr.Length)
            {
                char c = expr[i];
                if (c == 'π') { tokens.Add("π"); i++; continue; }
                if (c == 'e' && (i + 1 >= expr.Length || !char.IsLetter(expr[i + 1])))
                {
                    bool isPartOfFunc = i > 0 && char.IsLetter(expr[i - 1]);
                    if (!isPartOfFunc) { tokens.Add("e"); i++; continue; }
                }
                if (char.IsLetter(c))
                {
                    var sb = new StringBuilder();
                    while (i < expr.Length && (char.IsLetter(expr[i]) || expr[i] == '_'))
                        sb.Append(expr[i++]);
                    tokens.Add(sb.ToString());
                    continue;
                }
                if (char.IsDigit(c) || c == '.')
                {
                    var sb = new StringBuilder();
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                        sb.Append(expr[i++]);
                    tokens.Add(sb.ToString());
                    continue;
                }
                tokens.Add(c.ToString());
                i++;
            }
            return tokens;
        }

        private double ParseAddSub(List<string> tokens, ref int pos)
        {
            double left = ParseMulDiv(tokens, ref pos);
            while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
            {
                string op = tokens[pos++];
                double right = ParseMulDiv(tokens, ref pos);
                left = op == "+" ? left + right : left - right;
            }
            return left;
        }

        private double ParseMulDiv(List<string> tokens, ref int pos)
        {
            double left = ParsePow(tokens, ref pos);
            while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/"))
            {
                string op = tokens[pos++];
                double right = ParsePow(tokens, ref pos);
                left = op == "*" ? left * right : left / right;
            }
            return left;
        }

        private double ParsePow(List<string> tokens, ref int pos)
        {
            double left = ParseUnary(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == "^")
            {
                pos++;
                double right = ParsePow(tokens, ref pos);
                left = Math.Pow(left, right);
            }
            return left;
        }

        private double ParseUnary(List<string> tokens, ref int pos)
        {
            if (pos < tokens.Count && tokens[pos] == "-")
            {
                pos++;
                return -ParsePrimary(tokens, ref pos);
            }
            if (pos < tokens.Count && tokens[pos] == "+")
            {
                pos++;
            }
            return ParsePrimary(tokens, ref pos);
        }

        private double ParsePrimary(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count) throw new Exception("Váratlan vég");
            string tok = tokens[pos];

            if (tok == "π") { pos++; return Math.PI; }
            if (tok == "e") { pos++; return Math.E; }

            if (double.TryParse(tok, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double num))
            {
                pos++;
                return num;
            }

            if (tok == "(")
            {
                pos++;
                double val = ParseAddSub(tokens, ref pos);
                if (pos < tokens.Count && tokens[pos] == ")")
                    pos++;
                return val;
            }

            string fnName = tok.ToLower();
            if (fnName is "sin" or "cos" or "tan" or "asin" or "acos" or "atan"
                or "ln" or "log" or "sqrt" or "abs")
            {
                pos++;
                if (pos < tokens.Count && tokens[pos] == "(")
                {
                    pos++;
                    if (fnName == "log" && pos < tokens.Count && tokens[pos] != ")")
                    {
                        int savedPos = pos;
                        try
                        {
                            double baseVal = ParseAddSub(tokens, ref pos);
                            if (pos < tokens.Count && tokens[pos] == ",")
                            {
                                pos++;
                                double argVal = ParseAddSub(tokens, ref pos);
                                if (pos < tokens.Count && tokens[pos] == ")")
                                    pos++;
                                return Math.Log(argVal) / Math.Log(baseVal);
                            }
                            pos = savedPos;
                        }
                        catch { pos = savedPos; }
                    }
                    double arg = ParseAddSub(tokens, ref pos);
                    if (pos < tokens.Count && tokens[pos] == ")") pos++;
                    return ApplyFunction(fnName, arg);
                }
                return ApplyFunction(fnName, ParsePrimary(tokens, ref pos));
            }

            throw new Exception($"Ismeretlen token: {tok}");
        }

        private double ApplyFunction(string fn, double arg)
        {
            double a = _isRadians ? arg : arg * Math.PI / 180;
            double aOut = _isRadians ? 1.0 : 180.0 / Math.PI;
            return fn switch
            {
                "sin" => Math.Sin(a),
                "cos" => Math.Cos(a),
                "tan" => Math.Tan(a),
                "asin" => Math.Asin(arg) * aOut,
                "acos" => Math.Acos(arg) * aOut,
                "atan" => Math.Atan(arg) * aOut,
                "ln" => Math.Log(arg),
                "log" => Math.Log10(arg),
                "sqrt" => Math.Sqrt(arg),
                "abs" => Math.Abs(arg),
                _ => throw new Exception($"Ismeretlen függvény: {fn}")
            };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                string d = ((int)(e.Key - Key.D0)).ToString();
                AppendToExpression(d);
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                string d = ((int)(e.Key - Key.NumPad0)).ToString();
                AppendToExpression(d);
            }
            else if (e.Key == Key.Add || (e.Key == Key.OemPlus && !Keyboard.IsKeyDown(Key.LeftShift)))
                AppendToExpression("+");
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
                AppendToExpression("-");
            else if (e.Key == Key.Multiply)
                AppendToExpression("*");
            else if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
                AppendToExpression("/");
            else if (e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.OemComma)
                BtnDot_Click(sender, e);
            else if (e.Key == Key.Enter || e.Key == Key.Return)
                Calculate();
            else if (e.Key == Key.Back)
                BtnDel_Click(sender, e);
            else if (e.Key == Key.Escape)
                BtnAC_Click(sender, e);
            else if (e.Key == Key.OemOpenBrackets)
                AppendToExpression("(");
            else if (e.Key == Key.OemCloseBrackets)
                AppendToExpression(")");
        }
    }
}