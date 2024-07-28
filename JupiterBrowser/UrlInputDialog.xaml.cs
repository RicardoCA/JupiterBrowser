using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

namespace JupiterBrowser
{
    public partial class UrlInputDialog : Window
    {
        public string EnteredUrl { get; set; }
        private List<string> _history = new List<string>();
        
        private int lastResult = 0;

        public UrlInputDialog()
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            UrlTextBox.Focus();
        }

        public UrlInputDialog(string url)
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            UrlTextBox.Focus();
            UrlTextBox.Text = url;
        }

        private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UrlTextBox.Text.Contains(".com") || UrlTextBox.Text.Contains(".net") ||
                UrlTextBox.Text.Contains(".org") || UrlTextBox.Text.Contains(".gov") ||
                UrlTextBox.Text.Contains("calc:"))
            {
                SearchIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchIcon.Visibility = Visibility.Visible;
            }
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
                if (url.IndexOf(".com") != -1 || url.IndexOf(".net") != -1 || url.IndexOf(".gov") != -1 || url.IndexOf(".org") != -1)
                {
                    url = "https://" + url;
                }
                else
                {
                    if(url.IndexOf("edge://") == -1)
                    {
                        if (!url.Equals("startpage"))
                        {
                            url = $"https://www.google.com/search?q={url}";
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
                if (url.IndexOf(".com") == -1 && url.IndexOf(".net") == -1 && url.IndexOf(".gov") == -1 && url.IndexOf(".org") == -1)
                {
                    url = $"https://www.google.com/search?q={url}";
                }
            }

            EnteredUrl = url;
            DialogResult = true;
        }
    }
}
