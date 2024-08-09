using System.Configuration;
using System.Data;
using System.Windows;

namespace JupiterBrowser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();

            if (e.Args.Length > 0)
            {
                string url = e.Args[0];
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    mainWindow.OpenNewTabWithUrl(url);
                }
            }

            mainWindow.Show();
        }
    }

}
