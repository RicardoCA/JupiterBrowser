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
using Newtonsoft.Json;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using Microsoft.VisualBasic;
//using Wpf.Ui.Controls; // Para as cores do WPF

namespace JupiterBrowser
{
    public partial class MainWindow : Window
    {
        private string VERSION = "0.17";
        public ObservableCollection<TabItem> Tabs { get; set; }
        public ObservableCollection<TabItem> PinnedTabs { get; set; }
        private TabItem _draggedItem;
        private bool _isDragging;
        private Point _dragStartPoint;
        private bool isFullScreen = false;
        private DispatcherTimer _musicTitleUpdateTimer;
        private string prompt = "";
        private DispatcherTimer _titleUpdateTimer;

        public int id = 1;

        public MainWindow()
        {
            InitializeComponent();
            Tabs = new ObservableCollection<TabItem>();
            TabListBox.ItemsSource = Tabs;
            this.DataContext = this;
            this.KeyDown += Window_KeyDown;
            this.Closing += MainWindow_Closing;
            // Inicializa o timer
            _musicTitleUpdateTimer = new DispatcherTimer();
            _musicTitleUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            _musicTitleUpdateTimer.Tick += MusicTitleUpdateTimer_Tick;
            OpenStartPage();
            CleanUpdates();
            LoadSidebarColor();
            LoadPinneds();
            LoadTabsClosed();

            // Inicializa o timer de atualização de títulos
            _titleUpdateTimer = new DispatcherTimer();
            _titleUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            _titleUpdateTimer.Tick += async (s, e) => await UpdateTabTitlesAsync();
            _titleUpdateTimer.Start();
        }

        private void TabListRename_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = GetClickedTabItem(e);
            if (clickedItem != null)
            {
                // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia
                string currentName = clickedItem.TabName;
                PromptWindow promptWindow = new PromptWindow(currentName, "Rename Tab:");
                if(promptWindow.ShowDialog() == true)
                {
                    if(promptWindow.UserInput.Length > 0)
                    {
                        string newName = promptWindow.UserInput;
                        // Atualiza o nome da guia se o novo nome não for nulo ou vazio
                        if (!string.IsNullOrEmpty(newName))
                        {
                            clickedItem.TabName = newName.Length > 18 ? newName.Substring(0, 18) : newName;
                            clickedItem.FullTabName = newName;
                            clickedItem.isRenamed = true;
                        }
                    }
                    else
                    {
                        clickedItem.isRenamed = false;
                    }
                    
                    

                    
                }

                
            }
        }
        private TabItem GetClickedTabItem(MouseButtonEventArgs e)
        {
            // Obtém o elemento visual onde ocorreu o clique
            var clickedElement = e.OriginalSource as FrameworkElement;
            if (clickedElement != null)
            {
                // Navega na árvore visual até encontrar o ListBoxItem correspondente
                var listBoxItem = VisualUpwardSearch(clickedElement) as ListBoxItem;
                if (listBoxItem != null)
                {
                    // Retorna o DataContext, que deve ser o TabItem
                    return listBoxItem.DataContext as TabItem;
                }
            }
            return null;
        }

        private async Task UpdateTabTitlesAsync()
        {
            try
            {
                foreach (var tab in Tabs)
                {
                    if (tab.WebView != null && tab.WebView.CoreWebView2 != null)
                    {
                        try
                        {
                            string script = "document.title";
                            string title = await tab.WebView.ExecuteScriptAsync(script);
                            title = title.Trim('"'); // Remove as aspas ao redor

                            if (!string.IsNullOrEmpty(title) && title != tab.FullTabName)
                            {
                                if(tab.isRenamed == false)
                                {
                                    tab.FullTabName = title;
                                    tab.TabName = title.Length > 18 ? title.Substring(0, 18) : title;
                                }
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            // Tratar exceções, se necessário
                        }
                    }
                }

                // Atualiza a interface para refletir as mudanças nos títulos
                var selectedIndex = TabListBox.SelectedIndex;
                TabListBox.ItemsSource = null;
                TabListBox.ItemsSource = Tabs;
                TabListBox.SelectedIndex = selectedIndex;
            }
            catch(Exception ex)
            {
                return;
            }
            
        }

        private void SiteThemeMenu_Click(object sender, RoutedEventArgs e)
        {
            string url = "";
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                url = selectedTab.WebView.Source.ToString();
                var colorPicker = new SiteColorPicker(url);
                colorPicker.OnColorsSelected += ColorPicker_OnColorsSelected;
                colorPicker.ShowDialog();

            }
            else
            {
                ToastWindow.Show("Select a tab for this.");
            }
            
        }

