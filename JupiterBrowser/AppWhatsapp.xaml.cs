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
    public partial class AppWhatsapp : Window
    {
        private MainWindow _mainWindow;
        public AppWhatsapp(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public void start()
        {
            var webView = new WebView2();
            webView.Source = new System.Uri("https://web.whatsapp.com");
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
