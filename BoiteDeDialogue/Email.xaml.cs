using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CashphotoWPF.BoiteDeDialogue
{
    /// <summary>
    /// Logique d'interaction pour Email.xaml
    /// </summary>
    public partial class Email : Window
    {
        public Email()
        {
            InitializeComponent();
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if(IsValidEmail(InputTextBox.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurEmail.Content = "Veuillez entrer une adresse email valide.";
            }
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "";
            this.DialogResult = false;
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var mailAddress))
                return false;

            // And if you want to be more strict:
            var hostParts = mailAddress.Host.Split('.');
            if (hostParts.Length == 1)
                return false; // No dot.
            if (hostParts.Any(p => p == string.Empty))
                return false; // Double dot.
            if (hostParts[^1].Length < 2)
                return false; // TLD only one letter.

            if (mailAddress.User.Contains(' '))
                return false;
            if (mailAddress.User.Split('.').Any(p => p == string.Empty))
                return false; // Double dot or dot at end of user part.

            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus(); //permet d'écrire directement dans le champ de texte sans le sélectionner manuellement
        }
    }
}