        private void ColorPicker_OnColorsSelected(SiteTheme siteTheme)
        {
            string script = "";
            var siteColorInfos = ColorPersistence.LoadColors();

            if (!string.IsNullOrEmpty(siteTheme.ForegroundColor))
            {
                script += $"document.querySelectorAll('p, label').forEach(el => el.style.color = '{siteTheme.ForegroundColor}');";
            }

            if (!string.IsNullOrEmpty(siteTheme.BackgroundColor))
            {
                script += $"document.body.style.backgroundColor = '{siteTheme.BackgroundColor}';";
            }

            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if (script.Length > 0)
                {
                    selectedTab.WebView.ExecuteScriptAsync(script);

                    // Encontrar ou adicionar a informação do site
                    var siteInfo = siteColorInfos.Find(info => info.Url == selectedTab.WebView.Source.ToString());
                    if (siteInfo == null)
                    {
                        siteInfo = new SiteColorInfo
                        {
                            Url = selectedTab.WebView.Source.ToString(),
                            ForegroundColor = siteTheme.ForegroundColor,
                            BackgroundColor = siteTheme.BackgroundColor
                        };
                        siteColorInfos.Add(siteInfo);
                    }
                    else
                    {
                        // Atualizar as cores apenas se elas não forem nulas
                        if (!string.IsNullOrEmpty(siteTheme.ForegroundColor))
                        {
                            siteInfo.ForegroundColor = siteTheme.ForegroundColor;
                        }
                        if (!string.IsNullOrEmpty(siteTheme.BackgroundColor))
                        {
                            siteInfo.BackgroundColor = siteTheme.BackgroundColor;
                        }
                    }

                    // Salvar as informações atualizadas
                    ColorPersistence.SaveColors(siteColorInfos);
                }
            }
            else
            {
                ToastWindow.Show("Select a tab for this.");
            }
        }


        private void JupiterCard_Click(object sender,  RoutedEventArgs e)
        {
            JupiterCard jupiterCard = new JupiterCard();
            if(jupiterCard.ShowDialog() == true)
            {

            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTabsBeforeClose();
        }

        private void LoadTabsClosed()
        {
            var JsonFilePath = "closedtabs.json";
            if (File.Exists(JsonFilePath))
            {
                string jsonContent = File.ReadAllText(JsonFilePath);
                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{ }")
                {
                    try
                    {

                        ConfirmDialog confirmDialog = new ConfirmDialog("You have guides to restore, do you want to restore?");
                        if (confirmDialog.ShowDialog() == true)
                        {
                            var loadedTabs = JsonConvert.DeserializeObject<ObservableCollection<TabItem>>(jsonContent);
                            foreach (var tab in loadedTabs)
                            {
                                if (!tab.url.Contains("startpage.html"))
                                {
                                    OpenNewTabWithUrl(tab.url, tab.TabName);
                                }

                            }
                        }




                        

                       
                    }
                    catch (JsonException ex)
                    {
                        // Lida com o erro de desserialização
                        
                    }
                }
            }
            else
            {
                
            }
        }

        private void SaveTabsBeforeClose()
        {
            if (File.Exists("closedtabs.json"))
            {
                File.Delete("closedtabs.json");
            }


            foreach (var item in TabListBox.Items)
            {
                var listBoxItem = TabListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (listBoxItem != null)
                {
                    var tab = item as TabItem;
                    if (tab != null)
                    {
                        tab.UpdateUrl();
                        if (!tab.TabName.Equals("startpage"))
                        {
                            try
                            {
                               
                                var JsonFilePath = "closedtabs.json";
                                string jsonContent = JsonConvert.SerializeObject(Tabs, Formatting.Indented);
                                File.WriteAllText(JsonFilePath, jsonContent);
                            }
                            catch (IOException ex)
                            {
                                // Lida com o erro de IO ao salvar o arquivo
                                ToastWindow.Show("Error: " + ex.Message);
                                
                            }

                        }
                        
                    }
                }
            }
        }

        

        private void LoadPinneds()
        {
            var JsonFilePath = "pinneds.json";
            if (File.Exists(JsonFilePath))
            {
                string jsonContent = File.ReadAllText(JsonFilePath);
                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{ }")
                {
                    try
                    {
                        var loadedTabs = JsonConvert.DeserializeObject<ObservableCollection<TabItem>>(jsonContent);

                        PinnedTabs = new ObservableCollection<TabItem>();

                        foreach (var tab in loadedTabs)
                        {
                            // Verifica se o TabName já existe na coleção PinnedTabs
                            if (!PinnedTabs.Any(t => t.TabName == tab.TabName))
                            {
                                
                                PinnedTabs.Add(tab);
                            }
                        }

                        PinnedTabsListBox.ItemsSource = PinnedTabs;
                    }
                    catch (JsonException ex)
                    {
                        // Lida com o erro de desserialização
                        MessageBox.Show("Erro ao carregar as abas fixas " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                PinnedTabs = new ObservableCollection<TabItem>();
                PinnedTabsListBox.ItemsSource = PinnedTabs;
            }
        }

        
    

    private void SavePinneds()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(PinnedTabs, Formatting.Indented);
                File.WriteAllText("pinneds.json", jsonContent);
            }
            catch (IOException ex)
            {
                // Lida com o erro de IO ao salvar o arquivo
                MessageBox.Show("Erro ao salvar as abas fixas: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSidebarColor()
        {
          

            BackgroundPersist backgroundPersist = new BackgroundPersist();
            string color = backgroundPersist.GetColor();
            Sidebar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            TabListBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            Janela.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            var contextMenu = (ContextMenu)FindResource("BrowserMenu");
            contextMenu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            foreach (var item in contextMenu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                }
            }

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
            bool existent = false;
            if (PinnedTabsListBox.SelectedItem is TabItem selectedTab)
            {
                foreach (var tab in Tabs)
                {
                    if (tab.WebView != null && (tab.WebView.Source.ToString().Contains(selectedTab.url)))
                    {
                        existent = true;
                        SelectTab(tab);
                        break;
                    }
                }
                if(existent == false)
                {
                    OpenNewTabWithUrl(selectedTab.url);
                }
                else
                {

                }
                
            }
            UpdateMiniPlayerVisibility();
            
        }

        private void SelectTab(TabItem tab)
        {
            // Dependendo da sua implementação, pode ser necessário definir o tab como o item selecionado
            // em algum controle de exibição de abas.
            TabListBox.SelectedItem = tab;
        }

        private void PinMouseLeave(object sender, MouseEventArgs e)
        {

            var listBox = sender as ListBox;

            if (listBox != null)
            {
                // Desmarca todos os itens selecionados
                listBox.SelectedItem = null;
                // ou
                listBox.UnselectAll();
            }

            // Define o foco para outro controle ou janela
            Janela.Focus();
        }
        
        private void OpenNewTabWithUrl(string url, string tabName = null)
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            newTab.adBlock = false;
            Tabs.Add(newTab);
            id += 1;

            if (Tabs.Count > 5)
            {
                clearBtn.Visibility = Visibility.Visible;
            }
            else
            {
                clearBtn.Visibility = Visibility.Collapsed;
            }

            var webView = new WebView2();
            webView.Source = new System.Uri(url);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.NavigationStarting += WebView2_NavigationStarting;
            newTab.WebView = webView;
            if(tabName != null)
            {
                newTab.TabName = tabName;
            }

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

        private async void Pin()
        {
            if (PinnedTabs == null)
            {
                PinnedTabs = new ObservableCollection<TabItem>();
                PinnedTabsListBox.ItemsSource = PinnedTabs;
            }

            // Suponha que você tenha um método para obter a URL atual do WebView
            var (currentUrl, currentLogo) = GetCurrentWebViewUrl();
            if(string.IsNullOrEmpty(currentLogo) || currentLogo == "html.png")
            {
                currentLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html.png");
                if(currentUrl.IndexOf("openai.com") != -1 || currentUrl.IndexOf("chatgpt.com") != -1)
                {
                    currentLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chatgpt.png");
                }
                else if (currentUrl.IndexOf("reddit.com") != -1)
                {
                    currentLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reddit.png");
                }
            }

            

            // Verifique se o item já está nos PinnedTabs
            var existingTab = PinnedTabs.FirstOrDefault(tab => tab.url == currentUrl);
            if (existingTab != null)
            {
                // Remove o item existente
                PinnedTabs.Remove(existingTab);
                ToastWindow.Show("Unpinned site: " + currentUrl);
            }
            else
            {
                if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
                {
                    var title = await selectedTab.WebView.CoreWebView2.ExecuteScriptAsync("document.title");
                    title = title.Trim('"'); // Remove the surrounding quotes
                    // Adicione o novo item
                    PinnedTabs.Add(new TabItem
                    {
                        TabName = title,
                        FullTabName = title,
                        LogoUrl = currentLogo,
                        url = currentUrl
                    });
                    ToastWindow.Show("Pinned site: " + currentUrl);
                    
                }
                    
            }
            SavePinneds();
        }

        public class ImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var url = value as string;
                if (string.IsNullOrEmpty(url)) return null;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    bitmap.EndInit();
                    return bitmap;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        private string GetFaviconUrl(string url)
        {
            try
            {
                // Tenta o caminho padrão para .ico
                Uri uri = new Uri(url);
                string domain = uri.GetLeftPart(UriPartial.Authority);
                string[] possiblePaths = { "/favicon.ico", "/favicon.png", "/favicon.jpg" };

                foreach (var path in possiblePaths)
                {
                    string faviconUrl = $"{domain}{path}";
                    if (IsUrlAccessible(faviconUrl))
                    {
                        return faviconUrl;
                    }
                }

                // Tenta buscar o favicon através da análise do HTML
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string htmlContent = response.Content.ReadAsStringAsync().Result;
                        var favicons = ExtractFaviconsFromHtml(htmlContent, domain);

                        if (favicons.Count > 0)
                        {
                            return favicons[0]; // Retorna o primeiro favicon encontrado
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Tratamento de erro ou log
                Console.WriteLine($"Erro ao obter o favicon: {ex.Message}");
            }

            // Retorna um ícone padrão ou nulo se não encontrar o favicon
            return "html.png";
        }

        private bool IsUrlAccessible(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<string> ExtractFaviconsFromHtml(string htmlContent, string domain)
        {
            var favicons = new List<string>();
            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(htmlContent);

            var nodes = document.DocumentNode.SelectNodes("//link[contains(@rel, 'icon')]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var href = node.GetAttributeValue("href", null);
                    if (!string.IsNullOrEmpty(href))
                    {
                        if (!href.StartsWith("http"))
                        {
                            href = new Uri(new Uri(domain), href).ToString();
                        }
                        favicons.Add(href);
                    }
                }
            }

            return favicons;
        }

        private (string, string) GetCurrentWebViewUrl()
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                string url = selectedTab.WebView.Source.ToString();
                //string domain = new Uri(url).GetLeftPart(UriPartial.Authority); // Obtém o domínio da URL
                //string faviconUrl = $"{domain}/favicon.ico"; // Construir a URL do favicon
                string faviconUrl = GetFaviconUrl(url);
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

                if(Tabs.Count > 5) {
                    clearBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    clearBtn.Visibility = Visibility.Collapsed;
                }

                var webView = new WebView2();
                if(urlInputDialog.EnteredUrl.ToString().Equals("startpage"))
                {
                    string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "startpage.html");
                    webView.Source = new System.Uri(htmlFilePath);
                    webView.NavigationCompleted += WebView_NavigationCompleted;
                    webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                    webView.NavigationStarting += WebView2_NavigationStarting;
                    
                }
                else
                {
                    string url = urlInputDialog.EnteredUrl;
                    if(url.Contains("chatgpt "))
                    {
                        prompt = url.Split("chatgpt ")[1];

                        webView.Source = new System.Uri("https://chat.openai.com");
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                        webView.NavigationStarting += WebView2_NavigationStarting;
                        
                    }
                    else
                    {
                        webView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                        webView.NavigationStarting += WebView2_NavigationStarting;
                        
                    }
                    
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
                        string newurl = urlInputDialog.EnteredUrl;
                        if(newurl.Contains("chatgpt "))
                        {
                            prompt = newurl.Split("chatgpt ")[1];

                            selectedTab.WebView.Source = new System.Uri("https://chat.openai.com");
                            selectedTab.WebView.NavigationCompleted += WebView_NavigationCompleted;
                            selectedTab.WebView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                        }
                        else
                        {
                            selectedTab.WebView.Source = new System.Uri(urlInputDialog.EnteredUrl);
                        }
                        
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
            else if(e.Key == Key.J && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Downloads();
            }
            else if(e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CloseTab();
            }
        }

        private void CloseTab()
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                if (selectedTab.WebView.CoreWebView2 != null)
                {
                    selectedTab.WebView?.Dispose();
                    Tabs.Remove(selectedTab);
                    ToastWindow.Show("Tab closed.");
                    if (Tabs.Count > 5)
                    {
                        clearBtn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        clearBtn.Visibility = Visibility.Collapsed;
                    }
                }

            }
            UpdateMiniPlayerVisibility();
        }

        private void Downloads()
        {
            OpenNewTabWithUrl("edge://downloads");
        }

        private void ShortCuts_Click(object sender, RoutedEventArgs e)
        {
            ToastWindow.Show("Ctrl + T (new tab)\nCtrl + L (edit tab url)\nCtrl + H (open historic)\nCtrl + D (Pin/Unpin)\nCtrl + S (toggle sidebar)\nCtrl + W (copy url)\nCtrl + J (open downloads)\n Ctrl + F4 (close tab)");
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

        private void ShowSideBar()
        {
            // Restaurar o Border ao seu tamanho original
            Grid.SetColumn(ContentBorder, 1);
            ContentBorder.Margin = new Thickness(10);
            ContentBorder.CornerRadius = new CornerRadius(10);
            
            Sidebar.Visibility = Visibility.Visible;
            
        }

        private void HideSideBar()
        {
            // Fazer o Border ocupar a tela toda
            Grid.SetColumn(ContentBorder, 0);
            Grid.SetColumnSpan(ContentBorder, 2);
            ContentBorder.Margin = new Thickness(0);
            ContentBorder.CornerRadius = new CornerRadius(0);
            Sidebar.Visibility = Visibility.Collapsed;
            
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isFullScreen)
            {
                // Mostra o sidebar temporariamente
                ShowSideBar();
            }
        }

        // Evento que detecta quando o mouse sai da borda da tela
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isFullScreen)
            {
                // Esconde o sidebar novamente
                HideSideBar();
            }
        }

        private void SidebarToggle()
        {
            //Sidebar.Visibility = Sidebar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (isFullScreen)
            {
                ShowSideBar();
                isFullScreen = false;
            }
            else
            {
                HideSideBar();
                isFullScreen = true;
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
                    Janela.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    var contextMenu = (ContextMenu)FindResource("BrowserMenu");
                    contextMenu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    foreach (var item in contextMenu.Items)
                    {
                        if (item is MenuItem menuItem)
                        {
                            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                            menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                        }
                    }

                    BackgroundPersist backgroundPersist = new BackgroundPersist();
                    backgroundPersist.SaveColor(backgroundColor);
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

        

        private void MiniPlayer_Previus_Click(object sender, RoutedEventArgs e)
        {
            string script = @"
        if (window.location.href.includes('music.youtube.com')) {
            // YouTube Music
            var prevButton = document.querySelector('.previous-button.style-scope.ytmusic-player-bar');
            if (prevButton) prevButton.click();
        } else if (window.location.href.includes('youtube.com')) {
            // YouTube Padrão
            var prevButton = document.querySelector('.ytp-prev-button');
            if (prevButton) prevButton.click();
        }
    ";
            ExecuteMiniPlayerScript(script);
        }

        private void MiniPlayer_Next_Click(object sender, RoutedEventArgs e)
        {
            string script = @"
        if (window.location.href.includes('music.youtube.com')) {
            // YouTube Music
            var nextButton = document.querySelector('.next-button.style-scope.ytmusic-player-bar');
            if (nextButton) nextButton.click();
        } else if (window.location.href.includes('youtube.com')) {
            // YouTube Padrão
            var nextButton = document.querySelector('.ytp-next-button');
            if (nextButton) nextButton.click();
        }
    ";
            ExecuteMiniPlayerScript(script);
        }

        private async void ExecuteMiniPlayerScript(string script)
        {
            foreach (var tab in Tabs)
            {
                if (tab.WebView != null && (tab.WebView.Source.ToString().Contains("youtube.com") ))
                {
                    await tab.WebView.ExecuteScriptAsync(script);
                }
            }
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Start animation when navigation starts
            var storyboard = (Storyboard)this.Resources["LoadingAnimation"];
            storyboard.Begin();


        }


        private void LogNavigationDetails(string url, string title, string favicon)
        {
            
            if (!url.Contains("file:"))
            {
                string logFilePath = "navigationLog.json";
                var logEntry = new NavigationLogEntry
                {
                    Url = url,
                    UrlIco = favicon,
                    Title = title,
                    AccessedAt = DateTime.Now
                };

                List<NavigationLogEntry> logEntries;

                if (File.Exists(logFilePath))
                {
                    string existingLog = File.ReadAllText(logFilePath);
                    logEntries = JsonConvert.DeserializeObject<List<NavigationLogEntry>>(existingLog) ?? new List<NavigationLogEntry>();
                }
                else
                {
                    logEntries = new List<NavigationLogEntry>();
                }

                logEntries.Add(logEntry);

                string updatedLog = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
                File.WriteAllText(logFilePath, updatedLog);
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

                var storyboard = (Storyboard)this.Resources["LoadingAnimation"];
                storyboard.Stop();

                var title = await webView.CoreWebView2.ExecuteScriptAsync("document.title");
                title = title.Trim('"'); // Remove the surrounding quotes
                string url = webView.Source.ToString();
                //string domain = new Uri(url).GetLeftPart(UriPartial.Authority); // Obtém o domínio da URL
                //string faviconUrl = $"{domain}/favicon.ico";
                string faviconUrl = GetFaviconUrl(url);
                LogNavigationDetails(url, title,faviconUrl);

                if (string.IsNullOrEmpty(faviconUrl) || faviconUrl == "html.png")
                {
                    faviconUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html.png");
                    if(url.IndexOf("openai.com") != -1 || url.IndexOf("chatgpt.com") != -1)
                    {
                        faviconUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chatgpt.png");
                    }
                    else if (url.IndexOf("reddit.com") != -1)
                    {
                        faviconUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reddit.png");
                    }
                }


                var tabItem = Tabs.FirstOrDefault(tab => tab.WebView == webView);
                if (tabItem != null)
                {
                    tabItem.OnNavigationCompleted();
                    if (title.Length > 18)
                    {
                        if(tabItem.isRenamed == false)
                        {
                            tabItem.TabName = title.Substring(0, 18);
                        }
                        
                        
                        
                    }
                    else
                    {
                        if(tabItem.isRenamed == false)
                        {
                            tabItem.TabName = title;
                        }
                        
                    }
                    tabItem.LogoUrl = faviconUrl;
                    tabItem.FullTabName = title;
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
                TabListBox.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                


                UpdateMiniPlayerVisibility();

                // Load colors from siteColors.json
                if (File.Exists("siteColors.json"))
                {
                    string siteColorsJson = File.ReadAllText("siteColors.json");
                    var siteColors = JsonConvert.DeserializeObject<List<SiteColorInfo>>(siteColorsJson);

                    var currentSiteColor = siteColors.FirstOrDefault(sc => url.StartsWith(sc.Url, StringComparison.OrdinalIgnoreCase));
                    if (currentSiteColor != null)
                    {
                        string setColorScript = $@"
                document.body.style.backgroundColor = '{currentSiteColor.BackgroundColor}';
                var paragraphs = document.getElementsByTagName('p');
                for (var i = 0; i < paragraphs.length; i++) {{
                    paragraphs[i].style.color = '{currentSiteColor.ForegroundColor}';
                }}
                var labels = document.getElementsByTagName('label');
                for (var i = 0; i < labels.length; i++) {{
                    labels[i].style.color = '{currentSiteColor.ForegroundColor}';
                }}
            ";
                        await webView.CoreWebView2.ExecuteScriptAsync(setColorScript);
                    }
                }
                


                if (prompt.Length > 0)
                {

                    string escapedPrompt = prompt.Replace("'", @"\'").Replace("\"", "\\\"");
                    string fillInputScript = $@"
        console.log('Running script...');
        var input = document.getElementById('prompt-textarea');
        if (input) {{
            console.log('Found input element');
            input.value = '{escapedPrompt}';
            
            // Forçar a mudança de valor e o disparo do evento
            var event = new Event('input', {{ bubbles: true }});
            input.dispatchEvent(event);
            
            // Tentativa adicional de garantir que o valor foi definido
            setTimeout(function() {{
                var submitButton = document.querySelector('[data-testid=""send-button""]');
                if (submitButton) {{
                    console.log('Found submit button');
                    submitButton.click();
                }} else {{
                    console.log('Submit button not found');
                }}
            }}, 100); // Pequeno atraso para garantir que o valor é processado
        }} else {{
            console.log('Input element not found');
        }}
    ";
                    await webView.CoreWebView2.ExecuteScriptAsync(fillInputScript);
                    prompt = "";
                }


                
            }
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (TabListBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // Unsubscribe from event
                TabListBox.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;

                foreach (var item in TabListBox.Items)
                {
                    var listBoxItem = TabListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    if (listBoxItem != null)
                    {
                        var tab = item as TabItem;
                        if (tab != null)
                        {
                            listBoxItem.ToolTip = tab.FullTabName;
                        }
                    }
                }
            }
        }

        private async void UpdateMiniPlayerVisibility()
        {
            try
            {
                bool hasMusicTab = Tabs.Any(tab => tab.WebView != null &&
                                                   tab.WebView.Source != null &&
                                                   tab.WebView.Source.ToString().Contains("youtube.com"));
                MiniPlayer.Visibility = hasMusicTab ? Visibility.Visible : Visibility.Collapsed;

                if (hasMusicTab)
                {
                    try
                    {
                        var musicTab = Tabs.First(tab => tab.WebView != null &&
                                                         tab.WebView.Source != null &&
                                                         tab.WebView.Source.ToString().Contains("youtube.com"));

                        // Espera até que o CoreWebView2 seja inicializado
                        await musicTab.WebView.EnsureCoreWebView2Async();

                        string script = "document.querySelector('title').textContent";

                        var musicTitle = await musicTab.WebView.CoreWebView2.ExecuteScriptAsync(script);
                        musicTitle = musicTitle.Trim('"'); // Remove the surrounding quotes
                        if(musicTitle is not null)
                        {
                            MusicTitle.Text = musicTitle;
                        }
                        
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

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tabItem in Tabs.ToList())
            {
                // Dispose of the WebView2 control
                tabItem.WebView?.Dispose();
            }

            // Clear all tabs
            Tabs.Clear();

            // Update the visibility of the clear button
            clearBtn.Visibility = Visibility.Collapsed;

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
                if (Tabs.Count > 5)
                {
                    clearBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    clearBtn.Visibility = Visibility.Collapsed;
                }
            }
            UpdateMiniPlayerVisibility();
        }





        private void TabListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TabItem)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        private void PinnedListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TabItem)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        private void TabListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the starting point of the drag
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false; // Reset dragging state
        }

        private void PinnedListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the starting point of the drag
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false; // Reset dragging state
        }

        private void TabListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Initialize the drag & drop operation
                _isDragging = true;
                var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBoxItem != null)
                {
                    TabItem tabItem = (TabItem)listBoxItem.DataContext;
                    if (tabItem != null)
                    {
                        DataObject dragData = new DataObject(typeof(TabItem), tabItem);
                        DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PinnedListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Initialize the drag & drop operation
                _isDragging = true;
                var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBoxItem != null)
                {
                    TabItem tabItem = (TabItem)listBoxItem.DataContext;
                    if (tabItem != null)
                    {
                        DataObject dragData = new DataObject(typeof(TabItem), tabItem);
                        DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TabListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                e.Handled = true; // Prevent the ListBox from changing selection
            }
        }

        private void PinnedListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                e.Handled = true; // Prevent the ListBox from changing selection
            }
        }

        // Helper to find the ancestor of a given type
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    
    private void TabListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TabItem)))
            {
                var droppedData = e.Data.GetData(typeof(TabItem)) as TabItem;
                var target = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource)?.DataContext as TabItem;

                if (droppedData != null && target != null)
                {
                    int removedIdx = Tabs.IndexOf(droppedData);
                    int targetIdx = Tabs.IndexOf(target);

                    if (removedIdx != targetIdx)
                    {
                        try
                        {
                            Tabs.Move(removedIdx, targetIdx);
                        }
                        catch(Exception ex)
                        {
                            return;
                        }
                    }
                }
            }
            _isDragging = false; // Reset dragging state after drop
        }

        private void PinnedListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TabItem)))
            {
                var droppedData = e.Data.GetData(typeof(TabItem)) as TabItem;
                var target = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource)?.DataContext as TabItem;

                if (droppedData != null && target != null)
                {
                    int removedIdx = PinnedTabs.IndexOf(droppedData);
                    int targetIdx = PinnedTabs.IndexOf(target);

                    if (removedIdx != targetIdx)
                    {
                        try
                        {
                            PinnedTabs.Move(removedIdx, targetIdx);
                            SavePinneds();
                        }
                        catch(Exception ex)
                        {
                            return;
                        }
                        
                    }
                }
            }
            _isDragging = false; // Reset dragging state after drop
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

    public class NavigationLogEntry
    {
        public string Url { get; set; }

        public string UrlIco { get; set; }
        public string Title { get; set; }
        public DateTime AccessedAt { get; set; }
    }

    public class TabItem
    {
        public string TabName { get; set; }

        public string FullTabName { get; set; }

        public string LogoUrl { get; set; }

        public string url { get; set; }

        public bool adBlock { get; set; }

        [JsonIgnore]
        public WebView2 WebView { get; set; }

        [JsonIgnore]
        public Visibility ProgressBarVisibility { get; set; } = Visibility.Visible;

        [JsonIgnore]
        public bool isRenamed { get; set; } = false;

        public void OnNavigationCompleted()
        {
            ProgressBarVisibility = Visibility.Hidden;
        }

        public void UpdateUrl()
        {
            if (WebView != null)
            {
                url = WebView.Source?.ToString();
            }
        }
    }
}
