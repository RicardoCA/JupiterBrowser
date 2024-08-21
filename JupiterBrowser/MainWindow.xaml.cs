using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Storage;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Interop.WinDef;
using MessageBox = System.Windows.MessageBox;

//using Wpf.Ui.Controls; // Para as cores do WPF

namespace JupiterBrowser
{
    public partial class MainWindow : Window
    {
        private string VERSION = "7.0";
        public ObservableCollection<TabItem> Tabs { get; set; }
        public ObservableCollection<TabItem> PinnedTabs { get; set; }
        private TabItem _draggedItem;
        private bool _isDragging;
        private Point _dragStartPoint;
        private bool isFullScreen = false;
        private DispatcherTimer _musicTitleUpdateTimer;
        private string prompt = "";
        private DispatcherTimer _titleUpdateTimer;
        private DispatcherTimer _updateCheckerTimer;

        private string languageT = "en";
        private string start = "Question";
        private string miniWindow = "MiniWindowTrue";

        private bool tabMenuIsOpen = false;
        private TabItem selectedTabItemContextMenu;
        private TabItem selectedPinnedItemContextMenu;

        private AppPreview appWhatsapp;
        private AppPreview appFacebook;
        private AppPreview appInstagram;
        private AppPreview appX;
        private AppPreview appYoutube;
        private AppPreview appTiktok;

        public int id = 1;



        private static readonly string ApiKey = Environment.GetEnvironmentVariable("JUPITER_KEY");

        private static readonly string ProjectId = Environment.GetEnvironmentVariable("JUPITER_ID");
        private static readonly string DatabaseUrl = Environment.GetEnvironmentVariable("JUPITER_URL");
        private const string loggedFile = "account.json";
        private FirebaseAuthClient authClient;
        private FirebaseClient databaseClient;

        static readonly HttpClient client = new HttpClient();
        private string email = "";
        private string password = "";




