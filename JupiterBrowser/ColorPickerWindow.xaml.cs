using System.Windows;
using System.Windows.Controls;

namespace JupiterBrowser
{
    public partial class ColorPickerWindow : Window
    {
        public string SelectedBackgroundColor { get; private set; }
        public string SelectedTextColor { get; private set; }

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedBackgroundColor = (BackgroundColorComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            DialogResult = true;
        }
    }
}
