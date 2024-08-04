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

namespace JupiterBrowser
{
    /// <summary>
    /// Lógica interna para AccountCreate.xaml
    /// </summary>
    public partial class AccountCreate : Window
    {

        private const string ApiKey = "AIzaSyDiVnDzUepc8yHBYxxUMgY163D-TnA40e0";
        private const string ProjectId = "jupiterbrowser-8f6b2";
        private const string DatabaseUrl = "https://jupiterbrowser-8f6b2-default-rtdb.firebaseio.com";
        private const string loggedFile = "account.json";

        private FirebaseAuthClient authClient;
        private FirebaseClient databaseClient;

        public AccountCreate()
        {
            InitializeComponent();
            InitializeFirebase();
            IsLogged();
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
                ToastWindow.Show("You have logged out.");
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
                MessageBox.Show("Email already exists. Try using a different email.");
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

                ToastWindow.Show($"Account successfully created for: {email}");
            }
            catch (Exception ex)
            {
                ToastWindow.Show($"Error creating account: {ex.Message}");
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
                            ToastWindow.Show("Password must contain more than 6 characters.");
                        }
                    }
                    else
                    {
                        ToastWindow.Show("Invalid E-mail.");

                    }
                }
                else
                {
                    ToastWindow.Show("Invalid E-mail.");
                }
            }
            else
            {
                ToastWindow.Show("Invalid Name.");
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
                            ToastWindow.Show("Login successful.");
                        }
                        else
                        {
                            ToastWindow.Show("Invalid email or password.");
                        }
                    }
                    else
                    {
                        ToastWindow.Show("Invalid Password.");
                    }
                }
                else
                {
                    ToastWindow.Show("Invalid E-mail.");
                }
            }
            else
            {
                ToastWindow.Show("Invalid E-mail.");
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
