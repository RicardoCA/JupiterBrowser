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
using System.IO;

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string[] restaureFiles = { "calc.json", "navigationLog.json", "pinneds.json", "sidebar.json", "siteColors.json", "vault.json", "settings.json" };

        public Settings()
        {
            InitializeComponent();
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
            ToastWindow.Show("Browser restored to factory default.\nRestart the Jupiter Browser.");
        }

        private void ApplyClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
