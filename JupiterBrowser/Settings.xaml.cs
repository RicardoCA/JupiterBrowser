using Newtonsoft.Json;
using System.IO;
using File = System.IO.File;
using System.Windows;
using IWshRuntimeLibrary;
using System.Reflection;

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string[] restaureFiles = { "calc.json", "navigationLog.json", "pinneds.json", "sidebar.json", "siteColors.json", "vault.json", "settings.json", "account.json", "closedtabs.json","folders.json" };
        private const string SettingsFilePath = "settings.json";
        private string language = "en-US";
        public Settings()
        {
            InitializeComponent();
            LoadSettings();


        }



        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var jsonString = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<BrowserSettings>(jsonString);

                    if (settings != null)
                    {
                        SetSelectedLanguage(settings.DefaultTranslateLanguage);
                        SetPreviousNavigation(settings.PreviousNavigation);
                        SetSearchEngine(settings.SearchEngine);
                        SetMiniWindow(settings.MiniWindow);
                        SetSelectedInterfaceLanguage(settings.Language);
                        language = settings.Language;
                        UpdateUI();
                    }
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Failed to load settings: {ex.Message}");
            }
        }

        private void EnableDisableStartup(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutName = "Jupiter Browser.lnk"; // Nome do atalho
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolderPath, shortcutName);

                // Obtém o caminho do executável (.exe)
                string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string appDirectory = Path.GetDirectoryName(appPath); // Diretório do .exe

                if (!System.IO.File.Exists(shortcutPath))
                {
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.Description = "Jupiter Browser"; // Descrição do atalho
                    shortcut.TargetPath = appPath;
                    shortcut.WorkingDirectory = appDirectory; // Define o diretório de trabalho
                    shortcut.Save();

                    ToastWindow.Show("Jupiter Browser set to start with the operating system.");
                }
                else
                {
                    System.IO.File.Delete(shortcutPath);
                    ToastWindow.Show("Jupiter Browser set to not start with the operating system.");
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show("Error: " + ex.Message);
            }
        }

        private void SetSelectedInterfaceLanguage(string language)
        {
            switch (language)
            {
                case "en-US":
                    enUSBtn.IsChecked = true;
                    language = "en-US";
                    UpdateUI();
                    break;
                case "pt-BR":
                    ptBRBtn.IsChecked = true;
                    language = "pt-BR";
                    UpdateUI();
                    break;
                case "ES":
                    esBtn.IsChecked= true;
                    language = "ES";
                    UpdateUI();
                    break;
                default:
                    enUSBtn.IsChecked = true;
                    language = "en-US";
                    UpdateUI();
                    break;
            }
        }

        private void UpdateUI()
        {
            if(language == "en-US")
            {
                translateTitle.Text = "Default Translate language";
                translateLabel.Text = "Sets the default language for the 'Translate' option of the open tab.";
                PreviusNavigationTitle.Text = "Previous navigation";
                PreviusNavigationLabel.Text = "Sets your preference when opening the browser, whether you want to open closed tabs again, or start browsing a new one.";
                SearchEngineTitle.Text = "Default Search Engine";
                SearchEngineLabel.Text = "Set your default search engine preference.";
                MiniWindowTitle.Text = "Mini Window";
                MiniWindowLabel.Text = "Select your preference, whether you want to open new tabs in mini window or in new tab.";
                LanguageTitle.Text = "Language";
                LanguageLabel.Text = "Set your language, and restart browser.";
                StartupTitle.Text = "Startup with Windows";
                StartupLabel.Text = "Enable or disable startup with Windows.";
                DefaultSettingsTitle.Text = "Default Settings";
                DefaultSettingsLabel.Text = "Restores the browser's default settings, this includes deleting all configuration parameters.";
                DeleteRecomendationsTitle.Text = "Delete Site Recomendations";
                DeleteRecomendationsLabel.Text = "Only delete the Ctrl+T and Ctrl+L website recommendation file";
                ApplyBtn.Content = "Apply";
                CloseBtn.Content = "Close";
                DeleteRecomendationsBtn.Content = "Delete Recomendations";
                DefaultSettingsBtn.Content = "Restaure";
                StartupBtn.Content = "Enable/Disable";
                MiniWindowTrue.Content = "Enabled (default)";
                MiniWindowFalse.Content = "Disabled";
            }
            if(language == "pt-BR")
            {
                translateTitle.Text = "Idioma padrão da tradução";
                translateLabel.Text = "Define o idioma padrão para a opção \"Traduzir\" da aba aberta.";
                PreviusNavigationTitle.Text = "Navegação anterior";
                PreviusNavigationLabel.Text = "Define sua preferência ao abrir o navegador, se você deseja abrir abas fechadas novamente ou começar a navegar em uma nova.";
                SearchEngineTitle.Text = "Motor de busca padrão";
                SearchEngineLabel.Text = "Defina sua preferência de mecanismo de busca padrão.";
                MiniWindowTitle.Text = "Mini janela";
                MiniWindowLabel.Text = "Selecione sua preferência, se deseja abrir novas abas em mini janela ou em nova aba.";
                LanguageTitle.Text = "Linguagem";
                LanguageLabel.Text = "Defina seu idioma e reinicie o navegador.";
                StartupTitle.Text = "Inicialização com Windows";
                StartupLabel.Text = "Habilite ou desabilite a inicialização com Windows.";
                DefaultSettingsTitle.Text = "Configurações padrão";
                DefaultSettingsLabel.Text = "Restaura as configurações padrão do navegador, o que inclui a exclusão de todos os parâmetros de configuração.";
                DeleteRecomendationsTitle.Text = "Excluir Recomendações de Site";
                DeleteRecomendationsLabel.Text = "Exclua apenas o arquivo de recomendação do site Ctrl+T e Ctrl+L";
                ApplyBtn.Content = "Aplicar";
                CloseBtn.Content = "Fechar";
                DeleteRecomendationsBtn.Content = "Excluir Recomendações";
                DefaultSettingsBtn.Content = "Restaurar";
                StartupBtn.Content = "Ativar/Desativar";
                MiniWindowTrue.Content = "Ativado (default)";
                MiniWindowFalse.Content = "Desativado";
            }
            if(language == "ES")
            {
                translateTitle.Text = "Idioma de traducción predeterminado";
                translateLabel.Text = "Establece el idioma predeterminado para la opción 'Traducir' de la pestaña abierta.";
                PreviusNavigationTitle.Text = "Navegación anterior";
                PreviusNavigationLabel.Text = "Establece su preferencia al abrir el navegador, si desea volver a abrir las pestañas cerradas o comenzar a navegar por una nueva.";
                SearchEngineTitle.Text = "Motor de búsqueda predeterminado";
                SearchEngineLabel.Text = "Establezca su preferencia de motor de búsqueda predeterminado.";
                MiniWindowTitle.Text = "Mini ventana";
                MiniWindowLabel.Text = "Seleccione su preferencia, si desea abrir nuevas pestañas en mini ventana o en nueva pestaña.";
                LanguageTitle.Text = "Lenguaje";
                LanguageLabel.Text = "Defina su idioma y reinicie el navegador.";
                StartupTitle.Text = "Inicio con Windows";
                StartupLabel.Text = "Habilite o deshabilite el inicio con Windows.";
                DefaultSettingsTitle.Text = "Configuración predeterminada";
                DefaultSettingsLabel.Text = "Restaura la configuración predeterminada del navegador, lo que incluye la eliminación de todos los parámetros de configuración.";
                DeleteRecomendationsTitle.Text = "Eliminar recomendaciones de sitios";
                DeleteRecomendationsLabel.Text = "Elimine únicamente el archivo de recomendaciones de sitios web con Ctrl+T y Ctrl+L";
                ApplyBtn.Content = "Aplicar";
                CloseBtn.Content = "Cerrar";
                DeleteRecomendationsBtn.Content = "Eliminar recomendaciones";
                DefaultSettingsBtn.Content = "Restaurar";
                StartupBtn.Content = "Habilitar/Deshabilitar";
                MiniWindowTrue.Content = "Activado (default)";
                MiniWindowFalse.Content = "Desactivado";
            }
        }

        private void SetSelectedLanguage(string language)
        {
            switch (language)
            {
                case "English":
                    EnglishRadioButton.IsChecked = true;
                    break;
                case "Português":
                    PortugueseRadioButton.IsChecked = true;
                    break;
                case "Español":
                    SpanishRadioButton.IsChecked = true;
                    break;
                default:
                    EnglishRadioButton.IsChecked = true;
                    break;
            }
        }

        private void SetMiniWindow(string miniWindow)
        {
            switch (miniWindow)
            {
                case "MiniWindowTrue":
                    MiniWindowTrue.IsChecked = true;
                    break;
                case "MiniWindowFalse":
                    MiniWindowFalse.IsChecked = true;
                    break;
                default:
                    MiniWindowTrue.IsChecked = true;
                    break;
            }
        }

        private void SetSearchEngine(string engine)
        {
            switch (engine)
            {
                case "Google":
                    Google.IsChecked = true;
                    break;
                case "Bing":
                    Bing.IsChecked = true;
                    break;
                case "Duckduckgo":
                    Duckduckgo.IsChecked = true;
                    break;
                case "Perplexity":
                    Perplexity.IsChecked = true;
                    break;
                case "Morphic":
                    Morphic.IsChecked = true;
                    break;
                default:
                    Google.IsChecked = true;
                    break;
            }
        }

        private void SetPreviousNavigation(string navigation)
        {
            switch (navigation)
            {
                case "Question":
                    QuestionRadioButton.IsChecked = true;
                    break;
                case "ReopenTabs":
                    ReopenTabsRadioButton.IsChecked = true;
                    break;
                case "StartNewNavigation":
                    StartNewNavigationRadioButton.IsChecked = true;
                    break;
                default:
                    QuestionRadioButton.IsChecked = true;
                    break;
            }
        }


        private void DeleteRecomendations(object sender, RoutedEventArgs e)
        {
            if (File.Exists("navigationLog.json"))
            {
                File.Delete("navigationLog.json");
                ToastWindow.Show("Navigation recommendations deleted.");
            }
            else
            {
                ToastWindow.Show("There are no navigation recommendations to delete yet.");
            }
        }

        private void RestaureBrowser(object sender, RoutedEventArgs e)
        {
            foreach (string file in restaureFiles)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        ToastWindow.Show($"Erro ao deletar o arquivo {file}: {ex.Message}");

                    }
                }
            }
            ToastWindow.Show("Browser restored to factory default.");
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }

        private void ApplyClick(object sender, RoutedEventArgs e)
        {
            var settings = new BrowserSettings
            {
                DefaultTranslateLanguage = GetSelectedLanguage(),
                PreviousNavigation = GetPreviousNavigation(),
                SearchEngine = GetSearchEngine(),
                MiniWindow = GetMiniWindow(),
                Language = GetSelectedInterfaceLanguage(),
            };

            SaveSettings(settings);
            ToastWindow.Show("Settings have been applied.");



        }

        private string GetMiniWindow()
        {
            if (MiniWindowTrue.IsChecked == true)
            {
                return "MiniWindowTrue";
            }
            if (MiniWindowFalse.IsChecked == true)
            {
                return "MiniWindowFalse";
            }
            return "MiniWindowTrue";

        }

        private string GetSearchEngine()
        {
            if (Google.IsChecked == true)
                return "Google";
            if (Bing.IsChecked == true)
                return "Bing";
            if (Duckduckgo.IsChecked == true)
                return "Duckduckgo";
            if (Perplexity.IsChecked == true)
                return "Perplexity";
            if (Morphic.IsChecked == true)
                return "Morphic";

            return "Google"; // Default
        }

        private string GetSelectedInterfaceLanguage()
        {
            if(enUSBtn.IsChecked == true)
            {
                UpdateUI();
                return "en-US";
            }
            if(ptBRBtn.IsChecked == true)
            {
                UpdateUI();
                return "pt-BR";
            }
            if(esBtn.IsChecked == true)
            {
                UpdateUI();
                return "ES";
            }
            UpdateUI();
            return "en-US";
        }

        private string GetSelectedLanguage()
        {
            if (EnglishRadioButton.IsChecked == true)
                return "English";
            if (PortugueseRadioButton.IsChecked == true)
                return "Português";
            if (SpanishRadioButton.IsChecked == true)
                return "Español";

            return "English"; // Default
        }

        private string GetPreviousNavigation()
        {
            if (QuestionRadioButton.IsChecked == true)
                return "Question";
            if (ReopenTabsRadioButton.IsChecked == true)
                return "ReopenTabs";
            if (StartNewNavigationRadioButton.IsChecked == true)
                return "StartNewNavigation";

            return "Question"; // Default
        }

        private void SaveSettings(BrowserSettings settings)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, jsonString);
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Failed to save settings: {ex.Message}");
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
    public class BrowserSettings
    {
        public string DefaultTranslateLanguage { get; set; }
        public string PreviousNavigation { get; set; }

        public string MiniWindow { get; set; }

        public string SearchEngine { get; set; }

        public string Language { get; set; }
    }
}
