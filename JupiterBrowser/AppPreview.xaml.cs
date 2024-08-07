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
        public AppPreview(MainWindow mainWindow, string url)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            start(url);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
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