        public MainWindow()
        {
            AmbienteCheck();
            InitializeComponent();
            RestoreAccount();
            InitializeFirebase();
            Tabs = new ObservableCollection<TabItem>();
            TabListBox.ItemsSource = Tabs;
            this.DataContext = this;
            this.KeyDown += Window_KeyDown;
            this.Closing += MainWindow_Closing;
            this.MaxHeight = (SystemParameters.WorkArea.Height + SystemParameters.WindowCaptionHeight) - 15;
            this.MaxWidth = SystemParameters.WorkArea.Width + 10;
            this.WindowState = WindowState.Maximized;

            // Inicializa o timer
            _musicTitleUpdateTimer = new DispatcherTimer();
            _musicTitleUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            _musicTitleUpdateTimer.Tick += MusicTitleUpdateTimer_Tick;

            // Inicializa o timer de atualização de títulos
            _titleUpdateTimer = new DispatcherTimer();
            _titleUpdateTimer.Interval = TimeSpan.FromSeconds(5);
            _titleUpdateTimer.Tick += async (s, e) => await UpdateTabTitlesAsync();
            _titleUpdateTimer.Start();

            _updateCheckerTimer = new DispatcherTimer();
            _updateCheckerTimer.Interval = TimeSpan.FromMinutes(30);
            _updateCheckerTimer.Tick += CheckForUpdatesTimer_Tick;
            _updateCheckerTimer.Start();

           


            // Chama o método de inicialização assíncrono
            if (email.Length > 0 && password.Length > 0)
            {
                _ = InitializeAsync();
                CheckForUpdatesTimer();
            }
            else
            {
                LoadSettings();
                CleanUpdates();
                LoadSidebarColor();
                LoadPinneds();
                LoadTabsClosed();
                OpenStartPage();
                CheckForUpdatesTimer();
                LoadFolders();
                
            }


            
            
            

        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreButton_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                
                this.WindowState = WindowState.Maximized;
            }
            
        }


        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            CopyURL();
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Downloads_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabWithUrl("edge://downloads");
        }

        


        private void AmbienteCheck()
        {
            if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(ProjectId) || string.IsNullOrEmpty(DatabaseUrl))
            {
                ExecutarSetup();
            }
        }

        private void ExecutarSetup()
        {
            string setupPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup.exe");

            if (File.Exists(setupPath))
            {
                try
                {
                    // Executa o Setup.exe
                    Process.Start(setupPath);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task InitializeAsync()
        {
            await SyncOnStart();

            LoadSettings();
            CleanUpdates();
            LoadSidebarColor();
            LoadPinneds();
            LoadPinnedsMobile();
            LoadTabsClosed();
            LoadTabsClosedMobile();
            OpenStartPage();
            LoadFolders();
        }

       

        private void CheckForUpdatesTimer_Tick(object sender, EventArgs e)
        {
            CheckForUpdatesTimer();
        }

        private async void CheckForUpdatesTimer()
        {
            string serverVersion = await GetServerVersionAsync();
            if (!serverVersion.Equals(VERSION))
            {
                BannerAtt.Visibility = Visibility.Visible;
                _updateCheckerTimer.Stop();
            }

        }

        private void ClickBannerWhats(object sender, MouseButtonEventArgs e)
        {
            OpenWhatsappApp();
        }

        private void ClickBannerAtt(object sender, MouseButtonEventArgs e)
        {
            Process.Start("Updater.exe");
        }


        private void X_Click(object sender, RoutedEventArgs e)
        {
            OpenXApp();
        }

        private void TikTok_Click(object sender, RoutedEventArgs e)
        {
            OpenTiktokApp();
        }

        private void Youtube_Click(object sender, RoutedEventArgs e)
        {
            OpenYoutubeApp();
        }

        private void OpenXApp()
        {
            if (appX == null)
            {
                appX = new AppPreview(this, "https://x.com");
                appX.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appX.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void OpenTiktokApp()
        {
            if (appTiktok == null)
            {
                appTiktok = new AppPreview(this, "https://tiktok.com");
                appTiktok.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appTiktok.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void OpenYoutubeApp()
        {
            if (appYoutube == null)
            {
                appYoutube = new AppPreview(this, "https://youtube.com");
                appYoutube.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appYoutube.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void OpenWhatsappApp()
        {
            if (appWhatsapp == null)
            {
                appWhatsapp = new AppPreview(this, "https://web.whatsapp.com");
                appWhatsapp.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appWhatsapp.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void OpenFacebookApp()
        {
            if (appFacebook == null)
            {
                appFacebook = new AppPreview(this, "https://facebook.com");
                appFacebook.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appFacebook.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void OpenInstagramApp()
        {
            if (appInstagram == null)
            {
                appInstagram = new AppPreview(this, "https://instagram.com");
                appInstagram.Show();
                UpdateIcon("Jupiter Browser");
            }
            else
            {
                appInstagram.Show();
                UpdateIcon("Jupiter Browser");
            }
        }

        private void Instagram_Click(object sender, RoutedEventArgs e)
        {
            OpenInstagramApp();
        }

        private void Facebook_Click(object sender, RoutedEventArgs e)
        {
            OpenFacebookApp();
        }

        private void WhatsApp_Click(object sender, RoutedEventArgs e)
        {
            OpenWhatsappApp();
        }

        public void UpdateIcon(string newTitle)
        {
            // Lógica para atualizar o ícone com base no novo título
            //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/newIcon.ico")); // Exemplo de atualização de ícone
            this.Title = newTitle;
            if (newTitle.IndexOf("(") != -1)
            {
                if (newTitle.IndexOf(")") != -1)
                {
                    if (newTitle.IndexOf("WhatsApp") != -1)
                    {
                        BannerWhats.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BannerWhats.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    BannerWhats.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BannerWhats.Visibility = Visibility.Collapsed;
            }

        }



        private void RestoreAccount()
        {
            if (File.Exists(loggedFile))
            {
                string json = File.ReadAllText(loggedFile);
                Account account = JsonConvert.DeserializeObject<Account>(json);

                if (account != null)
                {
                    this.email = account.Email;
                    this.password = account.Password;
                    ToastWindow.Show("Synchronizing browser.", 6000);
                }
            }
        }

        public async Task<bool> AccountExistsAsync(string email, string password)
        {
            var accounts = await databaseClient.Child("users").OnceAsync<User>();

            return accounts.Any(a => a.Object.Email == email && a.Object.Password == password);
        }

        private async Task<string> GetFirebaseTokenAsync()
        {
            // Este método deve retornar um token válido para autenticação
            // Você precisará implementar a lógica para obter e gerenciar tokens
            if (authClient.User != null)
            {
                return await authClient.User.GetIdTokenAsync();
            }
            return null;
        }

        private async void InitializeFirebase()
        {
            try
            {
                // Configuração do Firebase Auth
                var config = new FirebaseAuthConfig
                {
                    ApiKey = ApiKey,
                    AuthDomain = $"{ProjectId}.firebaseapp.com",
                    Providers = new FirebaseAuthProvider[]
                    {
                        new EmailProvider()
                    }
                };

                authClient = new FirebaseAuthClient(config);

                // Configuração do Firebase Realtime Database
                databaseClient = new FirebaseClient(
                    DatabaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = GetFirebaseTokenAsync }
                );


            }
            catch (Exception ex)
            {

            }
        }

        private async Task<List<string>> GetStorageFilesAsync(string email)
        {
            // Certifique-se de que o URL da API esteja correto
            var uri = $"https://firebasestorage.googleapis.com/v0/b/jupiterbrowser-8f6b2.appspot.com/o?prefix={Uri.EscapeDataString(email)}/&delimiter=/";

            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var firebaseResponse = JsonConvert.DeserializeObject<FirebaseStorageResponse>(responseContent);

            return firebaseResponse?.Items?.Select(item => item.Name).ToList() ?? new List<string>();
        }

        private async Task SyncOnClose()
        {

            bool accountExists = await AccountExistsAsync(email, password);
            if (accountExists)
            {
                ToastWindow.Show("Synchronizing browser...");
                string[] uploadFiles = { "calc.json", "navigationLog.json", "pinneds.json", "sidebar.json", "siteColors.json", "vault.json", "settings.json", "closedtabs.json","folders.json" };
                var storage = new FirebaseStorage("jupiterbrowser-8f6b2.appspot.com");

                

                foreach (var file in uploadFiles)
                {
                    try
                    {
                        await storage
                            .Child(email)
                            .Child(file)
                            .DeleteAsync();
                        Console.WriteLine($"Arquivo {file} na pasta {email} apagado com sucesso.");
                    }
                    catch (Exception ex)
                    {
                        // Se o arquivo não existir, isso não é um problema, então podemos ignorar a exceção
                        Console.WriteLine($"Aviso ao tentar apagar {file}: {ex.Message}");
                    }
                }

                // Upload de arquivos locais
                foreach (var file in uploadFiles)
                {
                    var filePath = Path.Combine(Environment.CurrentDirectory, file);
                    if (File.Exists(filePath))
                    {
                        using (var fileStream = File.Open(filePath, FileMode.Open))
                        {
                            var task = storage
                                .Child(email) // Usar email como nome da pasta
                                .Child(file)  // Nome do arquivo no storage
                                .PutAsync(fileStream);
                            // Acompanhar progresso (opcional)
                            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");
                            await task;
                        }
                    }
                }
            }
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }

        private async Task SyncOnStart()
        {
            bool accountExists = await AccountExistsAsync(email, password);
            if (accountExists)
            {
                string[] downloadFiles = { "calc.json", "navigationLog.json", "pinneds.json", "sidebar.json", "siteColors.json", "vault.json", "settings.json", "closedtabs.json","pinnedsMobile.json","closedtabsMobile.json","folders.json" };
                var storage = new FirebaseStorage("jupiterbrowser-8f6b2.appspot.com");

                foreach (var file in downloadFiles)
                {
                    try
                    {
                        // Obter a referência do arquivo no Firebase Storage
                        var fileReference = storage.Child(email).Child(file);

                        // Caminho local para salvar o arquivo
                        string localFilePath = Path.Combine(Environment.CurrentDirectory, file);

                        // Download do arquivo
                        var downloadTask = fileReference.GetDownloadUrlAsync();
                        var url = await downloadTask;

                        // Usar HttpClient para baixar o arquivo
                        using (var httpClient = new HttpClient())
                        {
                            var fileBytes = await httpClient.GetByteArrayAsync(url);

                            // Salvar o arquivo localmente, substituindo se já existir
                            File.WriteAllBytes(localFilePath, fileBytes);


                        }
                    }
                    catch (Exception ex)
                    {
                        // Se o arquivo não existir no Storage ou houver outro erro, registre e continue
                        Console.WriteLine($"Erro ao baixar {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Conta não existe. Não foi possível sincronizar.");
            }
        }



        private void Account_Click(object sender, RoutedEventArgs e)
        {
            AccountCreate accountCreate = new AccountCreate();
            if (accountCreate.ShowDialog() == true)
            {

            }
        }



        private void Anonymous_Click(object sender, RoutedEventArgs e)
        {
            // OpenAnonymousWindow();
            AnonymousWindow anonymousWindow = new AnonymousWindow();
            anonymousWindow.Show();
        }







        private void LoadSettings()
        {
            try
            {
                if (File.Exists("settings.json"))
                {
                    var jsonString = File.ReadAllText("settings.json");
                    var settings = JsonConvert.DeserializeObject<BrowserSettings>(jsonString);

                    if (settings != null)
                    {
                        languageT = settings.DefaultTranslateLanguage switch
                        {
                            "English" => "en",
                            "Português" => "pt",
                            "Español" => "es",
                            _ => "en"
                        };

                        start = settings.PreviousNavigation switch
                        {
                            "Question" => "Question",
                            "ReopenTabs" => "ReopenTabs",
                            "StartNewNavigation" => "StartNewNavigation",
                            _ => "Question"
                        };

                        miniWindow = settings.MiniWindow switch
                        {
                            "MiniWindowTrue" => "MiniWindowTrue",
                            "MiniWindowFalse" => "MiniWindowFalse",
                            _ => "MiniWindowTrue"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Failed to load settings: {ex.Message}");
            }
        }

        private void TabMenuItem_Translate(object sender, RoutedEventArgs e)
        {

            if (selectedTabItemContextMenu != null)
            {
                string currentUrl = selectedTabItemContextMenu.WebView.Source.ToString();
                string translatedUrl = "https://translate.google.com/translate?sl=auto&tl=" + languageT + "&u=" + currentUrl;
                selectedTabItemContextMenu.WebView.Source = new Uri(translatedUrl);
            }
        }

        private void TabListRename_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = GetClickedTabItem(e);
            if (clickedItem != null)
            {
                // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia
                string currentName = clickedItem.TabName;
                PromptWindow promptWindow = new PromptWindow(currentName, "Rename Tab:");
                if (promptWindow.ShowDialog() == true)
                {
                    if (promptWindow.UserInput.Length > 0)
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

        private void TabListBox_MouseEnter(object sender, MouseEventArgs e)
        {
            foreach (var item in TabListBox.Items)
            {
                // Encontra o ListBoxItem correspondente
                var listBoxItem = TabListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (listBoxItem != null)
                {
                    var closeButton = FindChild<Button>(listBoxItem, "CloseTabButton");
                    if (closeButton != null)
                    {
                        closeButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void TabListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var item in TabListBox.Items)
            {
                // Encontra o ListBoxItem correspondente
                var listBoxItem = TabListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (listBoxItem != null)
                {
                    var closeButton = FindChild<Button>(listBoxItem, "CloseTabButton");
                    if (closeButton != null)
                    {
                        closeButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Busca direta
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // Se o tipo filho não é o alvo
                if (child is not T childType)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // Busca o nome do filho
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private async Task UpdateTabTitlesAsync()
        {
            if (tabMenuIsOpen == false)
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
                                string rawTitle = await tab.WebView.ExecuteScriptAsync(script);

                                // Log do valor bruto retornado pelo script
                                Console.WriteLine($"Raw title: {rawTitle}");

                                // Remove as aspas ao redor do título
                                string title = rawTitle.Trim(new char[] { '"' });

                                // Log após a remoção das aspas
                                Console.WriteLine($"Processed title: {title}");

                                if (!string.IsNullOrEmpty(title) && title != tab.FullTabName)
                                {
                                    if (tab.isRenamed == false)
                                    {
                                        tab.FullTabName = title;
                                        tab.TabName = title.Length > 13 ? title.Substring(0, 13) : title;

                                        // Atualiza o item no ListBox sem recriar todos os itens
                                        var listBoxItem = TabListBox.ItemContainerGenerator.ContainerFromItem(tab) as ListBoxItem;
                                        if (listBoxItem != null)
                                        {
                                            var textBlock = FindVisualChild<TextBlock>(listBoxItem);
                                            if (textBlock != null)
                                            {
                                                textBlock.Text = tab.TabName;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log de exceções
                                Console.WriteLine($"Erro ao atualizar o título: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log de exceções
                    Console.WriteLine($"Erro ao atualizar as abas: {ex.Message}");
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
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


        private void JupiterCard_Click(object sender, RoutedEventArgs e)
        {
            JupiterCard jupiterCard = new JupiterCard();
            if (jupiterCard.ShowDialog() == true)
            {

            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            // Perform the asynchronous operation

            SaveTabsBeforeClose();
            await SyncOnClose();
            e.Cancel = false;
            this.Close();
        }

        private void MiniPlayer_Enter(object sender, MouseEventArgs e)
        {
            MusicTitle.Visibility = Visibility.Visible;
            PlayerTitle.Visibility = Visibility.Visible;
        }

        private void MiniPlayer_Leave(object sender, MouseEventArgs e)
        {
            MusicTitle.Visibility = Visibility.Collapsed;
            PlayerTitle.Visibility = Visibility.Collapsed;
        }

        private async void LoadTabsClosedMobile()
        {
            string jsonFilePath = "closedtabsMobile.json";
            if (File.Exists(jsonFilePath))
            {
                if (start.Equals("Question"))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
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
                                    OpenNewTabWithUrl(tab.url, tab.TabName);

                                }
                            }







                        }
                        catch (JsonException ex)
                        {
                            // Lida com o erro de desserialização

                        }
                    }
                }
                else if (start.Equals("ReopenTabs"))
                {
                    string jsonContent = File.ReadAllText(jsonFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{ }")
                    {
                        var loadedTabs = JsonConvert.DeserializeObject<ObservableCollection<TabItem>>(jsonContent);
                        foreach (var tab in loadedTabs)
                        {
                            OpenNewTabWithUrl(tab.url, tab.TabName);

                        }
                    }
                    
                }

                File.Delete(jsonFilePath);
                var storage = new FirebaseStorage("jupiterbrowser-8f6b2.appspot.com");
                
                try
                {
                    await storage.Child(email).Child(jsonFilePath).DeleteAsync();
                }
                catch (Exception ex)
                {
                    return;
                }

            }

            
        }

        private void LoadTabsClosed()
        {
            if (start.Equals("Question"))
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
            else if (start.Equals("ReopenTabs"))
            {
                var JsonFilePath = "closedtabs.json";
                if (File.Exists(JsonFilePath))
                {
                    string jsonContent = File.ReadAllText(JsonFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{ }")
                    {
                        try
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
                        catch (Exception ex)
                        {

                        }
                    }
                }
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

        private async void LoadPinnedsMobile()
        {
            var JsonFilePath = "pinnedsMobile.json";

            if (File.Exists(JsonFilePath))
            {
                string jsonContent = File.ReadAllText(JsonFilePath);
                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{ }")
                {
                    try
                    {
                        var loadedTabs = JsonConvert.DeserializeObject<ObservableCollection<TabItem>>(jsonContent);

                        if (loadedTabs != null)
                        {
                            // Remove os itens que não estão listados no JSON apenas se isProtected for false
                            for (int i = PinnedTabs.Count - 1; i >= 0; i--)
                            {
                                var existingTab = PinnedTabs[i];
                                if (!existingTab.isProtected && !loadedTabs.Any(t => t.TabName == existingTab.TabName))
                                {
                                    PinnedTabs.RemoveAt(i);
                                }
                            }

                            // Renomeia ou troca o link dos itens existentes
                            foreach (var tab in loadedTabs)
                            {
                                var existingTab = PinnedTabs.FirstOrDefault(t => t.TabName == tab.TabName);
                                if (existingTab != null)
                                {
                                    existingTab.TabName = tab.TabName;
                                    existingTab.url = tab.url;
                                }
                                else
                                {
                                    PinnedTabs.Add(tab);
                                }
                            }

                            // Atualiza a fonte de itens do ListBox
                            PinnedTabsListBox.ItemsSource = PinnedTabs;
                            var storage = new FirebaseStorage("jupiterbrowser-8f6b2.appspot.com");
                            File.Delete(JsonFilePath);
                            try
                            {
                                await storage.Child(email).Child(JsonFilePath).DeleteAsync();
                            }
                            catch(Exception ex)
                            {
                                return;
                            }
                            

                        }
                    }
                    catch (JsonException ex)
                    {
                        // Lida com o erro de desserialização
                        MessageBox.Show("Erro ao carregar as abas fixas " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            CustomBarWindow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            FoldersTreeView.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

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

            var contextMenuTabs = (ContextMenu)FindResource("TabItemMenu");
            contextMenuTabs.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            foreach (var item in contextMenuTabs.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                }
            }

            var contextMenuPins = (ContextMenu)FindResource("PinnedItemMenu");
            contextMenuPins.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            foreach (var item in contextMenuPins.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                }
            }

            var contextMenuApps = (ContextMenu)FindResource("AppsMenu");
            contextMenuApps.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            foreach (var item in contextMenuApps.Items)
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
                if (selectedTab.adBlock == false)
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

        public static async Task<string> GetServerVersionAsync()
        {
            using (WebClient client = new WebClient())
            {
                string url = "https://drive.google.com/uc?export=download&id=1sz4dx76iHLJ7gTl9tezuGc27_rbUsR8j";
                return (await client.DownloadStringTaskAsync(new Uri(url))).Trim();
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
            if (TabListBox.Items.Count == 0)
            {
                string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "startpage.html");
                OpenNewTabWithUrl(htmlFilePath);
            }

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
                if (existent == false)
                {
                    if (selectedTab.isProtected == false)
                    {
                        OpenNewTabWithUrl(selectedTab.url);
                    }
                    else
                    {
                        PromptWindow promptWindow = new PromptWindow("", "Password:");
                        if (promptWindow.ShowDialog() == true)
                        {
                            if (!string.IsNullOrEmpty(promptWindow.UserInput))
                            {
                                Vault vault = new Vault();
                                string decryptedPassword = vault.GetStoredPassword();
                                string password = promptWindow.UserInput;
                                if (decryptedPassword.Equals(password))
                                {
                                    string url = vault.Decrypt(selectedTab.url);
                                    ToastWindow.Show("Opening tab.");
                                    OpenNewTabWithUrl(url);
                                }
                                else
                                {
                                    ToastWindow.Show("Wrong password.");
                                }
                            }
                        }
                    }

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

        public void OpenNewTabWithUrl(string url, string tabName = null)
        {
            var newTab = new TabItem { TabName = "New Tab " + id };
            newTab.adBlock = false;
            Tabs.Add(newTab);
            id += 1;

            

            var webView = new WebView2();
            webView.Source = new System.Uri(url);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.NavigationStarting += WebView2_NavigationStarting;

            newTab.WebView = webView;
            if (tabName != null)
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

            if (miniWindow.Equals("MiniWindowFalse"))
            {
                OpenNewTabWithUrl(newUrl);
            }
            else
            {
                AppPreview appPreview = new AppPreview(this, newUrl, true);
                appPreview.Show();
            }

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
            if (string.IsNullOrEmpty(currentLogo) || currentLogo == "html.png")
            {
                currentLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html.png");
                
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
                // Usa a URL do Google Favicon Service para obter o favicon
                string favUrl = $"https://www.google.com/s2/favicons?sz=64&domain_url={url}";

                // Verifica se a URL é acessível
                if (IsUrlAccessible(favUrl))
                {
                    return favUrl; // Retorna a URL do favicon obtido
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

        /*private string GetFaviconUrl(string url)
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
        }*/

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

                

                var webView = new WebView2();
                if (urlInputDialog.EnteredUrl.ToString().Equals("startpage"))
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
                    if (url.Contains("chatgpt "))
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
                        if (newurl.Contains("chatgpt "))
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



        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            string serverVersion = await GetServerVersionAsync();
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
            if (e.Key == Key.T && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenTiktokApp();
            }
            else if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenNewTab();
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                EditTabUrl();
            }
            else if (e.Key == Key.H && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                OpenHistoric();
            }
            else if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Pin();
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SidebarToggle();
            }
            else if (e.Key == Key.W && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenWhatsappApp();
            }
            else if (e.Key == Key.F && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenFacebookApp();
            }

            else if (e.Key == Key.X && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenXApp();
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenYoutubeApp();
            }
            else if (e.Key == Key.I && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                OpenInstagramApp();
            }
            else if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CopyURL();
            }
            else if (e.Key == Key.J && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Downloads();
            }
            else if (e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CloseTab();
            }
            else if (e.Key == Key.N && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                AnonymousWindow anonymousWindow = new AnonymousWindow();
                anonymousWindow.Show();
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
            ToastWindow.Show("Ctrl + T (new tab)\nCtrl + L (edit tab url)\nCtrl + H (open historic)\nCtrl + D (Pin/Unpin)\nCtrl + S (toggle sidebar)\nCtrl + W (copy url)\nCtrl + J (open downloads)\nCtrl + F4 (close tab)\nCtrl + Shift + N (open incognito window)\nCtrl + Shift + W(open whatsapp app)\nCtrl + Shift + F(open facebook app)\nCtrl + Shift + I(open instagram app)\nCtrl + Shift + Y(open youtube app)\nCtrl + Shift + T(open tiktok app)\nCtrl + Shift + X(open X app) ", 7000);
        }

        private void CopyURL()
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                string url = selectedTab.WebView.Source.ToString();
                if (url.IndexOf("http") != -1)
                {
                    Clipboard.SetText(url);
                    ToastWindow.Show("Url copied.");
                }
                else
                {
                    ToastWindow.Show("You cannot be on a file or startpage.");
                }

            }
            else
            {
                ToastWindow.Show("No tab selected or no url available.");
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
            ContentBorder.Margin = new Thickness(20);
            ContentBorder.CornerRadius = new CornerRadius(10);
            Sidebar.Visibility = Visibility.Collapsed;

        }
        private bool _isContextMenuOpen = false;
        private void ClearTreeViewSelection(ItemsControl itemsControl)
        {
            if(_isContextMenuOpen is false)
            {
                foreach (var item in itemsControl.Items)
                {
                    TreeViewItem treeViewItem = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                    if (treeViewItem != null && treeViewItem.IsSelected)
                    {
                        treeViewItem.IsSelected = false;
                    }

                    if (treeViewItem != null && treeViewItem.Items.Count > 0)
                    {
                        ClearTreeViewSelection(treeViewItem);
                    }
                }
            }
            
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            _isContextMenuOpen = false;
        }

        // Evento que detecta quando o mouse sai da borda da tela
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if(FoldersTreeView.SelectedItem != null)
            {
                jupiterMenuBtn.Focus();
                ClearTreeViewSelection(FoldersTreeView);
            }
            

            if (isFullScreen)
            {
                // Esconde o sidebar novamente
                HideSideBar();
            }
        }

        private void SidebarToggle_Enter(object sender, MouseEventArgs e)
        {
            if (isFullScreen)
            {
                ShowSideBar();
            }
        }

        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            SidebarToggle();
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

        private void Apps_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu menu = this.FindResource("AppsMenu") as ContextMenu;
            menu.PlacementTarget = button;
            menu.IsOpen = true;

        }

        private void OpenBrowserMenu_Click(object sender, RoutedEventArgs e)
        {


            Button button = sender as Button;
            ContextMenu menu = this.FindResource("BrowserMenu") as ContextMenu;
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }

        private void TabMenuItem_Close(object sender, RoutedEventArgs e)
        {
            // Remove o item selecionado da lista de Tabs
            if (selectedTabItemContextMenu != null)
            {
                selectedTabItemContextMenu.WebView.Dispose();
                Tabs.Remove(selectedTabItemContextMenu);
                selectedTabItemContextMenu = null; // Limpa a referência após remover
            }
        }

        private void TabMenuItem_Pin(object sender, RoutedEventArgs e)
        {
            Pin();
        }

        private void TabMenuItem_Rename(object sender, RoutedEventArgs e)
        {
            // Obtenha o ContextMenu do sender
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                // Obtenha o elemento que foi alvo do ContextMenu
                if (contextMenu.PlacementTarget is FrameworkElement element)
                {
                    // Obtenha o item de dados associado ao DataContext
                    var clickedItem = element.DataContext as TabItem;
                    if (clickedItem != null)
                    {
                        // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia
                        string currentName = clickedItem.TabName;
                        PromptWindow promptWindow = new PromptWindow(currentName, "Rename Tab:");
                        if (promptWindow.ShowDialog() == true)
                        {
                            if (!string.IsNullOrEmpty(promptWindow.UserInput))
                            {
                                string newName = promptWindow.UserInput;
                                // Atualiza o nome da guia se o novo nome não for nulo ou vazio
                                clickedItem.TabName = newName.Length > 18 ? newName.Substring(0, 18) : newName;
                                clickedItem.FullTabName = newName;
                                clickedItem.isRenamed = true;

                            }
                            else
                            {
                                clickedItem.isRenamed = false;

                            }
                        }
                    }
                }
            }
        }

        private void PinnedItemMenu_Protect(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                // Obtenha o elemento que foi alvo do ContextMenu
                if (contextMenu.PlacementTarget is FrameworkElement element)
                {
                    // Obtenha o item de dados associado ao DataContext
                    var clickedItem = element.DataContext as TabItem;
                    if (clickedItem != null)
                    {
                        // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia fixada

                        Vault vault = new Vault();
                        if (vault.HasVaultExist() == false)
                        {
                            PromptWindow promptWindow = new PromptWindow("", "Password:");
                            if (promptWindow.ShowDialog() == true)
                            {
                                if (!string.IsNullOrEmpty(promptWindow.UserInput))
                                {
                                    //PinnedTabs.Remove(clickedItem);
                                    string password = promptWindow.UserInput;


                                    if (clickedItem.isProtected == false)
                                    {
                                        string encryptedUrl = vault.Encrypt(clickedItem.url);
                                        clickedItem.url = encryptedUrl;
                                        //PinnedTabs.Add(clickedItem);

                                        // Encontra o item correspondente em PinnedTabs e o atualiza
                                        var pinnedItem = PinnedTabs.FirstOrDefault(item => item.url == clickedItem.url);
                                        if (pinnedItem != null)
                                        {
                                            int index = PinnedTabs.IndexOf(pinnedItem);
                                            if (index != -1)
                                            {

                                                vault.CreateVault(password);

                                                string decryptedPassword = vault.GetStoredPassword();


                                                // Remove o item antigo
                                                PinnedTabs.RemoveAt(index);

                                                // Cria um novo item com as propriedades atualizadas
                                                var updatedItem = new TabItem
                                                {
                                                    url = encryptedUrl,
                                                    TabName = clickedItem.TabName,
                                                    FullTabName = clickedItem.FullTabName,
                                                    LogoUrl = clickedItem.LogoUrl,
                                                    isProtected = true


                                                    // Adicione outras propriedades necessárias aqui
                                                };

                                                // Insere o novo item na mesma posição
                                                PinnedTabs.Insert(index, updatedItem);
                                                SavePinneds();
                                                ToastWindow.Show("Pinned protected.");
                                            }
                                        }
                                    }




                                }
                            }




                        }
                        else
                        {

                            if (clickedItem.isProtected == false)
                            {
                                string encryptedUrl = vault.Encrypt(clickedItem.url);

                                //PinnedTabs.Add(clickedItem);

                                // Encontra o item correspondente em PinnedTabs e o atualiza
                                var pinnedItem = PinnedTabs.FirstOrDefault(item => item.url == clickedItem.url);
                                if (pinnedItem != null)
                                {
                                    int index = PinnedTabs.IndexOf(pinnedItem);
                                    if (index != -1)
                                    {




                                        // Remove o item antigo
                                        PinnedTabs.RemoveAt(index);

                                        // Cria um novo item com as propriedades atualizadas
                                        var updatedItem = new TabItem
                                        {
                                            url = encryptedUrl,
                                            TabName = clickedItem.TabName,
                                            FullTabName = clickedItem.FullTabName,
                                            LogoUrl = clickedItem.LogoUrl,
                                            isProtected = true


                                            // Adicione outras propriedades necessárias aqui
                                        };

                                        // Insere o novo item na mesma posição
                                        PinnedTabs.Insert(index, updatedItem);
                                        SavePinneds();
                                        ToastWindow.Show("Pinned protected.");
                                    }
                                }
                            }
                        }




                    }
                }
            }





        }

        private void PinnedItemMenu_Rename(object sender, RoutedEventArgs e)
        {
            // Obtenha o ContextMenu do sender
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                // Obtenha o elemento que foi alvo do ContextMenu
                if (contextMenu.PlacementTarget is FrameworkElement element)
                {
                    // Obtenha o item de dados associado ao DataContext
                    var clickedItem = element.DataContext as TabItem;
                    if (clickedItem != null)
                    {
                        // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia fixada
                        string currentName = clickedItem.TabName;
                        PromptWindow promptWindow = new PromptWindow(currentName, "Rename Pinned Tab:");
                        if (promptWindow.ShowDialog() == true)
                        {
                            if (!string.IsNullOrEmpty(promptWindow.UserInput))
                            {
                                //PinnedTabs.Remove(clickedItem);
                                string newName = promptWindow.UserInput;
                                // Atualiza o nome da guia fixada se o novo nome não for nulo ou vazio
                                clickedItem.TabName = newName.Length > 18 ? newName.Substring(0, 18) : newName;
                                clickedItem.FullTabName = newName;
                                //PinnedTabs.Add(clickedItem);

                                // Encontra o item correspondente em PinnedTabs e o atualiza
                                var pinnedItem = PinnedTabs.FirstOrDefault(item => item.url == clickedItem.url);
                                if (pinnedItem != null)
                                {
                                    int index = PinnedTabs.IndexOf(pinnedItem);
                                    if (index != -1)
                                    {
                                        // Remove o item antigo
                                        PinnedTabs.RemoveAt(index);

                                        // Cria um novo item com as propriedades atualizadas
                                        var updatedItem = new TabItem
                                        {
                                            url = clickedItem.url,
                                            TabName = clickedItem.TabName,
                                            FullTabName = clickedItem.FullTabName,
                                            LogoUrl = clickedItem.LogoUrl,
                                            isProtected = clickedItem.isProtected

                                            // Adicione outras propriedades necessárias aqui
                                        };

                                        // Insere o novo item na mesma posição
                                        PinnedTabs.Insert(index, updatedItem);
                                        SavePinneds();
                                    }
                                }



                            }
                        }
                    }
                }
            }
        }



        private void PinnedItemMenu_Unpin(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                // Obtenha o elemento que foi alvo do ContextMenu
                if (contextMenu.PlacementTarget is FrameworkElement element)
                {
                    // Obtenha o item de dados associado ao DataContext
                    var clickedItem = element.DataContext as TabItem;
                    if (clickedItem != null)
                    {
                        // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia
                        string currentName = clickedItem.TabName;
                        ToastWindow.Show("Site unpinned: " + currentName);
                        PinnedTabs.Remove(clickedItem);
                        SavePinneds();

                    }
                }
            }
        }

        private void PinnedItemMenu_Open(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                // Obtenha o elemento que foi alvo do ContextMenu
                if (contextMenu.PlacementTarget is FrameworkElement element)
                {
                    // Obtenha o item de dados associado ao DataContext
                    var clickedItem = element.DataContext as TabItem;
                    if (clickedItem != null)
                    {
                        // Aqui você pode abrir uma caixa de diálogo para editar o nome da guia
                        if (clickedItem.isProtected == false)
                        {
                            string currentName = clickedItem.TabName;
                            string url = clickedItem.url;
                            OpenNewTabWithUrl(url, currentName);
                        }
                        else
                        {
                            Vault vault = new Vault();
                            if (vault.HasVaultExist())
                            {
                                PromptWindow promptWindow = new PromptWindow("", "Password:");
                                if (promptWindow.ShowDialog() == true)
                                {
                                    if (!string.IsNullOrEmpty(promptWindow.UserInput))
                                    {
                                        string decryptedPassword = vault.GetStoredPassword();
                                        string password = promptWindow.UserInput;
                                        if (password == decryptedPassword)
                                        {
                                            string currentName = clickedItem.TabName;
                                            string url = vault.Decrypt(clickedItem.url);
                                            ToastWindow.Show("Opening tab.");
                                            OpenNewTabWithUrl(url, currentName);
                                        }
                                        else
                                        {
                                            ToastWindow.Show("Wrong password.");
                                        }

                                    }
                                }


                            }

                        }


                    }
                }
            }
        }

        private void PinnedItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // Tenta encontrar o recurso ContextMenu com a chave "TabItemMenu"
                if (FindResource("PinnedItemMenu") is ContextMenu menu)
                {
                    selectedTabItemContextMenu = (TabItem)element.DataContext;
                    // Define o elemento como o alvo do menu de contexto e abre o menu
                    menu.PlacementTarget = element;
                    menu.IsOpen = true;

                }
                else
                {
                    // Tratar o caso em que o recurso não é encontrado
                    Console.WriteLine("ContextMenu 'TabItemMenu' não encontrado.");
                }
            }
            else
            {
                // Tratar o caso em que o sender não é do tipo esperado
                Console.WriteLine("O sender não é do tipo esperado (FrameworkElement).");
            }
        }

        private void TabItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Verifica se o sender é do tipo esperado, como StackPanel ou outro elemento de TabItem
            if (sender is FrameworkElement element)
            {
                // Tenta encontrar o recurso ContextMenu com a chave "TabItemMenu"
                if (FindResource("TabItemMenu") is ContextMenu menu)
                {
                    _isContextMenuOpen = true;
                    selectedTabItemContextMenu = (TabItem)element.DataContext;
                    // Define o elemento como o alvo do menu de contexto e abre o menu
                    menu.PlacementTarget = element;
                    menu.IsOpen = true;
                    menu.Closed += TabMenu_Closed;
                    tabMenuIsOpen = true;
                }
                else
                {
                    // Tratar o caso em que o recurso não é encontrado
                    Console.WriteLine("ContextMenu 'TabItemMenu' não encontrado.");
                }
            }
            else
            {
                // Tratar o caso em que o sender não é do tipo esperado
                Console.WriteLine("O sender não é do tipo esperado (FrameworkElement).");
            }
        }

        

        private void TabMenu_Closed(object sender, RoutedEventArgs e)
        {
            // Define tabMenuIsOpen como false quando o menu é fechado
            tabMenuIsOpen = false;

            // Desassina o evento Closed para evitar múltiplas assinaturas
            if (sender is ContextMenu menu)
            {
                menu.Closed -= TabMenu_Closed;
            }
        }



        private void QuitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            if (settings.ShowDialog() == true)
            {

            }
            else
            {
                LoadSettings();
            }
        }

        private void SidebarThemeMenu_Click(object sender, RoutedEventArgs e)
        {
            ThemeColorPicker themeColorPicker = new ThemeColorPicker();

            // Subscribing to the OnColorSelected event
            themeColorPicker.OnColorSelected += (backgroundColor) =>
            {


                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    // Aplica a cor de fundo selecionada aos elementos necessários
                    Sidebar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    TabListBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    Janela.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    CustomBarWindow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    FoldersTreeView.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
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

                    var contextMenuTabs = (ContextMenu)FindResource("TabItemMenu");
                    contextMenuTabs.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    foreach (var item in contextMenuTabs.Items)
                    {
                        if (item is MenuItem menuItem)
                        {
                            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                            menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                        }
                    }

                    var contextMenuPins = (ContextMenu)FindResource("PinnedItemMenu");
                    contextMenuPins.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    foreach (var item in contextMenuPins.Items)
                    {
                        if (item is MenuItem menuItem)
                        {
                            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                            menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                        }
                    }

                    var contextMenuApps = (ContextMenu)FindResource("AppsMenu");
                    contextMenuApps.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                    foreach (var item in contextMenuApps.Items)
                    {
                        if (item is MenuItem menuItem)
                        {
                            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
                            menuItem.Foreground = new SolidColorBrush(Colors.White); // Cor do texto branco
                        }
                    }

                    // Persiste a cor de fundo selecionada
                    BackgroundPersist backgroundPersist = new BackgroundPersist();
                    backgroundPersist.SaveColor(backgroundColor);
                }
            };

            themeColorPicker.Show();
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
                if (tab.WebView != null && (tab.WebView.Source.ToString().Contains("youtube.com")))
                {
                    await tab.WebView.ExecuteScriptAsync(script);
                }
            }
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Start animation when navigation starts
            WhiteBar.Visibility = Visibility.Visible;
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
                if (webView.Source.ToString().IndexOf("youtube.com") == -1)
                {
                    if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
                    {
                        if (selectedTab.adBlock == true)
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync(adBlockScript);

                        }
                    }

                }
                
                var storyboard = (Storyboard)this.Resources["LoadingAnimation"];
                storyboard.Stop();
                WhiteBar.Visibility = Visibility.Collapsed;
                var title = await webView.CoreWebView2.ExecuteScriptAsync("document.title");
                title = title.Trim('"'); // Remove the surrounding quotes
                string url = webView.Source.ToString();
                //string domain = new Uri(url).GetLeftPart(UriPartial.Authority); // Obtém o domínio da URL
                //string faviconUrl = $"{domain}/favicon.ico";
                //string favUrl = $"https://www.google.com/s2/favicons?sz=64&domain_url={url}";
                string faviconUrl = GetFaviconUrl(url);
                LogNavigationDetails(url, title, faviconUrl);

                if (string.IsNullOrEmpty(faviconUrl) || faviconUrl == "html.png")
                {
                    faviconUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html.png");
                    
                }


                var tabItem = Tabs.FirstOrDefault(tab => tab.WebView == webView);
                if (tabItem != null)
                {
                    tabItem.OnNavigationCompleted();
                    if (title.Length > 13)
                    {
                        if (tabItem.isRenamed == false)
                        {
                            tabItem.TabName = title.Substring(0, 13);
                        }



                    }
                    else
                    {
                        if (tabItem.isRenamed == false)
                        {
                            tabItem.TabName = title;
                        }

                    }
                    tabItem.LogoUrl = faviconUrl;
                    tabItem.FullTabName = title;

                    if (url.IndexOf("http") != -1)
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
                        if (musicTitle is not null)
                        {
                            MusicTitle.Text = musicTitle;
                            if (musicTitle.IndexOf(" - YouTube Music") != -1)
                            {
                                MiniPlayerPlayBtn.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MiniPlayerPlayBtn.Visibility = Visibility.Visible;
                            }
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
            ConfirmDialog confirmDialog = new ConfirmDialog("Do you want to close all open tabs?");
            if(confirmDialog.ShowDialog() == true)
            {
                foreach (var tabItem in Tabs.ToList())
                {
                    // Dispose of the WebView2 control
                    tabItem.WebView?.Dispose();
                }

                // Clear all tabs
                Tabs.Clear();



                UpdateMiniPlayerVisibility();
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
                        catch (Exception ex)
                        {
                            if (droppedData.isProtected == false)
                            {
                                OpenNewTabWithUrl(droppedData.url);
                                ToastWindow.Show("Opening tab.");
                            }
                            else
                            {
                                Vault vault = new Vault();
                                if (vault.HasVaultExist())
                                {
                                    PromptWindow promptWindow = new PromptWindow("", "Password:");
                                    if (promptWindow.ShowDialog() == true)
                                    {
                                        if (!string.IsNullOrEmpty(promptWindow.UserInput))
                                        {
                                            string decryptedPassword = vault.GetStoredPassword();
                                            string password = promptWindow.UserInput;
                                            if (password == decryptedPassword)
                                            {
                                                string currentName = droppedData.TabName;
                                                string url = vault.Decrypt(droppedData.url);
                                                ToastWindow.Show("Opening tab.");
                                                OpenNewTabWithUrl(url, currentName);
                                            }
                                            else
                                            {
                                                ToastWindow.Show("Wrong password.");
                                            }

                                        }
                                    }


                                }

                            }


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
                        catch (Exception ex)
                        {
                            droppedData.url = urlLabel.Text;



                            bool exists = false;
                            foreach (var tab in PinnedTabs)
                            {
                                if (tab.url == droppedData.url)
                                {
                                    exists = true;
                                    break;
                                }
                            }
                            if (!exists)
                            {
                                PinnedTabs.Add(droppedData);
                                ToastWindow.Show($"Pinned site: {droppedData.url}");
                                SavePinneds();
                                droppedData.WebView.Dispose();
                                Tabs.Remove(droppedData);
                            }

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

        private void MiniPlayer_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        #region FOLDERS

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (TabListBox.SelectedItem is TabItem selectedTab && selectedTab.WebView != null)
            {
                var selectedItem = FoldersTreeView.SelectedItem;
                if (selectedItem is Folder selectedFolder)
                {
                    Site newSite = new Site
                    {
                        TabName = selectedTab.TabName,
                        FullTabName = selectedTab.FullTabName,
                        LogoUrl = selectedTab.LogoUrl,
                        url = selectedTab.WebView.Source.ToString()
                    };
                    selectedFolder.sites.Add(newSite);
                    FoldersTreeView.Items.Refresh();
                    SaveFolders();
                }
                else
                {
                    ToastWindow.Show("Select a folder.");
                }
            }
        }

        private void FoldersTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = FoldersTreeView.SelectedItem;
            if(selectedItem is not null)
            {
                if(selectedItem is Folder folder)
                {
                    ToastWindow.Show("Folder: "+folder.folderName + " selected");
                }
                
            }
        }


        private void RemoveSiteFolder_Click(object sender, RoutedEventArgs e)
        {
            // Get the current site from the context menu's DataContext
            var site = (e.Source as MenuItem)?.DataContext as Site;
            if (site != null)
            {
                // Remove the site from the collection of sites
                RemoveSite(site);
            }
        }



        private void RemoveSite(Site site)
        {
            var folders = FoldersTreeView.ItemsSource as List<Folder>;
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    if (folder.sites.Contains(site))
                    {
                        folder.sites.Remove(site);
                        FoldersTreeView.Items.Refresh();
                        SaveFolders();
                        break;
                    }
                }
            }
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FoldersTreeView.SelectedItem is Folder selectedFolder)
            {
                var folders = FoldersTreeView.ItemsSource as List<Folder>;
                if (folders != null)
                {
                    folders.Remove(selectedFolder);
                    FoldersTreeView.Items.Refresh();
                }
            }
            SaveFolders();
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            addFolder();
        }

        private void addFolder()
        {
            PromptWindow promptWindow = new PromptWindow("","Folder name:");
            if (promptWindow.ShowDialog() == true)
            {
                string name = promptWindow.UserInput;
                if (!string.IsNullOrEmpty(name))
                {
                    Folder newFolder = new Folder
                    {
                        folderName = name,
                        sites = new List<Site>()
                    };

                    var folders = FoldersTreeView.ItemsSource as List<Folder>;
                    if (folders == null)
                    {
                        // Se estiver vazia ou nula, cria uma nova lista de pastas
                        folders = new List<Folder>();
                        FoldersTreeView.ItemsSource = folders;
                    }

                    // Adiciona a nova pasta à lista
                    folders.Add(newFolder);
                    FoldersTreeView.Items.Refresh();
                    SaveFolders();

                }
            }
        }

        private void SaveFolders()
        {
            // Obtém a lista de pastas do TreeView
            var folders = FoldersTreeView.ItemsSource as List<Folder>;

            if (folders != null)
            {
                // Serializa a lista de pastas para JSON
                string json = JsonConvert.SerializeObject(folders, Formatting.Indented);

                // Obtém o diretório onde o executável está sendo executado
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Combina o diretório do executável com o nome do arquivo JSON
                string jsonFilePath = Path.Combine(exeDirectory, "folders.json");

                // Grava o JSON no arquivo
                File.WriteAllText(jsonFilePath, json);
            }
        }

        private void Folder_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var stackPanel = sender as TextBlock;
            if (stackPanel != null)
            {
                var folder = stackPanel.DataContext as Folder;
                if (folder != null)
                {
                    // Find the context menu
                    ContextMenu removeFolderMenu = this.FindResource("RemoveFolderMenu") as ContextMenu;
                    if (removeFolderMenu != null)
                    {
                        _isContextMenuOpen = true;
                        // Set the current site as the context menu's DataContext
                        removeFolderMenu.DataContext = folder;
                        // Display the context menu on the StackPanel
                        removeFolderMenu.PlacementTarget = stackPanel;
                        removeFolderMenu.IsOpen = true;
                    }
                    // Mark the event as handled to prevent propagation
                    e.Handled = true;
                }
            }
        }


        private void Site_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var stackPanel = sender as StackPanel;
            if (stackPanel != null)
            {
                var site = stackPanel.DataContext as Site;
                if (site != null)
                {
                    // Find the context menu
                    ContextMenu removeSiteFolderMenu = this.FindResource("RemoveSiteFolderMenu") as ContextMenu;
                    if (removeSiteFolderMenu != null)
                    {
                        _isContextMenuOpen = true;
                        // Set the current site as the context menu's DataContext
                        removeSiteFolderMenu.DataContext = site;
                        // Display the context menu on the StackPanel
                        removeSiteFolderMenu.PlacementTarget = stackPanel;
                        removeSiteFolderMenu.IsOpen = true;
                    }
                    // Mark the event as handled to prevent propagation
                    e.Handled = true;
                }
            }
        }

        private const int DoubleClickTime = 300;
        private DateTime _lastClickTime;
        private void Site_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentTime = DateTime.Now;
            var timeSinceLastClick = currentTime - _lastClickTime;

            if (timeSinceLastClick.TotalMilliseconds <= DoubleClickTime)
            {
                // É um duplo clique
                var stackPanel = sender as StackPanel;
                if (stackPanel != null)
                {
                    var site = stackPanel.DataContext as Site;
                    if (site != null)
                    {
                        OpenNewTabWithUrl(site.url, site.TabName);
                    }
                }

                e.Handled = true;
            }

            _lastClickTime = currentTime;
        }



        private void LoadFolders()
        {
            // Obtém o diretório onde o executável está sendo executado
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combina o diretório do executável com o nome do arquivo JSON
            string jsonFilePath = Path.Combine(exeDirectory, "folders.json");
            if (File.Exists(jsonFilePath))
            {
                // Lê o conteúdo do arquivo JSON
                string json = File.ReadAllText(jsonFilePath);

                // Desserializa o JSON para uma lista de objetos Folder
                var folders = JsonConvert.DeserializeObject<List<Folder>>(json);

                // Define o ItemsSource do TreeView para a lista de pastas
                FoldersTreeView.ItemsSource = folders;
            }

            
            
        }

        #endregion

    }

    public class Site
    {
        public string TabName { get; set; }
        public string FullTabName { get; set; }
        public string LogoUrl { get; set; }
        public string url { get; set; }
    }

    public class Folder
    {
        public string folderName { get; set; }
        public List<Site> sites { get; set; }
    }

    public class NavigationLogEntry
    {
        public string Url { get; set; }

        public string UrlIco { get; set; }
        public string Title { get; set; }
        public DateTime AccessedAt { get; set; }
    }

    class FirebaseStorageResponse
    {
        public List<FirebaseStorageItem> Items { get; set; }
    }

    public class ExpandCollapseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isExpanded = (bool)value;
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Caminho da pasta onde o executável está
            if (isExpanded)
            {
                return new BitmapImage(new Uri(System.IO.Path.Combine(basePath, "Icons", "folder_open.png")));
            }
            else
            {
                return new BitmapImage(new Uri(System.IO.Path.Combine(basePath, "Icons", "folder.png")));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class FirebaseStorageItem
    {
        public string Name { get; set; }
    }

    public class Account
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class TabItem
    {
        public string TabName { get; set; }

        public string FullTabName { get; set; }

        public string LogoUrl { get; set; }

        public string url { get; set; }

        public bool adBlock { get; set; }

        public bool isProtected { get; set; }

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
