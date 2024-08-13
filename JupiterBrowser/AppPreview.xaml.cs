using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para AppWhatsapp.xaml
    /// </summary>
    public partial class AppPreview : Window
    {
        WebView2 webView = new WebView2();
        private MainWindow _mainWindow;
        private bool isClosable = false;
        public AppPreview(MainWindow mainWindow, string url, bool isClosable = false)
        {
            InitializeComponent();
            if(isClosable is false)
            {
                copyUrlBtn.Visibility = Visibility.Collapsed;
            }

            _mainWindow = mainWindow;
            this.isClosable = isClosable;
            start(url);
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            if (webView.Source != null)
            {
                // Copia a URL para a área de transferência
                string url = webView.Source.ToString();
                Clipboard.SetText(url);
                ToastWindow.Show($"Url copied.");
            }
            else
            {
                ToastWindow.Show("No Url available to copy.");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isClosable == false)
            {
                this.Hide();
            }
            else
            {
                _mainWindow.UpdateIcon("Jupiter Browser");
                this.Close();
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.OpenNewTabWithUrl(webView.Source?.ToString());
            _mainWindow.UpdateIcon("Jupiter Browser");
            this.Hide();
            
        }

        public void start(string url)
        {

            
            webView.Source = new System.Uri(url);
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {

                // Subscribe to the NewWindowRequested event
                webView.CoreWebView2.NewWindowRequested += (sender, args) =>
                {
                    // Prevent the default new window action
                    args.Handled = true;

                    // Open the URL in a new tab in the main window
                    _mainWindow.OpenNewTabWithUrl(args.Uri);

                    // Optionally, hide the current window if needed
                    this.Hide();
                };


                webView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            };
            ContentArea.Content = webView;
            

        }
        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            var webView = sender as CoreWebView2;
            string newTitle = webView.DocumentTitle;
            _mainWindow.UpdateIcon("Jupiter Browser - " + newTitle);
        }

    }
}
