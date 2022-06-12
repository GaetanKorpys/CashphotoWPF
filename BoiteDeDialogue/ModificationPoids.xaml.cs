using CashphotoWPF.BDD;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CashphotoWPF.BoiteDeDialogue
{
    /// <summary>
    /// Logique d'interaction pour Poids.xaml
    /// </summary>
    public partial class ModificationPoids : Window
    {
        private MainWindow _app; //un accès à la fenêtre principale
        private Commande _commande;
        public ModificationPoids(MainWindow app, Commande commande)
        {
            InitializeComponent();
            _app = app;
            _commande = commande;


            NbColis.Content = "La commande possède " + commande.NbColis + ".";
        }

        private bool ChoixColisOK(string choixColis)
        {
            if(int.Parse(choixColis) <= _commande.NbColis && int.Parse(choixColis) >= 1)
                return true;
            return false;
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if (_app.isValidPoids(PoidsColis.Text) && ChoixColisOK(ChoixColis.Text) )
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurPoids.Content = "Veuillez vérifier le poids et le nombre de colis.";
            }
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
