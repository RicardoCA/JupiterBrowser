using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace JupiterBrowser
{
    public partial class UrlInputDialog : Window
    {
        public string EnteredUrl { get; set; }
        private List<NavigationLogEntry> _history = new List<NavigationLogEntry>();

        private List<string> _suggestions = new List<string>();

        private string searchEngine = "Google";

        private int lastResult = 0;

        public UrlInputDialog()
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            UrlTextBox.Focus();
            LoadLastResult();
            LoadNavigationHistory();
            LoadSettings();
        }

        public UrlInputDialog(string url)
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            UrlTextBox.Focus();
            UrlTextBox.Text = url;
            LoadLastResult();
            LoadNavigationHistory();
            LoadSettings();
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
                        searchEngine = settings.SearchEngine switch
                        {
                            "Google" => "Google",
                            "Bing" => "Bing",
                            "Duckduckgo" => "Duckduckgo",
                            "Perplexity" => "Perplexity",
                            "Morphic" => "Morphic",
                            _ => "Google"
                        };

                        
                    }
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Failed to load settings: {ex.Message}");
            }
        }

        private void LoadNavigationHistory()
        {
            string logFilePath = "navigationLog.json";
            if (File.Exists(logFilePath))
            {
                string json = File.ReadAllText(logFilePath);
                _history = JsonConvert.DeserializeObject<List<NavigationLogEntry>>(json) ?? new List<NavigationLogEntry>();
            }
        }

        private string GetBaseUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
            }
            catch
            {
                return url; // Retorna a URL completa se não puder ser parseada
            }
        }

        private void ShowSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var uniqueUrls = new HashSet<string>();
            var suggestions = new List<NavigationLogEntry>();

            foreach (var entry in _history)
            {
                var baseUrl = GetBaseUrl(entry.Url);
                if (baseUrl.Contains(input, StringComparison.OrdinalIgnoreCase) && uniqueUrls.Add(baseUrl))
                {
                    string exePath = AppDomain.CurrentDomain.BaseDirectory;
                    // Verifica se a URL contém "reddit" e ajusta o ícone
                    if(entry.UrlIco.Contains("html.png", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.UrlIco = Path.Combine(exePath, "html.png");
                    }
                    
                    if (entry.Url.Contains("reddit.com", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.UrlIco = Path.Combine(exePath, "reddit.png");
                    }
                    else if(entry.Url.Contains("chatgpt.com", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.UrlIco = Path.Combine(exePath, "chatgpt.png");
                    }
                    else if (entry.Url.Contains("openai.com", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.UrlIco = Path.Combine(exePath, "chatgpt.png");
                    }

                    suggestions.Add(entry);
                }
            }

            SuggestionsListBox.ItemsSource = suggestions;
            SuggestionsListBox.Visibility = suggestions.Any() ? Visibility.Visible : Visibility.Collapsed;
        }


        private void LoadLastResult()
        {
            var JsonFilePath = "calc.json";
            if (File.Exists(JsonFilePath))
            {
                string json = File.ReadAllText(JsonFilePath);
                lastResult = int.Parse(json);
            }
        }

        private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UrlTextBox.Text.Contains(".com") || UrlTextBox.Text.Contains(".net") ||
                UrlTextBox.Text.Contains(".org") || UrlTextBox.Text.Contains(".gov") ||
                UrlTextBox.Text.Contains("calc:") || UrlTextBox.Text.Contains(".so") ||
                    UrlTextBox.Text.Contains(".ai"))
            {
                SearchIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchIcon.Visibility = Visibility.Visible;
            }
            ShowSuggestions(UrlTextBox.Text);
        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape) {
                this.Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUrl();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SuggestionChange(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is NavigationLogEntry selectedEntry)
            {
                UrlTextBox.Text = selectedEntry.Url;
                SuggestionsListBox.Visibility = Visibility.Collapsed;
                ProcessUrl();
            }
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUrl();
            }
        }

        public static string Calculate(string expression)
        {
            try
            {
                DataTable table = new DataTable();
                var value = table.Compute(expression, string.Empty);
                return value.ToString();
            }
            catch (Exception ex)
            {
                return "Error in calculation: " + ex.Message;
            }
        }

        private void ProcessUrl()
        {
            string url = UrlTextBox.Text;

            if (url.IndexOf("https://") == -1 && url.IndexOf("http://") == -1)
            {
                if (url.IndexOf(".com") != -1 || url.IndexOf(".net") != -1 || url.IndexOf(".gov") != -1 || url.IndexOf(".org") != -1 || url.IndexOf(".so") != -1 || url.IndexOf(".ai") != -1)
                {
                    url = "https://" + url;
                }
                else
                {
                    if(url.IndexOf("edge://") == -1)
                    {
                        if (!url.Equals("startpage"))
                        {
                            if (searchEngine.Equals("Google"))
                            {
                                url = $"https://www.google.com/search?q={url}";

                            }
                            else if (searchEngine.Equals("Bing"))
                            {
                                url = $"https://www.bing.com/search?q={url}";
                            }
                            else if (searchEngine.Equals("Duckduckgo"))
                            {
                                url = $"https://duckduckgo.com/?q={url}&ia=web";
                            }
                            else if (searchEngine.Equals("Perplexity"))
                            {
                                url = $"https://www.perplexity.ai/search?q={url}";
                            }
                            else if (searchEngine.Equals("Morphic"))
                            {
                                url = $"https://www.morphic.sh/search?q={url}";
                            }

                        }
                        if(url.Contains("chatgpt "))
                        {
                            url = UrlTextBox.Text;
                        }
                        if (url.Contains("calc:"))
                        {
                            string form = url.Substring(url.IndexOf("calc:") + 5);

                            if (string.IsNullOrWhiteSpace(form) || form.Contains("="))
                            {
                                // Evita calcular se a expressão já possui um resultado ou é inválida
                                ToastWindow.Show("Remove all result with =");
                                return;
                            }

                            // Substitui "lastresult" pelo valor do último resultado
                            form = form.Replace("lastresult", lastResult.ToString());

                            try
                            {
                                string result = Calculate(form);
                                lastResult = int.Parse(result);
                                UrlTextBox.Text = "calc:" + form + "=" + result;

                                var JsonFilePath = "calc.json";
                                string jsonContent = JsonConvert.SerializeObject(lastResult, Formatting.Indented);
                                File.WriteAllText(JsonFilePath, jsonContent);
                            }
                            catch (FormatException)
                            {
                                UrlTextBox.Text = "calc:" + form + "= Error in calculation";
                            }

                            return;
                        }


                    }
                    
                }
            }
            else
            {
                if (url.IndexOf(".com") == -1 && url.IndexOf(".net") == -1 && url.IndexOf(".gov") == -1 && url.IndexOf(".org") == -1 && url.IndexOf(".so") == -1 && url.IndexOf(".ai") == -1 )
                {
                    if (searchEngine.Equals("Google"))
                    {
                        url = $"https://www.google.com/search?q={url}";

                    }
                    else if (searchEngine.Equals("Bing"))
                    {
                        url = $"https://www.bing.com/search?q={url}";
                    }
                    else if (searchEngine.Equals("Duckduckgo"))
                    {
                        url = $"https://duckduckgo.com/?q={url}&ia=web";
                    }
                    else if (searchEngine.Equals("Perplexity"))
                    {
                        url = $"https://www.perplexity.ai/search?q={url}";
                    }
                    else if (searchEngine.Equals("Morphic"))
                    {
                        url = $"https://www.morphic.sh/search?q={url}";
                    }

                }
            }

            EnteredUrl = url;
            DialogResult = true;
        }
    }
}
