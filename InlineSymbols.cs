using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Szamologep
{
    public class InlineRootSymbol : Canvas
    {
        public InlineRootSymbol()
        {
            Width = 38; Height = 26;
            var n = new TextBlock { Text = "n", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(n, 0); SetTop(n, 0); Children.Add(n);
            var hook = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)),
                StrokeThickness = 1.4,
                Points = new PointCollection { new Point(7, 16), new Point(11, 22), new Point(16, 4), new Point(36, 4) }
            };
            Children.Add(hook);
            var x = new TextBlock { Text = "x", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(x, 18); SetTop(x, 6); Children.Add(x);
        }
    }

    public class InlineLogSymbol : Canvas
    {
        public InlineLogSymbol()
        {
            Width = 38; Height = 26;
            var log = new TextBlock { Text = "log", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(log, 0); SetTop(log, 2); Children.Add(log);
            var sub = new TextBlock { Text = "x", FontSize = 8, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(sub, 24); SetTop(sub, 15); Children.Add(sub);
            var y = new TextBlock { Text = "(y)", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(y, 28); Children.Add(y);
        }
    }

    public class InlinePowSymbol : Canvas
    {
        public InlinePowSymbol()
        {
            Width = 32; Height = 26;
            var x = new TextBlock { Text = "x", FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(x, 0); SetTop(x, 7); Children.Add(x);
            var y = new TextBlock { Text = "y", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(y, 12); SetTop(y, 0); Children.Add(y);
        }
    }

    public class InlineSqSymbol : Canvas
    {
        public InlineSqSymbol()
        {
            Width = 28; Height = 26;
            var x = new TextBlock { Text = "x", FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(x, 0); SetTop(x, 7); Children.Add(x);
            var two = new TextBlock { Text = "2", FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(0x6f, 0xcf, 0x97)), FontFamily = new FontFamily("Consolas") };
            SetLeft(two, 12); SetTop(two, 0); Children.Add(two);
        }
    }
}