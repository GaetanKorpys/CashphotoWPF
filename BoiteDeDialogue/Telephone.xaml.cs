using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Logique d'interaction pour Telephone.xaml
    /// </summary>
    public partial class Telephone : Window
    {
        private const string motif = @"^([\+]?33[-]?|[0])?[1-9][0-9]{8}$";
        public Telephone()
        {
            InitializeComponent();
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if(isValidPhoneNumber(InputTextBox.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurNumero.Content = "Veuillez entrer un numéro de téléphone valide.";
            }
           
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "";
            this.DialogResult = false;
            this.Close();
        }

        private bool isValidPhoneNumber(string numero)
        {
            if (numero != null) return Regex.IsMatch(numero, motif);
            else return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus(); //permet d'écrire directement dans le champ de texte sans le sélectionner manuellement
        }
    }
}
