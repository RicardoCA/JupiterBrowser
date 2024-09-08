using Newtonsoft.Json;
using System;
using System.Collections;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para ChatBot.xaml
    /// </summary>
    public partial class ChatBot : Window
    {
        public string apiKeyChatgpt = "";
        public ChatBot()
        {
            InitializeComponent();
            LoadSettings();
        }

        private async Task<string> GenerateImageWithChatGPT(string prompt)
        {
            try
            {
                // Crie o cliente HTTP
                using (var client = new HttpClient())
                {
                    // Defina a chave de autenticação da API
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKeyChatgpt);

                    // Crie o objeto de requisição para a API ChatGPT (para geração de imagem)
                    var requestBody = new
                    {
                        model = "dall-e-3", // Substitua pelo modelo apropriado para gerar imagens
                        prompt = prompt,
                        n = 1,
                        size = "1024x1024" // Tamanho da imagem
                    };

                    // Converta o objeto de requisição em JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                    // Envie a requisição para a API
                    var response = await client.PostAsync("https://api.openai.com/v1/images/generations", jsonContent);
                    
                    // Verifique se a requisição foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        // Deserialize para o objeto ImageResponse
                        var imageResponse = JsonConvert.DeserializeObject<ImageResponse>(responseString);
                        
                        // Verifique se há dados e extraia a URL da imagem gerada
                        if (imageResponse?.data != null && imageResponse.data.Count > 0)
                        {
                            string imageUrl = imageResponse.data[0].url;
                            AddImageToLeft(imageUrl);
                            SendBtn.IsEnabled = true;
                            return imageUrl;
                        }
                        else
                        {
                            return "Error: No image data returned from API.";
                        }
                    }
                    else
                    {
                        // Capture o conteúdo de erro da API
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        ToastWindow.Show($"Error: API returned status code {response.StatusCode} with message: {errorResponse}");
                        return $"Error: API returned status code {response.StatusCode} with message: {errorResponse}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error Generate Image: {ex.Message}";
            }
        }



        private async Task<string> SendMessageToChatGPT(string message)
        {
            try
            {
                // Crie o cliente HTTP
                using (var client = new HttpClient())
                {
                    // Defina a chave de autenticação da API
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKeyChatgpt);

                    // Crie o objeto de requisição para a API ChatGPT
                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo", // Ou o modelo que você está utilizando
                        messages = new[]
                        {
                    new { role = "user", content = message }
                }
                    };

                    // Converta o objeto de requisição em JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                    // Envie a requisição para a API do ChatGPT
                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);

                    // Verifique se a requisição foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var chatGptResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                        // Extraia a resposta gerada pelo ChatGPT
                        string reply = chatGptResponse.choices[0].message.content;
                        
                        return reply;
                    }
                    else
                    {
                        return "Error: Unable to reach ChatGPT API.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
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
                        apiKeyChatgpt = settings.ApiKeyChatGPT;
                    }
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Failed to load settings: {ex.Message}");
            }
        }

        private async Task<BitmapImage> LoadImageFromUrlAsync(string imageUrl)
        {
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    // Baixa os bytes da imagem a partir da URL
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Crie um BitmapImage a partir dos bytes
                    BitmapImage bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Congela o BitmapImage para que ele possa ser usado em diferentes threads
                    }

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                // Trate qualquer erro que ocorrer ao carregar a imagem
                MessageBox.Show($"Error loading image: {ex.Message}");
                return null;
            }
        }

        private async Task SaveImageToDownloads(string imageUrl, string fileName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Baixa os bytes da imagem a partir da URL
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Caminho para salvar a imagem diretamente no diretório de Downloads do usuário
                    string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    downloadsPath = System.IO.Path.Combine(downloadsPath, "Downloads");

                    string filePath = System.IO.Path.Combine(downloadsPath, fileName);

                    // Salva a imagem no diretório de Downloads
                    await File.WriteAllBytesAsync(filePath, imageBytes);

                    ToastWindow.Show($"Image saved to {filePath}", 7000);
                }
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Error saving image: {ex.Message}", 7000);
            }
        }


        private async void AddImageToLeft(string imageUrl)
        {
            string fileName = "GeneratedImage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

            // Salva a imagem no diretório de Downloads
            await SaveImageToDownloads(imageUrl, fileName);

            // Cria um objeto Image para exibir a imagem
            Image generatedImage = new Image
            {
                Width = 400, // Defina uma largura máxima
                Height = 400,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left, // Alinha a imagem à esquerda
                Stretch = Stretch.UniformToFill // Mantém a proporção da imagem
            };

            // Carrega a imagem a partir da URL
            BitmapImage bitmap = await LoadImageFromUrlAsync(imageUrl);

            if (bitmap != null)
            {
                // Atribua a imagem ao controle Image
                generatedImage.Source = bitmap;

                ContextMenu contextMenu = new ContextMenu();
                MenuItem copyMenuItem = new MenuItem { Header = "Copy URL from Image" };
                copyMenuItem.Click += (s, e) => Clipboard.SetText(imageUrl); // Copia o texto para a área de transferência
                contextMenu.Items.Add(copyMenuItem);
                generatedImage.ContextMenu = contextMenu;

                // Adiciona o controle Image ao StackPanel (MessagesPanel)
                MessagesPanel.Children.Add(generatedImage);

                // Rola para a última mensagem
                MessagesPanel.UpdateLayout();
                var scrollViewer = MessagesPanel.Parent as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }



        private void AddMessageToLeft(string message)
        {
            // Cria um TextBlock para exibir a mensagem
            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left, // Alinha a mensagem à esquerda
                Background = new SolidColorBrush(Colors.LightGray), // Opção: Cor de fundo para diferenciar mensagens
                Padding = new Thickness(10),
                MaxWidth = 400, // Define uma largura máxima para não esticar muito
                TextWrapping = TextWrapping.Wrap // Permite que o texto quebre em várias linhas
            };

            ContextMenu contextMenu = new ContextMenu();
            MenuItem copyMenuItem = new MenuItem { Header = "Copy" };
            copyMenuItem.Click += (s, e) => Clipboard.SetText(messageTextBlock.Text); // Copia o texto para a área de transferência
            contextMenu.Items.Add(copyMenuItem);
            messageTextBlock.ContextMenu = contextMenu;

            // Adiciona o TextBlock ao StackPanel (MessagesPanel)
            MessagesPanel.Children.Add(messageTextBlock);

            // Rola para a última mensagem
            MessagesPanel.UpdateLayout();
            var scrollViewer = MessagesPanel.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }

        private async void sendMessage(string message)
        {
            // Cria um TextBlock para exibir a mensagem
            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.White),

            };

            // Adiciona o TextBlock ao StackPanel (MessagesPanel)
            MessagesPanel.Children.Add(messageTextBlock);
            SendBtn.IsEnabled = false;

            if (textIARadio.IsChecked == true)
            {
                string reply = await SendMessageToChatGPT(message);
                AddMessageToLeft(reply);
                SendBtn.IsEnabled = true;
            }
            else
            {
                AddMessageToLeft("Loading...");
                await GenerateImageWithChatGPT(message);

            }



            // Limpa o TextBox
            MessageInput.Text = "";

            // Rola para a última mensagem
            MessagesPanel.UpdateLayout();
            var scrollViewer = MessagesPanel.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            // Pega o texto do TextBox
            string message = MessageInput.Text;
            sendMessage(message);
            
        }
    }

    public class ImageResponse
    {
        public List<ImageData> data { get; set; }
    }

    public class ImageData
    {
        public string url { get; set; }
    }
}
