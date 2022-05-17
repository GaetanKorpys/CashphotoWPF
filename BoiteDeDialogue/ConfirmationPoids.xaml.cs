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
    /// Logique d'interaction pour ConfirmationPoids.xaml
    /// </summary>
    public partial class ConfirmationPoids : Window
    {
        private MainWindow _app; //un accès à la fenêtre principale

        public ConfirmationPoids(MainWindow app)
        {
            InitializeComponent();
            InputTextBox.Focus();
            _app = app;
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if (_app.isValidPoids(InputTextBox.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurPoids.Content = "Veuillez entrer un poids valide.";
            }
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "";
            this.DialogResult = false;
            this.Close();
        }
    }
}
