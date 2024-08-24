using Firebase.Auth.Providers;
using Firebase.Auth;
using Firebase.Database;
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
using Firebase.Database.Query;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para AccountCreate.xaml
    /// </summary>
    public partial class AccountCreate : Window
    {

        private static readonly string ApiKey = Environment.GetEnvironmentVariable("JUPITER_KEY");

        private static readonly string ProjectId = Environment.GetEnvironmentVariable("JUPITER_ID");
        private static readonly string DatabaseUrl = Environment.GetEnvironmentVariable("JUPITER_URL");
        private const string loggedFile = "account.json";

        private FirebaseAuthClient authClient;
        private FirebaseClient databaseClient;
        private string language = "en-US";

        public AccountCreate(string language)
        {

            AmbienteCheck();
            InitializeComponent();
            InitializeFirebase();
            IsLogged();
            this.language = language;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if(language == "en-US")
            {
                LoginTitle.Content = "Login Account";
                RegisterAccountTitle.Content = "Register Account";
                CreateAccountButton.Content = "Create Account";
                CloseBtn.Content = "Close";
                loggedText.Content = "You are already logged in.";
                NameLabel.Content = "Name:";
            }
            if(language == "pt-BR")
            {
                LoginTitle.Content = "Login da Conta";
                RegisterAccountTitle.Content = "Registrar Conta";
                CreateAccountButton.Content = "Criar Conta";
                CloseBtn.Content = "Fechar";
                loggedText.Content = "Você já está logado.";
                NameLabel.Content = "Nome:";
            }
            if(language == "ES")
            {
                LoginTitle.Content = "Cuenta de inicio de sesión";
                RegisterAccountTitle.Content = "Registrar Cuenta";
                CreateAccountButton.Content = "Crear Cuenta";
                CloseBtn.Content = "Cerrar";
                loggedText.Content = "Ya has iniciado sesión.";
                NameLabel.Content = "Nombre:";
            }
        }

        private void AmbienteCheck()
        {
            if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(ProjectId) || string.IsNullOrEmpty(DatabaseUrl))
            {
                ExecutarSetup();
            }
        }

        private void ExecutarSetup()
        {
            string setupPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup.exe");

            if (File.Exists(setupPath))
            {
                try
                {
                    // Executa o Setup.exe
                    Process.Start(setupPath);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        private void IsLogged()
        {
            if (File.Exists(loggedFile))
            {
                string json = File.ReadAllText(loggedFile);
                var user = JsonConvert.DeserializeObject<User>(json);

                if (user != null && !string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Password))
                {
                    CheckCredentialsInDatabase(user);
                }
            }
            else
            {
                RegisterSection.Visibility = Visibility.Visible;
                LoginSection.Visibility = Visibility.Visible;
                loggedText.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<bool> CheckCredentialsInDatabase(User user)
        {
            bool exists = await CheckUserCredentials(user.Email, user.Password);

            if (exists)
            {
                RegisterSection.Visibility = Visibility.Collapsed;
                LoginSection.Visibility = Visibility.Collapsed;
                loggedText.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Visible;
            }
            else
            {
                RegisterSection.Visibility = Visibility.Visible;
                LoginSection.Visibility = Visibility.Visible;
                loggedText.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Collapsed;
            }

            return exists;
        }

        private async Task<bool> CheckUserCredentials(string email, string password)
        {
            var users = await databaseClient
                .Child("users")
                .OnceAsync<User>();

            

            return users.Any(u => u.Object.Email == email && u.Object.Password == password);
        }

        private async void InitializeFirebase()
        {
            try
            {
                // Configuração do Firebase Auth
                var config = new FirebaseAuthConfig
                {
                    ApiKey = ApiKey,
                    AuthDomain = $"{ProjectId}.firebaseapp.com",
                    Providers = new FirebaseAuthProvider[]
                    {
                        new EmailProvider()
                    }
                };

                authClient = new FirebaseAuthClient(config);

                // Configuração do Firebase Realtime Database
                databaseClient = new FirebaseClient(
                    DatabaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = GetFirebaseTokenAsync }
                );


            }
            catch (Exception ex)
            {

            }
        }

        private void Logout_Click(object sender,  RoutedEventArgs e)
        {
            if (File.Exists(loggedFile))
            {
                File.Delete(loggedFile);
                LoginSection.Visibility = Visibility.Visible;
                RegisterSection.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                loggedText.Visibility = Visibility.Collapsed;
                if(language == "en-US")
                    ToastWindow.Show("You have logged out.");
                if (language == "pt-BR")
                    ToastWindow.Show("Você efetuou logout.");
                if (language == "ES")
                    ToastWindow.Show("Has cerrado la sesión.");
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }

        private async Task<string> GetFirebaseTokenAsync()
        {
            // Este método deve retornar um token válido para autenticação
            // Você precisará implementar a lógica para obter e gerenciar tokens
            if (authClient.User != null)
            {
                return await authClient.User.GetIdTokenAsync();
            }
            return null;
        }

        private async Task<bool> EmailExists(string email)
        {
            var users = await databaseClient
                .Child("users")
                .OnceAsync<User>();

            return users.Any(u => u.Object.Email == email);
        }

        private async Task CreateAccount(string name, string email, string password)
        {
            if (await EmailExists(email))
            {
                if(language == "en-US")
                    MessageBox.Show("Email already exists. Try using a different email.");
                if (language == "pt-BR")
                    MessageBox.Show("O e-mail já existe. Tente usar um e-mail diferente.");
                if (language == "ES")
                    MessageBox.Show("El correo electrónico ya existe. Intente utilizar un correo electrónico diferente.");
                return;
            }
            Sha1 sha1 = new Sha1();
            string hashedPassword = sha1.HashPassword(password);

            try
            {
                var user = new User
                {
                    Name = name,
                    Email = email,
                    Password = hashedPassword,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await databaseClient
                    .Child("users")
                    .PostAsync(user);

                SaveUserToFile(user);
                LoginSection.Visibility = Visibility.Collapsed;
                RegisterSection.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;
                loggedText.Visibility = Visibility.Visible;

                if(language == "en-US")
                    ToastWindow.Show($"Account successfully created for: {email}");
                if (language == "pt-BR")
                    ToastWindow.Show($"Conta criada com sucesso para: {email}");
                if (language == "ES")
                    ToastWindow.Show($"Cuenta creada exitosamente para: {email}");
            }
            catch (Exception ex)
            {
                if(language == "en-US")
                    ToastWindow.Show($"Error creating account: {ex.Message}");
                if (language == "pt-BR")
                    ToastWindow.Show($"Erro ao criar conta: {ex.Message}");
                if (language == "ES")
                    ToastWindow.Show($"Error al crear la cuenta: {ex.Message}");
            }
        }

        private void SaveUserToFile(User user)
        {
            try
            {
                string json = JsonConvert.SerializeObject(user);
                File.WriteAllText("account.json", json);
                
            }
            catch (Exception ex)
            {
               
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string email = EmailTextBox.Text;
            string pass = PasswordBox.Password;
            if(name.Length >= 3)
            {
                if(email.Length >= 5)
                {
                    if(email.IndexOf("@") != -1)
                    {
                        if(pass.Length > 6)
                        {
                            CreateAccount(name, email, pass);
                        }
                        else
                        {
                            if(language == "en-us")
                                ToastWindow.Show("Password must contain more than 6 characters.");
                            if (language == "pt-BR")
                                ToastWindow.Show("A senha deve conter mais de 6 caracteres.");
                            if (language == "ES")
                                ToastWindow.Show("La contraseña debe contener más de 6 caracteres.");
                        }
                    }
                    else
                    {
                        if(language == "en-US")
                            ToastWindow.Show("Invalid E-mail.");
                        if (language == "pt-BR")
                            ToastWindow.Show("E-mail inválido.");
                        if (language == "ES")
                            ToastWindow.Show("Correo electrónico no válido.");

                    }
                }
                else
                {
                    if (language == "en-US")
                        ToastWindow.Show("Invalid E-mail.");
                    if (language == "pt-BR")
                        ToastWindow.Show("E-mail inválido.");
                    if (language == "ES")
                        ToastWindow.Show("Correo electrónico no válido.");
                }
            }
            else
            {
                if (language == "en-US")
                    ToastWindow.Show("Invalid Name.");
                if (language == "pt-BR")
                    ToastWindow.Show("Nome inválido.");
                if (language == "ES")
                    ToastWindow.Show("Nombre no válido.");
            }

            

            
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginEmailTextBox.Text;
            string pass = LoginPasswordBox.Password;
            if (email.Length > 5)
            {
                if (email.IndexOf("@") != -1)
                {
                    if (pass.Length > 6)
                    {
                        Sha1 sha1 = new Sha1();
                        pass = sha1.HashPassword(pass);
                        User u = new User
                        {
                            Email = email,
                            Password = pass
                        };

                        bool credentialsValid = await CheckUserCredentials(u.Email, u.Password);
                        if (credentialsValid)
                        {
                            RegisterSection.Visibility = Visibility.Collapsed;
                            LoginSection.Visibility = Visibility.Collapsed;
                            LogoutButton.Visibility = Visibility.Visible;
                            loggedText.Visibility = Visibility.Visible;
                            SaveUserToFile(u);
                            if(language == "en-US")
                                ToastWindow.Show("Login successful.\nRestart Jupiter Browser...");
                            if (language == "en-US")
                                ToastWindow.Show("Login bem-sucedido.\nReinicie o navegador Jupiter...");
                            if (language == "en-US")
                                ToastWindow.Show("Inicio de sesión exitoso.\nReinicie Jupiter Browser...");
                        }
                        else
                        {
                            if(language == "en-US")
                                ToastWindow.Show("Invalid email or password.");
                            if (language == "pt-BR")
                                ToastWindow.Show("E-mail ou senha inválidos.");
                            if (language == "ES")
                                ToastWindow.Show("Correo electrónico o contraseña no válidos.");
                        }
                    }
                    else
                    {
                        if(language == "en-US")
                            ToastWindow.Show("Invalid Password.");
                        if (language == "pt-BR")
                            ToastWindow.Show("Senha inválida.");
                        if (language == "ES")
                            ToastWindow.Show("Contraseña inválida.");
                    }
                }
                else
                {
                    if (language == "en-US")
                        ToastWindow.Show("Invalid E-mail.");
                    if (language == "pt-BR")
                        ToastWindow.Show("E-mail inválido.");
                    if (language == "ES")
                        ToastWindow.Show("Correo electrónico no válido.");
                }
            }
            else
            {
                if (language == "en-US")
                    ToastWindow.Show("Invalid E-mail.");
                if (language == "pt-BR")
                    ToastWindow.Show("E-mail inválido.");
                if (language == "ES")
                    ToastWindow.Show("Correo electrónico no válido.");
            }
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
