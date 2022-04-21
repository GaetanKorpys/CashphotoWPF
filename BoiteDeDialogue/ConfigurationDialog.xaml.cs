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

namespace CashphotoWPF.BoiteDeDialogue
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class ConfigurationDialog : Window
    {
        public ConfigurationDialog()
        {
            InitializeComponent();
            InputTextBox.Focus();
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if (IsPasswordValid(InputTextBox.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurMDP.Content = "Mot de passe incorrect.";
            }
           
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "";
            this.DialogResult = false;
            this.Close();

        }

        private bool IsPasswordValid(string password)
        {
            if(password == "Cashphoto") return true;
            return false;
        }
    }
}
