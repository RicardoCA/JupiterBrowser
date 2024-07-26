using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using Xceed.Wpf.Toolkit;
using System.Net.Http;
using System.Windows.Navigation;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media;
using WpfButton = System.Windows.Controls.Button;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;
//using Wpf.Ui.Controls; // Para as cores do WPF

namespace JupiterBrowser
{
    public partial class MainWindow : Window
    {
        private string VERSION = "0.9.1";
        public ObservableCollection<TabItem> Tabs { get; set; }
        public ObservableCollection<TabItem> PinnedTabs { get; set; }
        private TabItem _draggedItem;
        private Point _startPoint;
        private bool isFullScreen = false;
        private DispatcherTimer _musicTitleUpdateTimer;
        


        public int id = 1;

        public MainWindow()
        {
            InitializeComponent();
            Tabs = new ObservableCollection<TabItem>();
            TabListBox.ItemsSource = Tabs;
            this.DataContext = this;
            this.KeyDown += Window_KeyDown;
            PinnedTabs = new ObservableCollection<TabItem>
            {
                new TabItem { TabName = "Reddit", LogoUrl = "https://www.reddit.com/favicon.ico", url= "https://www.reddit.com/" },
                new TabItem { TabName = "Youtube Music", LogoUrl = "https://music.youtube.com/favicon.ico", url= "https://music.youtube.com" },
                new TabItem { TabName = "Google", LogoUrl = "https://www.google.com/favicon.ico", url= "https://www.google.com" }
            };
            PinnedTabsListBox.ItemsSource = PinnedTabs;
            // Inicializa o timer
            _musicTitleUpdateTimer = new DispatcherTimer();
            _musicTitleUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            _musicTitleUpdateTimer.Tick += MusicTitleUpdateTimer_Tick;
            OpenStartPage();
            CleanUpdates();
        }

        private void ContactJupiter_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabWithUrl("https://7olt4aho8ub.typeform.com/to/CNB9ea1Z");
        }

