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
    }
}
