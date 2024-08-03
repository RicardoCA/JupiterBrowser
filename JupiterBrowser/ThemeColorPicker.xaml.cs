using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JupiterBrowser
{
    public partial class ThemeColorPicker : Window
    {
        private bool isDragging = false;
        private Point clickPosition;
        public string SelectedColorHex { get; private set; }
        public string SelectedTarget { get; set; }

        public string SelectedBackgroundColor { get; private set; }

        public event Action<string> OnColorSelected;

        public ThemeColorPicker()
        {
            InitializeComponent();
            Loaded += ThemeColorPicker_Loaded;
        }

        private void ThemeColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColor(); // Chame UpdateColor ao carregar a janela
        }

        private void ColorBall_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(this);
            ColorBall.CaptureMouse();
        }

        private void ColorBall_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                double newLeft = Canvas.GetLeft(ColorBall) + (currentPosition.X - clickPosition.X);
                double newTop = Canvas.GetTop(ColorBall) + (currentPosition.Y - clickPosition.Y);

                // Ensure the ball stays within the bounds of the Canvas
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft > ColorCanvas.ActualWidth - ColorBall.Width) newLeft = ColorCanvas.ActualWidth - ColorBall.Width;
                if (newTop > ColorCanvas.ActualHeight - ColorBall.Height) newTop = ColorCanvas.ActualHeight - ColorBall.Height;

                Canvas.SetLeft(ColorBall, newLeft);
                Canvas.SetTop(ColorBall, newTop);

                clickPosition = currentPosition;

                UpdateColor();
            }
        }

        

        private void ColorBall_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ColorBall.ReleaseMouseCapture();
        }

        private void UpdateColor()
        {
            double position = Canvas.GetLeft(ColorBall) / (ColorCanvas.ActualWidth - ColorBall.Width);
            Color color = HsvToRgb(position * 360, 1, 1);
            ColorBall.Fill = new SolidColorBrush(color);
            SelectedColorHex = ColorToHex(color);
        }

        private Color HsvToRgb(double h, double s, double v)
        {
            // Ajuste para gerar cores mais escuras
            s = 0.8; // Saturação ajustada para garantir cores mais profundas
            v = 0.4; // Brilho reduzido para garantir que as cores sejam escuras

            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;

            double r = 0, g = 0, b = 0;
            if (h >= 0 && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (h >= 60 && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (h >= 120 && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (h >= 180 && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (h >= 240 && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (h >= 300 && h < 360)
            {
                r = c; g = 0; b = x;
            }

            return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }


        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void ApplyClick(object sender, RoutedEventArgs e)
        {
            SelectedBackgroundColor = SelectedColorHex;
            OnColorSelected?.Invoke(SelectedBackgroundColor);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            
            Close();
        }
    }
}
