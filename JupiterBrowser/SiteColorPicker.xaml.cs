using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JupiterBrowser
{
    public partial class SiteColorPicker : Window
    {
        private bool isDragging = false;
        private Point clickPosition;
        public string SelectedColorHex { get; private set; }
        public string SelectedTarget { get; set; }

        public SiteTheme siteTheme = new SiteTheme();

        public event Action<SiteTheme> OnColorsSelected;

        public SiteColorPicker(string url)
        {
            InitializeComponent();
            siteTheme.url = url;
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
            // Armazena a cor selecionada no siteTheme
            if (SelectedTarget == "Foreground")
            {
                siteTheme.ForegroundColor = SelectedColorHex;
            }
            else if (SelectedTarget == "Background")
            {
                siteTheme.BackgroundColor = SelectedColorHex;
            }

            OnColorsSelected?.Invoke(siteTheme);
            DialogResult = true;
            Close();
        }

        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            string filePath = "siteColors.json";

            // Carregar o conteúdo do JSON
            string json = System.IO.File.ReadAllText(filePath);
            var siteColors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SiteColorInfo>>(json);

            // Encontrar o item a ser removido
            var itemToRemove = siteColors.FirstOrDefault(sc => sc.Url.Equals(siteTheme.url, StringComparison.OrdinalIgnoreCase));
            if (itemToRemove != null)
            {
                siteColors.Remove(itemToRemove);

                // Salvar a lista modificada de volta no arquivo JSON
                string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(siteColors, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(filePath, updatedJson);
                ToastWindow.Show("Colors restored, please reload the page.");
            }
        }

        private void ColorTargetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorTargetComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();
                SelectedTarget = selectedContent;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class SiteTheme
    {
        public string url { get; set; }
        public string ForegroundColor { get; set; }
        public string BackgroundColor { get; set; }
    }
}
