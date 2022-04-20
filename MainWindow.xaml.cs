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

        /// <summary>
        /// Pour afficher les chemins et autres paramètres dans le menu Configuartion
        /// </summary>
        public void chargerConfiguration()
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
            CheminFichierConfig.Content = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
        }

        /// <summary>
        /// Affiche une boîte de dialogue générique pour choisir un dossier
        /// </summary>
        private string ChoisirDossierDialog()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\\\";
            dialog.IsFolderPicker = true;
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
        
        public void ModifierConfiguration(object sender, EventArgs e)
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

        public void ModifierConfigurationParametre(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            string dossier = ChoisirDossierDialog();

            if (sender.Equals(ModifierEmail))
            {
                constante.email = dossier;
                email.Content = dossier;
            }
            else if (sender.Equals(ModifierTel))
            {
                constante.telephone = dossier;
                tel.Content = dossier;
            }
        }
    }
}
