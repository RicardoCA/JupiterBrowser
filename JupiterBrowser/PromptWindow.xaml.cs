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
    /// Lógica interna para PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window
    {
        public string UserInput { get; private set; } = "";

        public string language = "en-US";
        public PromptWindow(string language)
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            textPrompt.Focus();
            this.language = language;
            if(language == "en-US")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Close";
            }
            if(language == "pt-BR")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Fechar";
            }
            if(language == "ES")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Cerrar";
            }
        }

        public PromptWindow(string text, string title = null, string language = "en-US")
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            textPrompt.Focus();
            this.language = language;
            if (language == "en-US")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Close";
            }
            if (language == "pt-BR")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Fechar";
            }
            if (language == "ES")
            {
                OkBtn.Content = "Ok";
                CloseBtn.Content = "Cerrar";
            }
            if (textPrompt.Text != null)
            {
                textPrompt.Text = text;
            }
            
            if(title != null)
            {
                labelPrompt.Text = title;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void PromptTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserInput = textPrompt.Text;
                DialogResult = true;
                this.Close();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            UserInput = textPrompt.Text;
            DialogResult = true;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }


    }
}
