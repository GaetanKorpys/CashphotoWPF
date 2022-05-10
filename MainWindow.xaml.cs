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

        //Une liste de commande.
        //Le DataGrid affiche cette liste.
        //Elle est mise à jour par 2 fonctions :
        //  - getCommandesDateToday()
        //  - getCommandesRecherche()
        private List<Commande> _commandes { get; set; }

        //On stock la commande à afficher dans la zone de récap.
        //Elle est mise à jour lorsque l'utilisateur entre une nouvelle commande
        //ou qu'il séléctionne une ligne dans le DataGrid.
        private Commande _commande { get; set; }

        private RechercheEnBoucle _rechercheEnBoucle;


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
            else if (constante.indexTabItem == 1)
            {
                CreerDossier();
                _rechercheEnBoucle = new RechercheEnBoucle(this);
            }
                

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
                
            _rechercheEnBoucle.StopRecherche();
            Close();
           
        }

        #endregion

        #region ToolBox

        /// <summary>
        /// Création des dossier (pour l'import et l'export) s'ils n'existent pas.
        /// </summary>
        private void CreerDossier()
        {
            Constante constante = Constante.GetConstante();
            System.Diagnostics.Debug.WriteLine("Dir " + constante.commandeAmazon);
            //Les dossiers d'import
            if(!Directory.Exists(constante.commandeAmazon))
                Directory.CreateDirectory(constante.commandeAmazon);
            if (!Directory.Exists(constante.commandeCashphoto))
                Directory.CreateDirectory(constante.commandeCashphoto);
            if (!Directory.Exists(constante.numeroSuiviColiposte))
                Directory.CreateDirectory(constante.numeroSuiviColiposte);

            //Les dossiers d'export
            if (!Directory.Exists(constante.numeroSuiviAmazon))
                Directory.CreateDirectory(constante.numeroSuiviAmazon);
            if (!Directory.Exists(constante.numeroSuiviCashphoto))
                Directory.CreateDirectory(constante.numeroSuiviCashphoto);

            if (!Directory.Exists(constante.commandeParsePourGLS))
                Directory.CreateDirectory(constante.commandeParsePourGLS);
            if (!Directory.Exists(constante.commandeParsePourColiposte))
                Directory.CreateDirectory(constante.commandeParsePourColiposte);

            //Les dossiers de backup
            if (!Directory.Exists(constante.backupCommandeAmazon))
                Directory.CreateDirectory(constante.backupCommandeAmazon);
            if (!Directory.Exists(constante.backupCommandeCashphoto))
                Directory.CreateDirectory(constante.backupCommandeCashphoto);
            if (!Directory.Exists(constante.backupNumeroSuiviAmazon))
                Directory.CreateDirectory(constante.backupNumeroSuiviCashphoto);
            if (!Directory.Exists(constante.backupCommandeTransporteurGLS))
                Directory.CreateDirectory(constante.backupCommandeTransporteurGLS);
            if (!Directory.Exists(constante.backupCommandeTransporteurColiposte))
                Directory.CreateDirectory(constante.backupCommandeTransporteurColiposte);
        }   



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

        /// <summary>
        /// Test si une commande existe déjà dans la BDD.
        /// <paramref name="numCommande"/>
        /// </summary>
        private bool commandeExist(string numCommande)
        {
            Constante constante = Constante.GetConstante();

            if (constante.cashphotoBDD.Commandes.Where(e => e.NumCommande == numCommande).Count() == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Construit une commande et l'ajoute à la BDD.
        /// <paramref name="numCommande"/>
        /// <paramref name="poids"/>
        /// </summary>
        private Commande validerCommande(string numCommande, string poids)
        {
            Constante constante = Constante.GetConstante();

            Commande commande = new Commande();
            commande.NumCommande = numCommande;
            double poidsD = double.Parse(poids, CultureInfo.InvariantCulture);
            commande.Poids = poidsD;
            commande.Date = DateTime.Now;
            commande.Preparer = true;
            commande.Expedier = false;
            commande.Completer = false;

            constante.cashphotoBDD.Add(commande);
            constante.cashphotoBDD.SaveChanges();

            return commande;
        }

        /// <summary>
        /// Test si un numéro de commande est valide.
        /// Les regex sont importés via le fichier de config.
        /// <paramref name="numCommande"/>
        /// </summary>
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

        /// <summary>
        /// Test si un poids est valide.
        /// <paramref name="poids"/>
        /// </summary>
        private bool isValidPoids(string poids)
        {
            string pattern = "^\\d{1,2}(\\.\\d{1,3})?$"; //Ex: 23.344 ou 34 
            Regex poidsRegex = new Regex(pattern);
            if (poidsRegex.IsMatch(poids))
                return true;
            return false;
        }

        /// <summary>
        /// Ferme l'application de la balance connectée lorsque l'on ferme la fenêtre principale du programme.
        /// </summary>
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

        /// <summary>
        /// Test si une appplication est en cours d'exécution.
        /// <paramref name="FullPath"/>
        /// </summary>
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

        /// <summary>
        /// Affiche un message dans le label pour une durée de 5s.
        /// <paramref name="label"/>
        /// <paramref name="message"/>
        /// </summary>
        private void DisplayTempMessage(System.Windows.Controls.Label label, string message)
        {
            label.Content = message; //affichage du message
            Timer aTimer = new Timer();
            aTimer.Interval = 5000; //délai de 5 seconds
            aTimer.Tick += (Object o, EventArgs e2) => { aTimer.Stop(); label.Content = ""; }; //effacement du message
            aTimer.Enabled = true; //démarrage du timer
        }

        /// <summary>
        /// Affiche une ellipse avec les couleurs définis par r, g et b pour une durée de 5s.
        /// <paramref name="ellipse"/>
        /// <paramref name="r"/>
        /// <paramref name="g"/>
        /// <paramref name="b"/>
        /// </summary>
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
        #endregion

        #region BDD
        /// <summary>
        /// Affichage le résultat du test de la connexion à la BDD.
        /// Utilise la fonction TestConnexionBDD().
        /// </summary>
        private void TestConnexionBDD_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Recherche les commandes dans la BDD ayant un numéro de commande correspondant avec celui dans la barre de recherche. 
        /// </summary>
        /// <param name="numCmd">Le numéro de la commande (pas forcement complet) à rechercher, celui entré dans la barre de recherche.</param>
        /// <returns>Une liste de commande</returns>
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
                    commandesTable = commandesTable.Where(commande => commande.Expedier == true);
                }
                else
                {
                    commandesTable = commandesTable.Where(commande => commande.Expedier == false);
                }
                commandesTable = commandesTable.OrderByDescending(commande => commande.Date);
                commandes = commandesTable.ToList();
            }
            return commandes;
        }

        /// <summary>
        /// Recherche les commandes dans la BDD qui ont pour date la date d'aujourd'hui.
        /// </summary>
        /// <returns>Une liste de commande</returns>
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

        private List<Article> getArticlesFromCommande(string numCommande)
        {
            List<Article> articles = new List<Article>();
            Constante constante = Constante.GetConstante();
            IQueryable<Article> articlesTable;

            if (constante.BDDOK)
            {
                articlesTable = constante.cashphotoBDD.Articles.Where(article => article.NumCommande == numCommande);

                articles = articlesTable.ToList();
            }
            return articles;
        }
        
        #endregion

        #region Configuration

        /// <summary>
        /// Modifie le chemin d'un dossier d'import ou d'export.
        /// La fonction est utilisée par plusieurs boutons.
        /// </summary>
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
                else if (sender.Equals(ModifierDossierNumeroSuiviCashphotoBouton))
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
        
        /// <summary>
        /// Modifie les paramètres globaux du programme.
        /// Actuellement, la fonction est utilisée par 2 boutons pour modifier l'email et le numéro de téléphone.
        /// </summary>
        private void ModifierConfigurationParametre(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();

            if (sender.Equals(ModifierEmail))
            {
                Email emailClass = new Email();
                if (emailClass.ShowDialog() == true)
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

        /// <summary>
        /// Modification du fichier de configuration.
        /// La fonction est utilisé par 2 boutons :
        ///     - l'un pour charger un nouveau fichier de config
        ///     - l'autre pour sauvegarder des modifications effectuées, donc modifier le fichier de config actuel.
        /// </summary>
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

        /// <summary>
        /// Modifie le status de la BDD, soit locale (127.0.0.1) soit distante.
        /// Si la BDD est locale, on modifie directement l'IP et on se connecte à la BDD.
        /// </summary>
        private void setBDDRadioBouton(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            if (sender.Equals(BDDLocaleRadio))
            {
                BDDAdresseTextBox.IsEnabled = false;
                ModifierBDDIPButton.IsEnabled = false;

                //BDD locale, l'utilisateur ne rentre donc pas d'adresse IP
                //On valide directement dès que le bouton radio locale est choisi.
                setBDDIP("127.0.0.1");
                //Nouvelle BDD locale
                CreationBDD();;

            }
            else if (sender.Equals(BDDDistanteRadio))
            {
                ModifierBDDIPButton.IsEnabled = true;
                BDDAdresseTextBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Modification de l'IP et connexion à la BDD avec la touche ENTER du clavier.
        /// </summary>
        private void ValiderIP(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                setBDDIP(BDDAdresseTextBox.Text);
                CreationBDD();
            }
        }

        /// <summary>
        /// Modification de l'IP et connexion à la BDD avec le clic souris sur le bouton.
        /// </summary>
        private void ValiderIP(object sender, EventArgs e)
        {
            setBDDIP(BDDAdresseTextBox.Text);
            CreationBDD();
        }

        /// <summary>
        /// On affiche les chemins et autres paramètres dans le menu Configuartion.
        /// Pour cela, on récupère les informations stockées dans la classe Constante.
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
        #endregion

        #region Préparation
        //-------------ZONE RECAP-------------

        /// <summary>
        /// Permet la modification des informations selectionnées via un double clique.
        /// La fonction est utilisée pour 2 champs situé dans la zone de récapitulatif :
        ///     - le numéro de commande
        ///     - le poids du colis
        /// </summary>
        private void ModifierRecap(object sender, MouseButtonEventArgs e)
        {
            if (sender.Equals(NumCommandeRecap))
            {
                NumCommandeRecap.Focusable = true;
                NumCommandeRecap.Focus();

            }
            else if (sender.Equals(PoidsRecap))
            {
                PoidsRecap.Focusable = true;
                PoidsRecap.Focus();
            }
        }

        /// <summary>
        /// Lorsque le champ selectionné perd le focus, on le rend non focsable. 
        /// Pour pouvoir modifier ce champ, il faut double cliquer.
        /// </summary>
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

        /// <summary>
        /// Si le champ est focusable alors le curseur est la barre verticale utilisé lors de l'écriture.
        /// Sinon le curseur reste la flèche.
        /// </summary>
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

        /// <summary>
        /// On actualise la zone de récap.
        /// Si numCommande est null, alors on affiche la dernière commande enregistrée.
        /// Sinon, on affiche la commande selectionnée dans le DataGrid.
        /// </summary>
        /// <param name="numCommande">Le numéro de la commande à afficher.</param>
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

        /// <summary>
        /// On affiche un messgae pour informer l'utilisateur.
        /// Si la modification est effectuée et validée, on affiche un message positif.
        /// Sinon, on affiche une erreur.
        /// </summary>
        /// <param name="status">Le résultat du test.</param>
        private void AfficherTestRecap(bool status)
        {
            if (status)
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

        /// <summary>
        /// On modifie la propriété IsEnabled du bouton qui à pour but de valider les modifications de la zone de récap.
        /// Utilisation des fonctions isValidNumCommande() et isValidPoids().
        /// </summary>
        private void ActiverBoutonRecap(object sender, TextChangedEventArgs e)
        {
            if (isValidNumCommande(NumCommandeRecap.Text) && isValidPoids(PoidsRecap.Text))
                ValiderRecapBouton.IsEnabled = true;
            else
                ValiderRecapBouton.IsEnabled = false;
        }

        /// <summary>
        /// Recherche la commande affichée dans la zone de récap et la modifie.
        /// La modification est appliquée à la commande dans la BDD.
        /// Maj de l'affichage.
        /// </summary>
        private void ValiderRecap(object sender, RoutedEventArgs e)
        {

            Constante constante = Constante.GetConstante();

            //On utilise _commande
            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.IdCommande == _commande.IdCommande).FirstOrDefault();

            //L'utilisateur ne modifie pas le numéro de commande
            //Alors pas besoin de vérifier s'il est valide ou si une commande existe déjà.
            if(commande.NumCommande == NumCommandeRecap.Text)
            {
                double poids = double.Parse(PoidsRecap.Text, CultureInfo.InvariantCulture);
                commande.Poids = poids;

                constante.cashphotoBDD.SaveChanges();
                AfficherTestRecap(true);

                //Rechargement du Datagrid
                _commandes = getCommandesDateToday();
                DatagGridPrep.ItemsSource = _commandes;
            }
            else if (!commandeExist(NumCommandeRecap.Text))
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

        //-------------ZONE ENREGISTREMENT-------------

        /// <summary>
        /// Lorsque la touche du clavier ENTER est préssé, on valide ou non le numéro de la commande.
        /// Si le numéro de commande est correct, on passe le focus au champ suivant.(saisi du poids) 
        /// Sinon on affiche un message temporaire pour informer l'utilisateur.
        /// Utilisation de la fonction isValidNumCommande().
        /// </summary>
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

        /// <summary>
        /// On modifie la propriété IsEnabled du bouton qui à pour but de valider l'enregistrement de la commande dans la BDD.
        /// Utilisation de la fonction isValidPoids().
        /// </summary>
        private void ActivationBoutonValider(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(SaisirPoids))
            {
                if (isValidPoids(SaisirPoids.Text))
                    ValiderCommandeBouton.IsEnabled = true;
                else
                    ValiderCommandeBouton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Le TextBox du poids n'est plus Enabled lorsque l'utilisateur clique sur le TextBox numéro de commande. 
        /// </summary>
        private void BloquerFocusPoids(object sender, RoutedEventArgs e)
        {
            SaisirPoids.IsEnabled = false;
        }

        /// <summary>
        /// On traite la commande si elle n'existe pas dans la BDD.
        /// Les vérifications concernant le numéro de commande et le poids sont effectués préalablement.
        /// Si la commande est bien une nouvelle commande on effectue plusieurs opérations :
        ///     - On construit la commande (new) et l'ajoute (Add) à la BDD.
        ///     - On repasse le focus au champ numéro de commande pour entrer directement une nouvelle commande.
        ///     - On affiche un message pour infomrer l'utilisateur.
        ///     - On actualise la zone de récap pour afficher la commande qui vient d'être traitée.
        ///     - On actualise le DataGrid car il doit contenir une commande supplémentaire.
        /// Sinon on informe l'utilisateur que la commande existe déjà et on repasse le focus au champ numéro de commande.
        /// </summary>
        private void EnregistrerCommande_Click(object sender, RoutedEventArgs e)
        {
            if (!commandeExist(SaisirCommande.Text))
            {
                _commande = validerCommande(SaisirCommande.Text, SaisirPoids.Text);
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

        /// <summary>
        /// On traite la commande si elle n'existe pas dans la BDD et si le poids est valide.
        /// Les vérifications concernant le numéro de commande et le poids sont effectués préalablement.
        /// Si la commande est bien une nouvelle commande on effectue plusieurs opérations :
        ///     - On construit la commande (new) et l'ajoute (Add) à la BDD.
        ///     - On repasse le focus au champ numéro de commande pour entrer directement une nouvelle commande.
        ///     - On affiche un message pour infomrer l'utilisateur.
        ///     - On actualise la zone de récap pour afficher la commande qui vient d'être traitée.
        ///     - On actualise le DataGrid car il doit contenir une commande supplémentaire.
        /// Sinon :
        ///     - On informe l'utilisateur que la commande existe déjà ou que le poids est incorrect.
        ///     - On repasse le focus au champ numéro de commande.
        /// </summary>
        private void EnregistrerCommande_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (isValidPoids(SaisirPoids.Text))
                {
                    if (!commandeExist(SaisirCommande.Text))
                    {
                        _commande = validerCommande(SaisirCommande.Text, SaisirPoids.Text);
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
        }

        /// <summary>
        /// On affiche un messgae pour informer l'utilisateur.
        /// Si l'enregistrement est effectué et validé, on affiche un message positif.
        /// Sinon, on affiche une erreur.
        /// </summary>
        /// <param name="status">Le résultat de l'enregistrement.</param>
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

        //-------------DataGrid-------------

        /// <summary>
        /// On actualise la zone de récap en fonction de la ligne séléctionnée dans le DataGrid.
        /// </summary>
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commande commande = (Commande)DatagGridPrep.SelectedItem;
            if (commande != null)
            {
                _commande = commande;
                ActualiserRecapEnregistrementCommande(commande.NumCommande);
                ActualiserDataGridArticle(commande.NumCommande);
            }
        }

        /// <summary>
        /// Si la barre de recherche contient du texte, on affiche les commandes correspondantes.
        /// Sinon, si la barre de recherche est vide, on affiche les commandes qui ont été enregistré aujourd'hui.
        /// </summary>
        private void rechercheCommande_TextChanged(object sender, EventArgs e)
        {
            if (RechercherCommande1.Text.Equals(""))
                _commandes = getCommandesDateToday();
            else
                _commandes = getCommandesRecherche(RechercherCommande1.Text);
            DatagGridPrep.ItemsSource = _commandes;
        }

        
        #endregion

        #region Expédition

        private void ActualiserDataGridArticle(string numCommande)
        {
            DatagGridArticle.ItemsSource = getArticlesFromCommande(numCommande);
        }

        public void ImporterCommandes() 
        {
            Importation importation = new Importation();

            int nbcommandes = importation.ImportCommandes();
            if (nbcommandes != 0)
            {
                string import = nbcommandes + " commande(s) importée(s). " + Regex.Replace(System.DateTime.Now.TimeOfDay.ToString(), "\\.\\d+$", "");
                DisplayTempMessage(NbCommandeImport, import);
            }
            
        }
        #endregion

        /// <summary>
        /// Effectue des actions lorsque l'utilisateur navigue vers un menu.
        /// Si l'utilisateur clique sur le menu Configuration, on ouvre une boite de dialogue.
        /// </summary>
        private void GestionTabItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        //Important sinon la boite de dialogue prend le focus et perturbe le TabControl
                        //Cela entraine un bug qui déclenche 2 fois la boite de dialogue
                        TabControl.Focus();
                        TabControl.SelectedIndex = constante.indexTabItem;
                    }
                }
            }
        }
    }
}
