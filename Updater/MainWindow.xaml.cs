using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Path = System.IO.Path;
using System.Diagnostics;


namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string currentVersion = "0.20"; // A versão atual da aplicação
        

        public MainWindow()
        {
            InitializeComponent();
            lblText.Content = string.Empty;
            currentV.Content = string.Empty;
        }

        



        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            string serverVersion = GetServerVersion();

            if (serverVersion != currentVersion)
            {
                lblText.Content= "New version available. Updating...";
                DownloadUpdate();
                ApplyUpdate();
                Application.Current.Shutdown();
            }
            else
            {
                lblText.Content= "You are already using the latest version.";
            }
        }


        
        



        static async void DownloadUpdate()
    {
            
            string destinationFilePath = "JupiterBrowser.zip";
            // Baixa o novo pacote de atualização do GitHub Releases
            using (WebClient client = new WebClient())
            {
                // Substitua pelo link direto do arquivo update.zip no GitHub
                string updateUrl = "https://github.com/RicardoCA/JupiterBrowser/releases/download/update/JupiterBrowser.zip";
                client.DownloadFile(updateUrl,destinationFilePath);
            }
            


        }



    static void ApplyUpdate()
        {

            foreach (var process in Process.GetProcessesByName("JupiterBrowser"))
            {
                process.Kill();
                process.WaitForExit(); // Aguarda até que o processo seja encerrado
            }

            // Descompacta e substitui os arquivos antigos
            System.IO.Compression.ZipFile.ExtractToDirectory("JupiterBrowser.zip", "JupiterBrowser", true);
            foreach (var file in Directory.GetFiles("JupiterBrowser"))
            {
                File.Copy(file, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file)), true);
            }
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string targetDirectory = Path.Combine(baseDirectory, "JupiterBrowser");
            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }
            // Inicia a aplicação principal
            Process.Start("JupiterBrowser.exe");
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
    }
}