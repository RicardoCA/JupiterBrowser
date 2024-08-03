using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace JupiterBrowser
{
    public partial class AnonymousWindow : Window
    {
        private WebView2 anonymousWebView;
        private string tempUserDataFolder;

        public AnonymousWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                tempUserDataFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempUserDataFolder);

                var envOptions = new CoreWebView2EnvironmentOptions("--incognito");
                var env = await CoreWebView2Environment.CreateAsync(null, tempUserDataFolder, envOptions);

                anonymousWebView = new WebView2();
                ContentArea.Content = anonymousWebView;
                await anonymousWebView.EnsureCoreWebView2Async(env);

                anonymousWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                anonymousWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                anonymousWebView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                anonymousWebView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                anonymousWebView.NavigationCompleted += AnonymousWebView_NavigationCompleted;

                

                anonymousWebView.Source = new Uri("https://www.google.com");
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Error initializing WebView2: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (anonymousWebView.CanGoBack)
            {
                anonymousWebView.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (anonymousWebView.CanGoForward)
            {
                anonymousWebView.GoForward();
            }
        }

        private void AnonymousWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            UpdateNavigationButtons();
            Dispatcher.Invoke(() =>
            {
                UrlTextBox.Text = anonymousWebView.Source.ToString();
            });
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                try
                {
                    anonymousWebView.Source = new Uri(UrlTextBox.Text);
                }
                catch (UriFormatException)
                {
                    ToastWindow.Show("Please enter a valid URL.");
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            BackButton.IsEnabled = anonymousWebView.CanGoBack;
            ForwardButton.IsEnabled = anonymousWebView.CanGoForward;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            try
            {
                if (Directory.Exists(tempUserDataFolder))
                {
                    Directory.Delete(tempUserDataFolder, true);
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}