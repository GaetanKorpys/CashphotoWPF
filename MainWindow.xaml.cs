﻿using CashphotoWPF.Configuration;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Globalization;
using System.Diagnostics;
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
using CashphotoWPF.BDD;
using Cursors = System.Windows.Input.Cursors;
using System.Collections.ObjectModel;
using TextBox = System.Windows.Controls.TextBox;

namespace CashphotoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Commande> _commandes { get; set; }
        public Commande _commande { get; set; }


        #region Window

        /// <summary>
        /// Construction de la fenêtre.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opérations à effectuer après avoir chargé la fenêtre principale.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            chargementLancement();
        }

        /// <summary>
        /// Exécute plusieurs fonctions au lancement de l'application :
        ///     - Maj des constantes
        ///     - Création de la BDD
        ///     - Test de connexion à la BDD
        ///     - Affichage dans la fenêtre ...
        /// </summary>
        private void chargementLancement()
        {
            //Maj des constantes qui ne sont pas chargées depuis le fichier de config.
            Constante constante = Constante.GetConstante();
            constante.BDDOK = false; //On suppose que la BDD n'est pas connectée, on modifie cette valeur lors du test de connexion.

            //On charge le fichier de config pour initiliser le reste des constantes dans la classe Constante
            FichierConfig config = FichierConfig.GetInstance();
            config.charger();

            //Affichage des données dans le menu Configuartion de la fenêtre principale
            chargerConfiguration();

            //Choix du menu à afficher lors de l'ouvrture de la fenêtre, paramétré via le fichier de config.
            //Poste 1 = Préparation, Poste 2 = Exépdition
            TabControl.SelectedIndex = constante.indexTabItem;


            //Le Poste de Préparation utilise une balance connectée pour renseigner le poids directement de la balance au proagramme.
            //Le software de la balance doit être obligatoirment lancée.
            if (constante.indexTabItem == 0)
                ExecuteAsAdmin(GetCheminAppliBalance());

            CreationBDD();

            TestConnexionBDD_Click(null, null);

            //Chargement du DataGrid
            //On affiche les commandes traitées ce jour.
            _commandes = getCommandesDateToday();
            DatagGridPrep.ItemsSource = _commandes;

        }

        /// <summary>
        /// Opérations à effectuer après avoir fermé la fenêtre.
        /// On cherche uniqument à fermer le software de la balance connectée.
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (ProgramIsRunning(GetCheminAppliBalance()))
            {
                killProcessBalance();
            }
                
            Close();
           
        }

        #endregion

        #region ToolBox
        /// <summary>
        /// Exéxute l'application situé à l'emplacement fileName avec les droits administarteur.
        /// <paramref name="fileName"/>
        /// </summary>
        private void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        /// <summary>
        /// Renvoie le chemin de l'application .exe de la balance connectée.
        /// </summary>
        private string GetCheminAppliBalance()
        {
            string chemin = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
            chemin += "\\";
            chemin += "activation_V3.01.exe";
            return chemin;
        }
        #endregion

        #region BDD
        /// <summary>
        /// Affichage le résultat du test de la connexion à la BDD.
        /// Utilise la fonction TestConnexionBDD().
        /// </summary>
        private void TestConnexionBDD_Click(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            constante.BDDOK = TestConnexionBDD();
            AfficherTestBDD();
        }

        /// <summary>
        /// Affiche le résultat du test de la connexion à la BDD.
        /// </summary>
        private void AfficherTestBDD()
        {
            Constante constante = Constante.GetConstante();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            if (constante.BDDOK)
            {
                mySolidColorBrush.Color = Color.FromRgb(0, 255, 0);
                LabelTestBDDPrep.Content = "Connexion OK";
                LabelTestBDDExpe.Content = "Connexion OK";
                DisplayTempMessage(ReponseBDD, "Connexion OK");
            }
            else
            {
                mySolidColorBrush.Color = Color.FromRgb(255, 0, 0);
                LabelTestBDDPrep.Content = "Erreur connexion";
                LabelTestBDDExpe.Content = "Erreur connexion";
                ReponseBDD.Content = "Erreur connexion";
            }
            LedTestBDDPrep.Fill = mySolidColorBrush;
            LedTestBDDExpe.Fill = mySolidColorBrush;
        }

        /// <summary>
        /// Création de la BDD.
        /// </summary>
        private void CreationBDD()
        {
            Constante constante = Constante.GetConstante();
            constante.cashphotoBDD = new CashphotoBDD();
        }

        /// <summary>
        /// Test de connexion à la BDD.
        /// </summary>
        private bool TestConnexionBDD()
        {
            Constante constante = Constante.GetConstante();
            if (constante.cashphotoBDD.Database.CanConnect())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Test si l'IP IpAddress est correct.
        /// <paramref name="IpAddress"/>
        /// </summary>
        private bool IsValidIPAddress(string IpAddress)
        {
            Regex validIpV4AddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.IgnoreCase);

            return validIpV4AddressRegex.IsMatch(IpAddress.Trim());

        }

        /// <summary>
        /// Affiche si l'IP est validé ou non.
        /// Si oui, met à jour l'IP (maj de la variable dans la classe Constante).
        /// </summary>
        private void setBDDIP(string IP)
        {
            Constante constante = Constante.GetConstante();

            if (IsValidIPAddress(IP))
            {
                constante.BDDIP = IP;
                DisplayTempMessage(ReponseBDD, "Adresse IP validée.");
            }
            else
            {
                ReponseBDD.Content = "Veuillez saisir une adresse IP valide.";
            }

        }
        #endregion

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commande commande = (Commande)DatagGridPrep.SelectedItem;
            if(commande != null)
            {
                _commande = commande;
                ActualiserRecapEnregistrementCommande(commande.NumCommande);
            }
            
        }

        private void ModifierRecap(object sender, MouseButtonEventArgs e)
        {
            if(sender.Equals(NumCommandeRecap))
            {
                NumCommandeRecap.Focusable = true;
                NumCommandeRecap.Focus();
                
            }
            else if(sender.Equals(PoidsRecap))
            {
                PoidsRecap.Focusable = true;
                PoidsRecap.Focus();
            }
        }

        private void Recap_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(NumCommandeRecap))
            {
                NumCommandeRecap.Focusable = false;
            }
            else if (sender.Equals(PoidsRecap))
            {
                PoidsRecap.Focusable = false;
            }
        }

        private void Recap_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender.Equals(NumCommandeRecap))
            {
                NumCommandeRecap.Cursor = NumCommandeRecap.Focusable ? Cursors.IBeam : Cursors.Arrow;
            }
            else if (sender.Equals(PoidsRecap))
            {
                PoidsRecap.Cursor = PoidsRecap.Focusable ? Cursors.IBeam : Cursors.Arrow;
            }
        }

        private void ActualiserRecapEnregistrementCommande(string? numCommande)
        {
            Constante constante = Constante.GetConstante();
            string poids;

            //Pas de paramètres, alors on affiche le récap de la dernière commande enregistrée.
            if (numCommande == null)
            {
                numCommande = constante.cashphotoBDD.Commandes.OrderByDescending(p => p.IdCommande).FirstOrDefault().NumCommande;
                poids = constante.cashphotoBDD.Commandes.OrderByDescending(p => p.IdCommande).FirstOrDefault().Poids.ToString();
            }
            else
            {
                numCommande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).FirstOrDefault().NumCommande;
                poids = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).FirstOrDefault().Poids.ToString();
            }

            poids = poids.Replace(",", ".");
            NumCommandeRecap.Text = numCommande;
            PoidsRecap.Text = poids;
        }

        private void AfficherTestEnregistrementCommande(bool status)
        {
            if (status)
            {
                DisplayTempMessage(PrepCommandeLabel, "Enregistrement OK.");
                DisplayTempEllipse(LedEnregistrementCommande, 0, 255, 0);
            }
            else
            {
                DisplayTempMessage(PrepCommandeLabel, "La commande existe déjà.");
                DisplayTempEllipse(LedEnregistrementCommande, 255, 0, 0);
            }
        }

        private void AfficherTestRecap(bool status)
        {
            if(status)
            {
                DisplayTempMessage(RecapLabel, "Modification OK.");
                DisplayTempEllipse(LedEnregistrementRecap, 0, 255, 0);
            }
            else
            {
                DisplayTempMessage(RecapLabel, "La commande existe déjà.");
                DisplayTempEllipse(LedEnregistrementRecap, 255, 0, 0);
            }

        }

        private bool commandeExist(string numCommande)
        {
            Constante constante = Constante.GetConstante();

            if (constante.cashphotoBDD.Commandes.Where(e => e.NumCommande == numCommande).Count() == 0)
                return false;
            else
                return true;
        }
           

        private Commande validerCommande(string numCommande)
        {
            Constante constante = Constante.GetConstante();

            Commande commande = new Commande();
            commande.NumCommande = SaisirCommande.Text;
            System.Diagnostics.Debug.WriteLine("Poids :" + SaisirPoids.Text);

            double poids = double.Parse(SaisirPoids.Text, CultureInfo.InvariantCulture);
            commande.Poids = poids;
            commande.Date = DateTime.Now;
            commande.Prepare = true;
            commande.Expedie = false;

            constante.cashphotoBDD.Add(commande);
            constante.cashphotoBDD.SaveChanges();

            return commande;
        }

        private void ActivationBoutonValider(object sender, EventArgs e)
        {
            if(sender.Equals(SaisirPoids))
            {
                if (isValidPoids(SaisirPoids.Text))
                    ValiderCommandeBouton.IsEnabled = true;
                else
                    ValiderCommandeBouton.IsEnabled = false;
            }
        }

        private void ActiverBoutonRecap(object sender, TextChangedEventArgs e)
        {
           if(isValidNumCommande(NumCommandeRecap.Text) && isValidPoids(PoidsRecap.Text))
                ValiderRecapBouton.IsEnabled = true;
           else
                ValiderRecapBouton.IsEnabled = false;
        }

        private void EnregistrerCommande_Click(object sender, RoutedEventArgs e)
        {
            if(!commandeExist(SaisirCommande.Text))
            {
                _commande = validerCommande(SaisirCommande.Text);
                SaisirPoids.Text = "";
                SaisirCommande.Text = "";
                SaisirCommande.IsEnabled = true;
                SaisirCommande.Focus();
                SaisirPoids.IsEnabled = false;
                AfficherTestEnregistrementCommande(true);
                ActualiserRecapEnregistrementCommande(null);

                //Rechargement du Datagrid
                _commandes = getCommandesDateToday();
                DatagGridPrep.ItemsSource = _commandes;
            }
            else
            {
                SaisirPoids.Text = "";
                SaisirCommande.Text = "";
                SaisirCommande.IsEnabled = true;
                SaisirCommande.Focus();
                SaisirPoids.IsEnabled = false;
                AfficherTestEnregistrementCommande(false);
            }
        }

        private bool isValidNumCommande(string numCommande)
        {
            Constante constante = Constante.GetConstante();
            Regex numCommandeAmazon = new Regex(constante.regexCommandeAmazon);
            Regex numCommandeCashphoto = new Regex(constante.regexCommandeCashphoto);
            if (numCommandeAmazon.IsMatch(numCommande) || numCommandeCashphoto.IsMatch(numCommande))
            {
                System.Diagnostics.Debug.WriteLine("OK");
                return true;
            }
                
            return false;
            
        }

        private bool isValidPoids(string poids)
        {
            string pattern = "^\\d{1,2}(\\.\\d{1,3})?$"; //Ex: 23.344 ou 34 
            Regex poidsRegex = new Regex(pattern);
            if (poidsRegex.IsMatch(poids))
                return true;
            return false;
        }


        private void SaisiNumCommande_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (isValidNumCommande(SaisirCommande.Text))
                {
                    SaisirPoids.IsEnabled = true;
                    SaisirPoids.Focus();
                }
                else
                    DisplayTempMessage(PrepCommandeLabel, "Veuillez renseigner un numéro de commande valide.");
            }
                
        }

        private void EnregistrerCommande_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (isValidPoids(SaisirPoids.Text))
                {
                    if(!commandeExist(SaisirCommande.Text))
                    {
                        _commande = validerCommande(SaisirCommande.Text);
                        SaisirPoids.Text = "";
                        SaisirCommande.Text = "";
                        SaisirCommande.IsEnabled = true;
                        SaisirCommande.Focus();
                        SaisirPoids.IsEnabled = false;
                        AfficherTestEnregistrementCommande(true);
                        ActualiserRecapEnregistrementCommande(null);

                        //Rechargement du Datagrid
                        _commandes = getCommandesDateToday();
                        DatagGridPrep.ItemsSource = _commandes;
                    }
                    else
                    {
                        SaisirPoids.Text = "";
                        SaisirCommande.Text = "";
                        SaisirCommande.IsEnabled = true;
                        SaisirCommande.Focus();
                        SaisirPoids.IsEnabled = false;
                        AfficherTestEnregistrementCommande(false);
                    }
                }
                else
                    DisplayTempMessage(PrepCommandeLabel, "Veuillez renseigner un poids valide.");                
            }
            if(isValidPoids(SaisirPoids.Text))
                ValiderCommandeBouton.IsEnabled = true;
        }


        private void killProcessBalance()
        {
            string targetProcessPath = GetCheminAppliBalance();
            string targetProcessName = "activation_V3.01";

            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName == targetProcessName &&
                    process.MainModule != null &&
                    string.Compare(process.MainModule.FileName, targetProcessPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    process.Kill();
                }
            }
        }

        private bool ProgramIsRunning(string FullPath)
        {
            string FilePath = System.IO.Path.GetDirectoryName(FullPath);
            string FileName = System.IO.Path.GetFileNameWithoutExtension(FullPath).ToLower();
            bool isRunning = false;

            Process[] pList = Process.GetProcessesByName(FileName);

            foreach (Process p in pList)
            {
                if (p.MainModule.FileName.StartsWith(FilePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    isRunning = true;
                    break;
                }
            }

            return isRunning;
        }


        private void ModificationActived(object sender, EventArgs e)
        {
            DatagGridPrep.IsReadOnly = false;
        }

        private void ModificationDesactived(object sender, EventArgs e)
        {
            DatagGridPrep.IsReadOnly = true;
        }

        private void rechercheCommande_TextChanged(object sender, EventArgs e) 
        {
            if(RechercherCommande1.Text.Equals(""))
                _commandes = getCommandesDateToday();
            else
                _commandes = getCommandesRecherche(RechercherCommande1.Text);
            DatagGridPrep.ItemsSource = _commandes;
        }

        private List<Commande> getCommandesRecherche(string numCmd)
        {
            List<Commande> commandes = new List<Commande>();
            Constante constante = Constante.GetConstante();
            IQueryable<Commande> commandesTable;

            if (constante.BDDOK)
            {
                commandesTable = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande.Contains(numCmd));
                if (constante.commandeExpedie)
                {
                    commandesTable = commandesTable.Where(commande => commande.Expedie == true);
                }
                else
                {
                    commandesTable = commandesTable.Where(commande => commande.Expedie == false);
                }
                commandesTable = commandesTable.OrderByDescending(commande => commande.Date);
                commandes = commandesTable.ToList();
            }
            return commandes;
        }

        private List<Commande> getCommandesDateToday()
        {
            List<Commande> commandes = new List<Commande>();
            Constante constante = Constante.GetConstante();
            IQueryable<Commande> commandesTable;

            if (constante.BDDOK)
            {
                commandesTable = constante.cashphotoBDD.Commandes.Where(commande => commande.Date.Date == DateTime.Today);
                commandesTable = commandesTable.OrderByDescending(commande => commande.Date);
                commandes = commandesTable.ToList();
            }
            return commandes;
        }

        private void DisplayTempMessage(System.Windows.Controls.Label label, string message)
        {
            label.Content = message; //affichage du message
            Timer aTimer = new Timer();
            aTimer.Interval = 5000; //délai de 5 seconds
            aTimer.Tick += (Object o, EventArgs e2) => { aTimer.Stop(); label.Content = ""; }; //effacement du message
            aTimer.Enabled = true; //démarrage du timer
        }

        private void DisplayTempEllipse(Ellipse ellipse, byte r, byte g, byte b)
        {
            ellipse.Visibility = Visibility.Visible;
            Timer aTimer = new Timer();
            aTimer.Interval = 5000; //délai de 5 seconds
            aTimer.Tick += (Object o, EventArgs e2) => { aTimer.Stop(); ellipse.Visibility = Visibility.Hidden; }; //effacement de l'ellipse
            aTimer.Enabled = true; //démarrage du timer
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromRgb(r, g, b);
            ellipse.Fill = mySolidColorBrush;
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

        private void ValiderRecap(object sender, RoutedEventArgs e)
        {

            Constante constante = Constante.GetConstante();
            
            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.IdCommande == _commande.IdCommande).FirstOrDefault();

            if(!commandeExist(NumCommandeRecap.Text))
            {
                commande.NumCommande = NumCommandeRecap.Text;
                double poids = double.Parse(PoidsRecap.Text, CultureInfo.InvariantCulture);
                commande.Poids = poids;

                constante.cashphotoBDD.SaveChanges();
                AfficherTestRecap(true);

                //Rechargement du Datagrid
                _commandes = getCommandesDateToday();
                DatagGridPrep.ItemsSource = _commandes;
            }
            else
                AfficherTestRecap(false);

        }
         
        private void effacerTextbox(object sender, EventArgs e)
        {
            if (sender.Equals(RechercherCommande))
                RechercherCommande.Text = "";
        }

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

                //BDD locale, l'utilisateur ne rentre donc pas d'adresse IP
                //On valide directement dès que le bouton radio locale est choisi.
                setBDDIP("127.0.0.1");
                //Nouvelle BDD locale
                CreationBDD();
            }
            else if(sender.Equals(BDDDistanteRadio))
            {
                ModifierBDDIPButton.IsEnabled = true;
                BDDAdresseTextBox.IsEnabled = true;
            }
        }

        private void ValiderIP(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                setBDDIP(BDDAdresseTextBox.Text);
                CreationBDD();
            }
        }

        private void ValiderIP(object sender, EventArgs e)
        {
            setBDDIP(BDDAdresseTextBox.Text);
            CreationBDD();
        }

        private void GestionTabItem(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == TabControl)
            {
                Constante constante = Constante.GetConstante();
                if (Preparation.IsSelected)
                {
                    constante.indexTabItem = 0;
                    SaisirCommande.Focus();
                }
                else if (Expedition.IsSelected)
                {
                    constante.indexTabItem = 1;
                    RechercherCommande.Focus();
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

        

        /*
private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
{
   Constante constante = Constante.GetConstante();

   //La commande modifiée que l'on récupère avec la ligne du DataGrid séléctionnée
   Commande cmdDataGrid = e.Row.DataContext as Commande;

   System.Diagnostics.Debug.WriteLine("cell "+((TextBox)e.EditingElement).Text);

   if (e.Column.Header.Equals("Numéro de commande"))
   {
       System.Diagnostics.Debug.WriteLine("YA");
   }
   int idCommandeDataGrid = cmdDataGrid.IdCommande;
   string numCommandeDataGrid = cmdDataGrid.NumCommande;
   double poidsCommandeDataGrid = cmdDataGrid.Poids;

   //On récupère la commande de la BDD avec l'ID (pas le numéro de commande).
   Commande commandeBDD = constante.cashphotoBDD.Commandes.Where(commande => commande.IdCommande == idCommandeDataGrid).FirstOrDefault();

   //Le numéro de commande n'est pas modifié.
   //L'utilisateur a unqiuement modifié le poids.
   System.Diagnostics.Debug.WriteLine("1 / " + poidsCommandeDataGrid.ToString());
   System.Diagnostics.Debug.WriteLine("2 / " + numCommandeDataGrid);
   if (commandeBDD.NumCommande.Equals(numCommandeDataGrid) && isValidPoids(poidsCommandeDataGrid.ToString()))
   {
       System.Diagnostics.Debug.WriteLine("A ");
       System.Diagnostics.Debug.WriteLine("A DT"+ numCommandeDataGrid);
       System.Diagnostics.Debug.WriteLine("A BDD "+ commandeBDD.NumCommande);
       commandeBDD.Poids = poidsCommandeDataGrid;
       AfficherTestDataGrid(true); 

       constante.cashphotoBDD.SaveChanges();

   }
   else if (isValidNumCommande(numCommandeDataGrid) &&
           !commandeExist(numCommandeDataGrid) &&
           isValidPoids(poidsCommandeDataGrid.ToString()))
   {
       System.Diagnostics.Debug.WriteLine("D ");
       commandeBDD.Poids = poidsCommandeDataGrid;
       commandeBDD.NumCommande = numCommandeDataGrid;
       AfficherTestDataGrid(true);

       constante.cashphotoBDD.SaveChanges();
   }
   else
       AfficherTestDataGrid(false);

   DatagGridPrep.ItemsSource = getCommandesDateToday();

}
*/
    }
}