        private void adBlock_Click(object sender, RoutedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if(selectedTab.adBlock == false)
                {
                    selectedTab.adBlock = true;
                    ToastWindow.Show("Native AdBlock enabled.");
                    selectedTab.WebView.Reload();
                }
                else
                {
                    selectedTab.adBlock = false;
                    selectedTab.WebView.Reload();
                    ToastWindow.Show("Native AdBlock disabled.");
                }
            }
            else
            {
                ToastWindow.Show("Select a tab for this.");
            }
        }

        private void CleanUpdates()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string targetDirectory = Path.Combine(baseDirectory, "JupiterBrowser");
            string zipFilePath = Path.Combine(baseDirectory, "JupiterBrowser.zip");
            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
        }

        static string GetServerVersion()
        {
            // Requisita a versão disponível no servidor
            using (WebClient client = new WebClient())
            {
                //https://drive.google.com/file/d/1sz4dx76iHLJ7gTl9tezuGc27_rbUsR8j/view?usp=drive_link
                //https://drive.google.com/uc?export=download&id=1sz4dx76iHLJ7gTl9tezuGc27_rbUsR8j
                return client.DownloadString("https://drive.google.com/uc?export=download&id=1sz4dx76iHLJ7gTl9tezuGc27_rbUsR8j").Trim();
            }
        }

        private void OpenStartPage()
        {
            string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "startpage.html");
            OpenNewTabWithUrl(htmlFilePath);
        }

        private async void MusicTitleUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateMiniPlayerVisibility();
        }

        private void PinnedTabsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PinnedTabsListBox.SelectedItem is TabItem selectedTab)
            {
                OpenNewTabWithUrl(selectedTab.url);
            }
            UpdateMiniPlayerVisibility();
        }
        private void OpenNewTabWithUrl(string url)
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            newTab.adBlock = false;
            Tabs.Add(newTab);
            id += 1;

            var webView = new WebView2();
            webView.Source = new System.Uri(url);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            newTab.WebView = webView;

            TabListBox.SelectedItem = newTab;
            UpdateMiniPlayerVisibility();
        }

        private async void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            var webView = sender as WebView2;
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            }
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (sender is WebView2 webView)
            {
                var uri = e.Request.Uri;
                if (uri.Contains("youtube.com") && !uri.Contains("ads") && !uri.Contains("adserver"))
                {
                    // Permitir recursos de youtube.com e music.youtube.com
                    return;
                }

                if (uri.Contains("ads") || uri.Contains("adserver") || uri.Contains("advertisement"))
                {
                    e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
                        null,  // Sem corpo de resposta
                        403,   // Código de status HTTP 403 (Forbidden)
                        "Blocked", // Descrição
                        "Content-Type: text/plain" // Cabeçalho de resposta
                    );
                }
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Cancela a abertura da nova janela
            e.Handled = true;

            // Obtém a URL solicitada
            string newUrl = e.Uri;

            // Abre uma nova aba com a URL
            OpenNewTabWithUrl(newUrl);
        }

        private void Pin()
        {
            if (PinnedTabs == null)
            {
                PinnedTabs = new ObservableCollection<TabItem>();
                PinnedTabsListBox.ItemsSource = PinnedTabs;
            }

            // Suponha que você tenha um método para obter a URL atual do WebView
            var (currentUrl, currentLogo) = GetCurrentWebViewUrl();

            // Verifique se o item já está nos PinnedTabs
            var existingTab = PinnedTabs.FirstOrDefault(tab => tab.url == currentUrl);
            if (existingTab != null)
            {
                // Remove o item existente
                PinnedTabs.Remove(existingTab);
            }
            else
            {
                // Adicione o novo item
                PinnedTabs.Add(new TabItem
                {
                    TabName = "New Site",
                    LogoUrl = currentLogo,
                    url = currentUrl
                });
            }
        }

        private (string, string) GetCurrentWebViewUrl()
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                string url = selectedTab.WebView.Source.ToString();
                string domain = new Uri(url).GetLeftPart(UriPartial.Authority); // Obtém o domínio da URL
                string faviconUrl = $"{domain}/favicon.ico"; // Construir a URL do favicon
                return (url, faviconUrl);
            }
            return (string.Empty, string.Empty);
        }

        private void OpenHistoric()
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            newTab.adBlock = false;
            Tabs.Add(newTab);
            id += 1;

            var webView = new WebView2();
            webView.Source = new System.Uri("edge://history");
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            newTab.WebView = webView;

            TabListBox.SelectedItem = newTab;
        }

        private void HistoricButton_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para o botão de configurações
            OpenHistoric();
        }

        

        private void OpenNewTab()
        {
            var urlInputDialog = new UrlInputDialog();
            if (urlInputDialog.ShowDialog() == true)
            {
                var newTab = new TabItem { TabName = "New Tab " + id };
                newTab.adBlock = false;
                Tabs.Add(newTab);
                id += 1;

                var webView = new WebView2();
                if(urlInputDialog.EnteredUrl.ToString().Equals("startpage"))
                {
                    string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "startpage.html");
                    webView.Source = new System.Uri(htmlFilePath);
                    webView.NavigationCompleted += WebView_NavigationCompleted;
                    webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                }
                else
                {
                    webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                    webView.NavigationCompleted += WebView_NavigationCompleted;
                    webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                }
                
                

           

                newTab.WebView = webView;

                TabListBox.SelectedItem = newTab;
                UpdateMiniPlayerVisibility();
            }
        }

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = TabListBox.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                // Atualiza a URL do WebView da guia selecionada
                if (selectedTab.WebView != null)
                {
                    string url = selectedTab.WebView.Source.ToString();
                    var urlInputDialog = new UrlInputDialog();
                    if (url.IndexOf("http") != -1)
                    {
                        urlInputDialog = new UrlInputDialog(url);
                    }


                    if (urlInputDialog.ShowDialog() == true)
                    {
                        // Verifica se há uma guia selecionada

                        selectedTab.WebView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                    }


                }
                else
                {
                    // Se o WebView não existe ainda, cria um novo
                    var urlInputDialog = new UrlInputDialog();
                    if (urlInputDialog.ShowDialog() == true)
                    {
                        var webView = new WebView2();
                        webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                        selectedTab.WebView = webView;
                    }
                }
            }
            else
            {
                ToastWindow.Show("Please select a tab to edit.");

            }



            UpdateMiniPlayerVisibility();
        }

        private void EditTabUrl()
        {


            var selectedTab = TabListBox.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                // Atualiza a URL do WebView da guia selecionada
                if (selectedTab.WebView != null)
                {
                    string url = selectedTab.WebView.Source.ToString();
                    var urlInputDialog = new UrlInputDialog();
                    if (url.IndexOf("http") != -1)
                    {
                        urlInputDialog = new UrlInputDialog(url);
                    }
                    
                    
                    if (urlInputDialog.ShowDialog() == true)
                    {
                        // Verifica se há uma guia selecionada
                        
                        selectedTab.WebView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                    }


                }
                else
                {
                    // Se o WebView não existe ainda, cria um novo
                    var urlInputDialog = new UrlInputDialog();
                    if (urlInputDialog.ShowDialog() == true)
                    {
                        var webView = new WebView2();
                        webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                        selectedTab.WebView = webView;
                    }
                }
            }
            else
            {
                ToastWindow.Show("Please select a tab to edit.");

            }


            
            UpdateMiniPlayerVisibility();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            string serverVersion = GetServerVersion();
            if (!serverVersion.Equals(VERSION))
            {
                Process.Start("Updater.exe");
            }
            else
            {
                ToastWindow.Show("You already have the latest version.");
            }
            
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTab();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenNewTab();
            }
            else if(e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                EditTabUrl();
            }
            else if(e.Key == Key.H && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenHistoric();
            }
            else if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Pin();
            }
            else if(e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SidebarToggle();
            }
            else if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CopyURL();   
            }
        }

        private void ShortCuts_Click(object sender, RoutedEventArgs e)
        {
            ToastWindow.Show("Ctrl + T (new tab)\nCtrl + L (edit tab url)\nCtrl + H (open historic)\nCtrl + D (Pin/Unpin)\nCtrl + S (toggle sidebar)\nCtrl + W (copy url)");
        }

        private void CopyURL()
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                string url = selectedTab.WebView.Source.ToString();
                if(url.IndexOf("http") != -1)
                {
                    Clipboard.SetText(url);
                    ToastWindow.Show("URL copied: " + url);
                }
                else
                {
                    ToastWindow.Show("You cannot be on a file or startpage.");
                }
                
            }
            else
            {
                ToastWindow.Show("No tab selected or no URL available.");
            }
        }

        private void SidebarToggle()
        {
            //Sidebar.Visibility = Sidebar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (isFullScreen)
            {
                // Restaurar o Border ao seu tamanho original
                Grid.SetColumn(ContentBorder, 1);
                ContentBorder.Margin = new Thickness(10);
                ContentBorder.CornerRadius = new CornerRadius(10);
                isFullScreen = false;
                Sidebar.Visibility = Visibility.Visible;
            }
            else
            {
                // Fazer o Border ocupar a tela toda
                Grid.SetColumn(ContentBorder, 0);
                Grid.SetColumnSpan(ContentBorder, 2);
                ContentBorder.Margin = new Thickness(0);
                ContentBorder.CornerRadius = new CornerRadius(0);
                isFullScreen = true;
                Sidebar.Visibility = Visibility.Collapsed;
            }
        }

        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            
            ToastWindow.Show("Press Ctrl + S to show sidebar again :D");
            SidebarToggle();
        }

        private void OpenBrowserMenu_Click(object sender, RoutedEventArgs e)
        {

            
            Button button = sender as Button;
            ContextMenu menu = this.FindResource("BrowserMenu") as ContextMenu;
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }

        private void QuitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SidebarThemeMenu_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow colorPickerWindow = new ColorPickerWindow();
            if (colorPickerWindow.ShowDialog() == true)
            {
                // Obtém as cores selecionadas
                string backgroundColor = colorPickerWindow.SelectedBackgroundColor;
                string textColor = colorPickerWindow.SelectedTextColor;

                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    Sidebar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    TabListBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    
                }

                


                
            }
        }

        private void ApplyThemeColors(System.Windows.Media.Color textColor, System.Windows.Media.Color backgroundColor)
        {
            // Aqui você pode aplicar as cores ao seu tema, por exemplo:
            // this.Foreground = new SolidColorBrush(textColor);
            // this.Background = new SolidColorBrush(backgroundColor);

            // Exemplo de aplicação às propriedades de um TextBlock e um Grid (ou outro controle que você deseja alterar)
            // textBlock.Foreground = new SolidColorBrush(textColor);
            // grid.Background = new SolidColorBrush(backgroundColor);

            // Exemplo para demonstração
            MessageBox.Show($"Texto: {textColor}, Fundo: {backgroundColor}");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if (selectedTab.WebView.CoreWebView2 != null && selectedTab.WebView.CoreWebView2.CanGoBack)
                {
                    selectedTab.WebView.CoreWebView2.GoBack();
                }
                
            }
        }
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if (selectedTab.WebView.CoreWebView2 != null && selectedTab.WebView.CoreWebView2.CanGoForward)
                {
                    selectedTab.WebView.CoreWebView2.GoForward();
                }
                
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if (selectedTab.WebView.CoreWebView2 != null)
                {
                    selectedTab.WebView.CoreWebView2.Reload();
                }

            }
        }

        private void MiniPlayer_Play_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMiniPlayerScript("if (document.querySelector('video')) { document.querySelector('video').play(); }");
        }

        private void MiniPlayer_Pause_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMiniPlayerScript("if (document.querySelector('video')) { document.querySelector('video').pause(); } ");
        }

        private void MiniPlayer_Stop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMiniPlayerScript("if (document.querySelector('video')) { document.querySelector('video').pause(); document.querySelector('video').currentTime = 0; } ");
        }

        private void MiniPlayer_Previus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMiniPlayerScript("if (document.querySelector('video')) { document.getElementsByClassName(\"previous-button style-scope ytmusic-player-bar\")[0].click(); }");
        }

        private void MiniPlayer_Next_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMiniPlayerScript("if (document.querySelector('video')) { document.getElementsByClassName(\"next-button style-scope ytmusic-player-bar\")[0].click(); }");
        }

        private async void ExecuteMiniPlayerScript(string script)
        {
            foreach (var tab in Tabs)
            {
                if (tab.WebView != null && (tab.WebView.Source.ToString().Contains("music.youtube.com") ))
                {
                    await tab.WebView.ExecuteScriptAsync(script);
                }
            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is WebView2 webView)
            {
                string adBlockScript = @"
                    var ads = document.querySelectorAll('[id^=ad], [class*=ad], [class*=banner]');
                    for (var i = 0; i < ads.length; i++) {
                        ads[i].style.display = 'none';
                    }
                ";
                if(webView.Source.ToString().IndexOf("youtube.com") == -1)
                {
                    if(TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
                    {
                        if(selectedTab.adBlock == true)
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync(adBlockScript);
                        }
                    }
                    
                }
                


                var title = await webView.CoreWebView2.ExecuteScriptAsync("document.title");
                title = title.Trim('"'); // Remove the surrounding quotes
                string url = webView.Source.ToString();
                string domain = new Uri(url).GetLeftPart(UriPartial.Authority); // Obtém o domínio da URL
                string faviconUrl = $"{domain}/favicon.ico";

                var tabItem = Tabs.FirstOrDefault(tab => tab.WebView == webView);
                if (tabItem != null)
                {
                    if(title.Length > 20)
                    {
                        tabItem.TabName = title.Substring(0,20);
                        
                        
                    }
                    else
                    {
                        tabItem.TabName = title;
                    }
                    tabItem.LogoUrl = faviconUrl;
                    if(url.IndexOf("http") != -1)
                    {
                        urlLabel.Text = url;
                    }
                    else
                    {
                        urlLabel.Text = "File or startpage";
                    }
                    
                }

                // Refresh the ListBox to update the displayed tab name
                var selectedIndex = TabListBox.SelectedIndex;
                TabListBox.ItemsSource = null;
                TabListBox.ItemsSource = Tabs;
                TabListBox.SelectedIndex = selectedIndex;

                UpdateMiniPlayerVisibility();

                
            }
        }

        private async void UpdateMiniPlayerVisibility()
        {
            try
            {
                bool hasMusicTab = Tabs.Any(tab => tab.WebView != null &&
                                                   tab.WebView.Source != null &&
                                                   tab.WebView.Source.ToString().Contains("music.youtube.com"));
                MiniPlayer.Visibility = hasMusicTab ? Visibility.Visible : Visibility.Collapsed;

                if (hasMusicTab)
                {
                    try
                    {
                        var musicTab = Tabs.First(tab => tab.WebView != null &&
                                                         tab.WebView.Source != null &&
                                                         tab.WebView.Source.ToString().Contains("music.youtube.com"));

                        // Espera até que o CoreWebView2 seja inicializado
                        await musicTab.WebView.EnsureCoreWebView2Async();

                        string script = "document.querySelector('title').textContent";

                        var musicTitle = await musicTab.WebView.CoreWebView2.ExecuteScriptAsync(script);
                        musicTitle = musicTitle.Trim('"'); // Remove the surrounding quotes
                        MusicTitle.Text = musicTitle;
                        _musicTitleUpdateTimer.Start();
                    }
                    catch (Exception ex)
                    {
                        // Tratar exceções aqui
                        MusicTitle.Text = "Erro ao obter o título";
                        _musicTitleUpdateTimer.Stop();
                    }
                }
                else
                {
                    MusicTitle.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Tratar exceções aqui, se necessário
            }
        }


        private void TabListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab)
            {
                ContentArea.Content = selectedTab.WebView;
                if (selectedTab.WebView.Source.ToString().IndexOf("http") != -1)
                {
                    urlLabel.Text = selectedTab.WebView.Source.ToString();
                }
                else
                {
                    urlLabel.Text = "File or startpage";
                }
                
            }
            UpdateMiniPlayerVisibility();
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tabItem = button.DataContext as TabItem;

            if (tabItem != null)
            {
                // Dispose of the WebView2 control
                tabItem.WebView?.Dispose();

                Tabs.Remove(tabItem);
            }
            UpdateMiniPlayerVisibility();
        }

        private void TabListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void TabListBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var listBoxItem = VisualUpwardSearch(e.OriginalSource as DependencyObject) as ListBoxItem;

                if (listBoxItem != null)
                {
                    _draggedItem = listBoxItem.DataContext as TabItem;
                    if (_draggedItem != null)
                    {
                        DragDrop.DoDragDrop(listBoxItem, _draggedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TabListBox_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void TabListBox_Drop(object sender, DragEventArgs e)
        {
            if (_draggedItem != null)
            {
                var listBox = sender as ListBox;
                var targetItem = VisualUpwardSearch(e.OriginalSource as DependencyObject) as ListBoxItem;
                var targetTab = targetItem?.DataContext as TabItem;

                if (targetTab != null && targetTab != _draggedItem)
                {
                    int oldIndex = Tabs.IndexOf(_draggedItem);
                    int newIndex = Tabs.IndexOf(targetTab);

                    if (oldIndex != -1 && newIndex != -1)
                    {
                        Tabs.Move(oldIndex, newIndex);
                    }
                }

                _draggedItem = null;
            }
        }

        private DependencyObject VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListBoxItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }
    }

    public class TabItem
    {
        public string TabName { get; set; }
        public WebView2 WebView { get; set; }

        public string LogoUrl { get; set; }

        public string url { get; set; }

        public bool adBlock { get; set; }
    }
}
