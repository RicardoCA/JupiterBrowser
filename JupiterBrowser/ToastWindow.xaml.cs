using System.Windows;
using System.Windows.Threading;

namespace JupiterBrowser
{
    public partial class ToastWindow : Window
    {
        private DispatcherTimer _timer;

        public ToastWindow(string message, int duration = 3000)
        {
            InitializeComponent();
            MessageText.Text = message;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(duration)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Close();
        }

        public static void Show(string message, int duration = 3000)
        {
            var toast = new ToastWindow(message, duration);
            toast.Show();
        }
    }
}