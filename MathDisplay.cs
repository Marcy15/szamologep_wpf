using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Szamologep
{
    public class MathDisplay : Canvas
    {
        public static readonly DependencyProperty TokensProperty =
            DependencyProperty.Register(nameof(Tokens), typeof(List<MathToken>), typeof(MathDisplay),
                new PropertyMetadata(null, OnTokensChanged));

        public List<MathToken> Tokens
        {
            get => (List<MathToken>)GetValue(TokensProperty);
            set => SetValue(TokensProperty, value);
        }

        private static void OnTokensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MathDisplay)d).Render();

        public double FontSz { get; set; } = 28;
        public Brush TextColor { get; set; } = new SolidColorBrush(Color.FromRgb(0x00, 0xe6, 0x76));
        public Brush DimColor { get; set; } = new SolidColorBrush(Color.FromRgb(0x00, 0x99, 0x55));
        public Brush CursorColor { get; set; } = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff));

        private double _totalWidth = 0;
        private double _totalHeight = 0;

        public void Render()
        {
            Children.Clear();
            if (Tokens == null || Tokens.Count == 0)
            {
                var zero = MakeText("0", FontSz, DimColor);
                SetLeft(zero, 0); SetTop(zero, 0);
                Children.Add(zero);
                _totalWidth = zero.DesiredSize.Width + 20;
                _totalHeight = FontSz + 10;
                Width = _totalWidth;
                Height = _totalHeight;
                return;
            }

            double x = 4;
            double baseline = FontSz + 8;

            foreach (var tok in Tokens)
            {
                double w = RenderToken(tok, x, baseline);
                x += w;
            }

            var cur = new Border
            {
                Width = 2,
                Height = FontSz + 4,
                Background = CursorColor,
                Opacity = 1.0
            };
            SetLeft(cur, x + 1);
            SetTop(cur, baseline - FontSz);
            Children.Add(cur);

            _totalWidth = x + 16;
            _totalHeight = baseline + 12;
            Width = _totalWidth;
            Height = _totalHeight;
        }

        private double RenderToken(MathToken tok, double x, double baseline)
        {
            switch (tok.Type)
            {
                case TokenType.Number:
                case TokenType.Symbol:
                case TokenType.Op:
                case TokenType.Paren:
                {
                    var tb = MakeText(tok.Value, FontSz, TextColor);
                    tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    SetLeft(tb, x);
                    SetTop(tb, baseline - FontSz);
                    Children.Add(tb);
                    return tb.DesiredSize.Width + 2;
                }
                case TokenType.Root:
                    return RenderRoot(tok, x, baseline);
                case TokenType.Power:
                    return RenderPower(tok, x, baseline);
                case TokenType.Log:
                    return RenderLog(tok, x, baseline);
                case TokenType.Func:
                {
                    var tb = MakeText(tok.Value + "(", FontSz, DimColor);
                    tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    SetLeft(tb, x);
                    SetTop(tb, baseline - FontSz);
                    Children.Add(tb);
                    return tb.DesiredSize.Width + 2;
                }
                default:
                    return 0;
            }
        }

        private double RenderRoot(MathToken tok, double x, double baseline)
        {
            double smallSz = FontSz * 0.52;
            double bigSz = FontSz;

            var nTb = MakeText(tok.Degree ?? "2", smallSz, DimColor);
            nTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double nW = nTb.DesiredSize.Width;
            double nH = nTb.DesiredSize.Height;

            var argTb = MakeText(tok.Value, bigSz, TextColor);
            argTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double argW = argTb.DesiredSize.Width;

            double hookX = x + nW - 2;
            double topY = baseline - bigSz - 4;
            double barEndX = hookX + 18 + argW + 6;

            SetLeft(nTb, x);
            SetTop(nTb, topY + 2);
            Children.Add(nTb);

            var hook = new Polyline
            {
                Stroke = TextColor,
                StrokeThickness = 1.6,
                Points = new PointCollection
                {
                    new Point(hookX, baseline - bigSz * 0.35),
                    new Point(hookX + 7, baseline + 2),
                    new Point(hookX + 14, topY),
                    new Point(barEndX, topY)
                }
            };
            Children.Add(hook);

            SetLeft(argTb, hookX + 16);
            SetTop(argTb, baseline - bigSz);
            Children.Add(argTb);

            return barEndX - x + 4;
        }

        private double RenderPower(MathToken tok, double x, double baseline)
        {
            var baseTb = MakeText(tok.Value, FontSz, TextColor);
            baseTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double bW = baseTb.DesiredSize.Width;

            var expTb = MakeText(tok.Degree ?? "2", FontSz * 0.55, DimColor);
            expTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double eW = expTb.DesiredSize.Width;

            SetLeft(baseTb, x);
            SetTop(baseTb, baseline - FontSz);
            Children.Add(baseTb);

            SetLeft(expTb, x + bW + 1);
            SetTop(expTb, baseline - FontSz - FontSz * 0.3);
            Children.Add(expTb);

            return bW + eW + 4;
        }

        private double RenderLog(MathToken tok, double x, double baseline)
        {
            double smallSz = FontSz * 0.52;

            var logTb = MakeText("log", FontSz * 0.8, DimColor);
            logTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double logW = logTb.DesiredSize.Width;

            var subTb = MakeText(tok.Degree ?? "10", smallSz, DimColor);
            subTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double subW = subTb.DesiredSize.Width;

            var argTb = MakeText("(" + tok.Value, FontSz, TextColor);
            argTb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double argW = argTb.DesiredSize.Width;

            SetLeft(logTb, x);
            SetTop(logTb, baseline - FontSz * 0.8);
            Children.Add(logTb);

            SetLeft(subTb, x + logW);
            SetTop(subTb, baseline - smallSz + 2);
            Children.Add(subTb);

            SetLeft(argTb, x + logW + subW + 2);
            SetTop(argTb, baseline - FontSz);
            Children.Add(argTb);

            return logW + subW + argW + 4;
        }

        private TextBlock MakeText(string text, double size, Brush color)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = size,
                Foreground = color,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal
            };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return tb;
        }
    }

    public enum TokenType { Number, Symbol, Op, Paren, Root, Power, Log, Func }

    public class MathToken
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = "";
        public string? Degree { get; set; }
    }
}