using System.Windows;
using System.Windows.Input;

namespace JupiterBrowser
{
    public partial class UrlInputDialog : Window
    {
        public string EnteredUrl { get; set; }

        public UrlInputDialog()
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            UrlTextBox.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape) {
                this.Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUrl();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUrl();
            }
        }

        private void ProcessUrl()
        {
            string url = UrlTextBox.Text;

            if (url.IndexOf("https://") == -1)
            {
                if (url.IndexOf(".com") != -1 || url.IndexOf(".net") != -1 || url.IndexOf(".gov") != -1 || url.IndexOf(".org") != -1)
                {
                    url = "https://" + url;
                }
                else
                {
                    if(url.IndexOf("edge://") == -1)
                    {
                        url = $"https://www.google.com/search?q={url}";
                    }
                    
                }
            }
            else
            {
                if (url.IndexOf(".com") == -1 && url.IndexOf(".net") == -1 && url.IndexOf(".gov") == -1 && url.IndexOf(".org") == -1)
                {
                    url = $"https://www.google.com/search?q={url}";
                }
            }

            EnteredUrl = url;
            DialogResult = true;
        }
    }
}
