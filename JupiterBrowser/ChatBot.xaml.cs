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

            // Adiciona o TextBlock ao StackPanel (MessagesPanel)
            MessagesPanel.Children.Add(messageTextBlock);

            // Rola para a última mensagem
            MessagesPanel.UpdateLayout();
            var scrollViewer = MessagesPanel.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            // Pega o texto do TextBox
            string message = MessageInput.Text;

            // Cria um TextBlock para exibir a mensagem
            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(5),
                
            };

            // Adiciona o TextBlock ao StackPanel (MessagesPanel)
            MessagesPanel.Children.Add(messageTextBlock);
            string reply = await SendMessageToChatGPT(message);
            AddMessageToLeft(reply);


            // Limpa o TextBox
            MessageInput.Text = "";

            // Rola para a última mensagem
            MessagesPanel.UpdateLayout();
            var scrollViewer = MessagesPanel.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }
    }
}
