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
    /// Lógica interna para ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string msg)
        {
            InitializeComponent();
            Msg.Text = msg;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Set DialogResult to true and close the dialog
            this.DialogResult = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // Set DialogResult to false or null and close the dialog
            this.DialogResult = false;
            this.Close();
        }
    }
}
