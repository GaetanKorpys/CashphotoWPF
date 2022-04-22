using CashphotoWPF.Configuration;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Forms;
using CashphotoWPF.BoiteDeDialogue;
using System.Text.RegularExpressions;

namespace CashphotoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //On charge le fichier de config pour initiliser les constantes dans la classe Constante
            FichierConfig config  = FichierConfig.GetInstance();
            config.charger();
            chargerConfiguration();
        }

        private bool IsValidIPAddress(string IpAddress)
        {
            Regex validIpV4AddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.IgnoreCase);
          
            return validIpV4AddressRegex.IsMatch(IpAddress.Trim());
            
        }

        private void setBDDIP(string IP)
        {
            Constante constante = Constante.GetConstante();

            if(IsValidIPAddress(IP))
            {
                constante.BDDIP = IP;
                ReponseBDD.Content = "Adresse IP validée.";
            }
            else
            {
                ReponseBDD.Content = "Veuillez saisir une adresse IP valide.";
            }
            
        }

        /// <summary>
        /// Pour afficher les chemins et autres paramètres dans le menu Configuartion
        /// </summary>
        private void chargerConfiguration()
        {
            //On récupère l'instance unique de la classe Constante
            Constante constante = Constante.GetConstante();

            //Les imports
            DossierCommandeAmazon.Content = constante.commandeAmazon;
            DossierCommandeCashphoto.Content = constante.commandeCashphoto;
            DossierNumeroSuiviColiposte.Content = constante.numeroSuiviColiposte;

            //Les exports
            DossierNumeroSuiviAmazon.Content = constante.numeroSuiviAmazon;
            DossierNumeroSuiviCashphoto.Content = constante.numeroSuiviCashphoto;
            DossierCommandeGLS.Content = constante.commandeParsePourGLS;
            DossierCommandeColiposte.Content = constante.commandeParsePourColiposte;

            //Les backup
            DossierBackupCommandeAmazon.Content = constante.backupCommandeAmazon;
            DossierBackupCommandeCashphoto.Content = constante.backupCommandeCashphoto;

            DossierBackupNumeroSuiviAmazon.Content = constante.backupNumeroSuiviAmazon;
            DossierBackupNumeroSuiviCashphoto.Content = constante.backupNumeroSuiviCashphoto;

            DossierBackupCommandePourGLS.Content = constante.backupCommandeTransporteurGLS;
            DossierBackupCommandePourColiposte.Content = constante.backupCommandeTransporteurColiposte;

            //Les paramètres globaux
            email.Content = constante.email;
            tel.Content = constante.telephone;

            if (constante.fichierConfigExist)
            {
                CheminFichierConfig.Content = constante.cheminFichierConfig;
            }
            else
            {
                CheminFichierConfig.Content = "Aucun fichier de configuration chargé.";
            }
        }

        /// <summary>
        /// Affiche une boîte de dialogue générique pour choisir un dossier
        /// </summary>
        private string ChoisirDossierDialog()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\\\";
            dialog.IsFolderPicker = true; //Pour bien chsoisir un dossier et non pas un fichier
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Affiche une boîte de dialogue générique pour choisir un fichier
        /// </summary>
        private string ChoisirFichierDialog()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\\\";
            dialog.Filters.Add(new CommonFileDialogFilter("Txt File", "*.txt"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return "";
            }
        }

        // -------------- Bouton --------------
        
        private void ModifierConfiguration(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            string dossier = ChoisirDossierDialog();
            if (!string.IsNullOrEmpty(dossier))
            {
                //Switch ? ou COR à la place
                if (sender.Equals(ModifierDossierNumeroSuiviAmazonBouton))
                {
                    constante.numeroSuiviAmazon = dossier;
                    DossierNumeroSuiviAmazon.Content = dossier;
                }
                else if(sender.Equals(ModifierDossierNumeroSuiviCashphotoBouton))
                {
                    constante.numeroSuiviCashphoto = dossier;
                    DossierNumeroSuiviCashphoto.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierCommandeGLSBouton))
                {
                    constante.commandeParsePourGLS = dossier;
                    DossierCommandeGLS.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierCommandeColiposteBouton))
                {
                    constante.commandeParsePourColiposte = dossier;
                    DossierCommandeColiposte.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupCommandeCashphotoBouton))
                {
                    constante.backupCommandeCashphoto = dossier;
                    DossierBackupCommandeCashphoto.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupCommandeAmazonBouton))
                {
                    constante.backupCommandeAmazon = dossier;
                    DossierBackupCommandeAmazon.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupNumeroSuiviCashphotoBouton))
                {
                    constante.backupNumeroSuiviCashphoto = dossier;
                    DossierBackupNumeroSuiviCashphoto.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupNumeroSuiviAmazonBouton))
                {
                    constante.backupNumeroSuiviAmazon = dossier;
                    DossierBackupNumeroSuiviAmazon.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupCommandePourGLSBouton))
                {
                    constante.backupCommandeTransporteurGLS = dossier;
                    DossierBackupCommandePourGLS.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierBackupCommandePourColiposteBouton))
                {
                    constante.backupCommandeTransporteurColiposte = dossier;
                    DossierBackupCommandePourColiposte.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierCommandeAmazonBouton))
                {
                    constante.commandeAmazon = dossier;
                    DossierCommandeAmazon.Content = dossier;
                }
                else if (sender.Equals(ModifierDossierCommandeCashphotoBouton))
                {
                    constante.commandeCashphoto = dossier;
                    DossierCommandeCashphoto.Content = dossier;
                }
                else if (sender.Equals(ModifierNumeroSuiviColiposteBouton))
                {
                    constante.numeroSuiviColiposte = dossier;
                    DossierNumeroSuiviColiposte.Content = dossier;
                }
            }
        }

        private void ModifierConfigurationParametre(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
           
            if (sender.Equals(ModifierEmail))
            {
                Email emailClass = new Email();
                if(emailClass.ShowDialog() == true)
                {
                    constante.email = emailClass.InputTextBox.Text;
                    email.Content = emailClass.InputTextBox.Text;
                }  
            }
            else if (sender.Equals(ModifierTel))
            {
                Telephone telephoneClass = new Telephone();
                if (telephoneClass.ShowDialog() == true)
                {
                    constante.telephone = telephoneClass.InputTextBox.Text;
                    tel.Content = telephoneClass.InputTextBox.Text;
                }
            }
        }

        private void ModifierFichierConfig(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            FichierConfig fichierConfig = FichierConfig.GetInstance();

            if (sender.Equals(ChargerFichierConfig))
            {
                string fichier = ChoisirFichierDialog();
                if (!string.IsNullOrEmpty(fichier))
                {
                    fichierConfig.charger(fichier);
                     chargerConfiguration();
                }
            }
            else if (sender.Equals(SaveFichierConfig))
            {
                fichierConfig.sauvegarder();
                CheminFichierConfig.Content = constante.cheminFichierConfig;
            }

        }

        private void setBDDRadioBouton(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            if(sender.Equals(BDDLocaleRadio))
            {
                BDDAdresseTextBox.IsEnabled = false;
                ModifierBDDIPButton.IsEnabled = false;
                setBDDIP("127.0.0.1");
            }
            else if(sender.Equals(BDDDistanteRadio))
            {
                ModifierBDDIPButton.IsEnabled = true;
                BDDAdresseTextBox.IsEnabled = true;
            }
        }

        private void ValiderIP(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) setBDDIP(BDDAdresseTextBox.Text);
        }

        private void ValiderIP(object sender, EventArgs e)
        {
            setBDDIP(BDDAdresseTextBox.Text);
        }

        private void GestionTabItem(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == TabControl)
            {
                Constante constante = Constante.GetConstante();
                if (Preparation.IsSelected)
                {
                    constante.indexTabItem = 0;
                    System.Diagnostics.Debug.WriteLine("Menu " + constante.indexTabItem);
                }
                else if (Expedition.IsSelected)
                {
                    constante.indexTabItem = 1;
                    System.Diagnostics.Debug.WriteLine("Menu " + constante.indexTabItem);
                }
                else if (Configuration.IsSelected)
                {
                    ConfigurationDialog configurationDialog = new ConfigurationDialog();
                    if (configurationDialog.ShowDialog() == true)
                    {
                        constante.indexTabItem = 2;
                    }
                    
                    else
                    {
                        //Important sinon, la boite de dialog prend le focus et perturbe le TabControl
                        //Cela entraine un bug qui déclenche 2 fois la boite de dialogue
                        TabControl.Focus(); 
                        TabControl.SelectedIndex = constante.indexTabItem;
                    }
                }
            }
        }
    }
}
