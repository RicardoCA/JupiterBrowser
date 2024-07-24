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
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string currentVersion = "0.6"; // A versão atual da aplicação
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "JupiterBrowser";

        public MainWindow()
        {
            InitializeComponent();
            lblText.Content = string.Empty;
        }

        public static async Task DownloadFileAsync(string fileId, string destinationFilePath)
        {
            try
            {
                UserCredential credential;

                // Load client secrets from the credentials.json file
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore("token.json", true));
                }

                // Create the Google Drive API service
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define the request for file download
                var request = service.Files.Get(fileId);
                var file = await request.ExecuteAsync();

                // Ensure the destination file path is correctly set
                using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                {
                    await request.DownloadAsync(fileStream);
                }

                MessageBox.Show("File downloaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }



        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            string serverVersion = GetServerVersion();

            if (serverVersion != currentVersion)
            {
                lblText.Content= "New version available. Updating...";
                DownloadUpdate();
                //ApplyUpdate();
            }
            else
            {
                lblText.Content= "You are already using the latest version.";
            }
        }


        
        



        static async void DownloadUpdate()
    {
            string fileId = "1f64z6fWVqmea6FQuPSHe5BG63hm2Y1-o";
            string destinationFilePath = "JupiterBrowser.zip";
            await DownloadFileAsync(fileId, destinationFilePath);


        }



    static void ApplyUpdate()
        {
            // Descompacta e substitui os arquivos antigos
            System.IO.Compression.ZipFile.ExtractToDirectory("JupiterBrowser.zip", "JupiterBrowser", true);
            foreach (var file in Directory.GetFiles("JupiterBrowser"))
            {
                File.Copy(file, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file)), true);
            }

            // Inicia a aplicação principal
            System.Diagnostics.Process.Start("JupiterBrowser.exe");
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