using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace JupiterBrowser
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TabItem> Tabs { get; set; }
        public ObservableCollection<TabItem> PinnedTabs { get; set; }
        private TabItem _draggedItem;
        private Point _startPoint;
        private bool isFullScreen = false;



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
                new TabItem { TabName = "Google", LogoUrl = "https://www.google.com/favicon.ico", url= "https://www.google.com/" },
                new TabItem { TabName = "Terra", LogoUrl = "https://www.terra.com.br/favicon.ico", url= "https://www.terra.com.br" },
                new TabItem { TabName = "Terra", LogoUrl = "https://www.chatgpt.com/favicon.ico", url= "https://www.chatgpt.com" }
            };
            PinnedTabsListBox.ItemsSource = PinnedTabs;
        }

        private void PinnedTabsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PinnedTabsListBox.SelectedItem is TabItem selectedTab)
            {
                OpenNewTabWithUrl(selectedTab.url);
            }
        }
        private void OpenNewTabWithUrl(string url)
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            Tabs.Add(newTab);
            id += 1;

            var webView = new WebView2();
            webView.Source = new System.Uri(url);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            newTab.WebView = webView;

            TabListBox.SelectedItem = newTab;
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
            Tabs.Add(newTab);
            id += 1;

            var webView = new WebView2();
            webView.Source = new System.Uri("edge://history");
            webView.NavigationCompleted += WebView_NavigationCompleted;
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
                Tabs.Add(newTab);
                id += 1;

                var webView = new WebView2();
                webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                webView.NavigationCompleted += WebView_NavigationCompleted;
                

           

                newTab.WebView = webView;

                TabListBox.SelectedItem = newTab;
            }
        }

        private void EditTabUrl()
        {
            var urlInputDialog = new UrlInputDialog();
            if (urlInputDialog.ShowDialog() == true)
            {
                // Verifica se há uma guia selecionada
                var selectedTab = TabListBox.SelectedItem as TabItem;
                if (selectedTab != null)
                {
                    // Atualiza a URL do WebView da guia selecionada
                    if (selectedTab.WebView != null)
                    {
                        selectedTab.WebView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        urlInputDialog.EnteredUrl = selectedTab.WebView.Source.ToString();
                    }
                    else
                    {
                        // Se o WebView não existe ainda, cria um novo
                        var webView = new WebView2();
                        webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        selectedTab.WebView = webView;
                    }
                }
                else
                {
                    MessageBox.Show("Por favor, selecione uma guia para editar.");
                }
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

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is WebView2 webView)
            {
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

                }

                // Refresh the ListBox to update the displayed tab name
                var selectedIndex = TabListBox.SelectedIndex;
                TabListBox.ItemsSource = null;
                TabListBox.ItemsSource = Tabs;
                TabListBox.SelectedIndex = selectedIndex;
            }
        }

        private void TabListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab)
            {
                ContentArea.Content = selectedTab.WebView;
            }
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
    }
}
