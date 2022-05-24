using CashphotoWPF.Configuration;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Microsoft.EntityFrameworkCore;

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

        private TextBox _focusedTextBox { get; set; }

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

            TestConnexionBDD();

            //Chargement des DataGrid
            //On affiche les commandes traitées ce jour.
            _commandes = getCommandesDateToday(false);
            DataGridPrep.ItemsSource = _commandes;
            DataGridExpe.ItemsSource = _commandes;
            
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

            if(_rechercheEnBoucle != null)
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
           

            constante.cashphotoBDD.Add(commande);
            constante.cashphotoBDD.SaveChanges();

            return commande;
        }

        private Commande CompleterCommande(Commande commande, string poids, bool secondColis)
        {
            Constante constante = Constante.GetConstante();

            Commande cmd = constante.cashphotoBDD.Commandes.Where(c => c.NumCommande == commande.NumCommande).First();

            if (secondColis)
                cmd.Poids2 = double.Parse(poids, CultureInfo.InvariantCulture);
            else
                cmd.Poids = double.Parse(poids, CultureInfo.InvariantCulture);

            cmd.Preparer = true;

            constante.cashphotoBDD.SaveChanges();

            return cmd;
        }


        public bool isCompleteCommande(Commande commande)
        {
            if (commande.NumCommande != null && commande.Poids != null && commande.NomClientLivraison != null)
                return true;
            return false;
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
                return true;
            }

            return false;

        }


        /// <summary>
        /// Test si un poids est valide.
        /// <paramref name="poids"/>
        /// </summary>
        public bool isValidPoids(string poids)
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
            TestConnexionBDD();
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
        private void TestConnexionBDD()
        {
            Constante constante = Constante.GetConstante();
      
            constante.cashphotoBDD = new CashphotoBDD();

            if (constante.cashphotoBDD.Database.CanConnect())
                constante.BDDOK = true;                   
            else
                constante.BDDOK = false;

            AfficherTestBDD();
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
        private List<Commande> getCommandesRecherche(string numCmd, bool expedier)
        {
            List<Commande> commandes = new List<Commande>();
            IQueryable<Commande> commandesTable;

            Constante constante = Constante.GetConstante();
            //constante.cashphotoBDD = new CashphotoBDD();

            if (constante.BDDOK)
            {
                commandesTable = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande.Contains(numCmd) && commande.Expedier == expedier);
                commandesTable = commandesTable.OrderByDescending(commande => commande.Date);
                commandes = commandesTable.ToList();

                foreach (Commande commande in commandes)
                {
                    constante.cashphotoBDD.Entry(commande).Reload();
                }

            }
            //constante.cashphotoBDD.Dispose();
            return commandes;
        }

        /// <summary>
        /// Recherche les commandes dans la BDD qui ont pour date la date d'aujourd'hui.
        /// </summary>
        /// <returns>Une liste de commande</returns>
        private List<Commande> getCommandesDateToday(bool expedier)
        {
            List<Commande> commandes = new List<Commande>();
            IQueryable<Commande> commandesTable;

            Constante constante = Constante.GetConstante();
            //constante.cashphotoBDD = new CashphotoBDD();

            if (constante.BDDOK)
            {
                commandesTable = constante.cashphotoBDD.Commandes.Where(commande => commande.Date.Date == DateTime.Today && commande.Expedier == expedier);
                commandesTable = commandesTable.OrderByDescending(commande => commande.Date);
                commandes = commandesTable.ToList();

                foreach (Commande commande in commandes)
                {
                    constante.cashphotoBDD.Entry(commande).Reload();
                }
            }
            //constante.cashphotoBDD.Dispose();
            return commandes;
        }

        private List<Article> getArticlesFromCommande(string numCommande)
        {
            List<Article> articles = new List<Article>();
            IQueryable<Article> articlesTable;

            Constante constante = Constante.GetConstante();
         
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
                TestConnexionBDD();

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

                //Test pour la nouvelle BDD
                Constante constante = Constante.GetConstante();
                TestConnexionBDD();;
            }
        }

        /// <summary>
        /// Modification de l'IP et connexion à la BDD avec le clic souris sur le bouton.
        /// </summary>
        private void ValiderIP(object sender, EventArgs e)
        {
            setBDDIP(BDDAdresseTextBox.Text);

            //Test pour la nouvelle BDD
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
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

            if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                Coliposte.IsChecked = true;
            else
                GLS.IsChecked = true;

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

        private void Recap_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(NumCommandeRecap))
            {
                _focusedTextBox = NumCommandeRecap;
            }
            else if (sender.Equals(PoidsRecap))
            {
                _focusedTextBox = PoidsRecap;
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

            if(_commande != null)
            {
                Constante constante = Constante.GetConstante();

                //On utilise _commande
                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).FirstOrDefault();

                //L'utilisateur ne modifie pas le numéro de commande
                //Alors pas besoin de vérifier s'il est valide ou si une commande existe déjà.
                if (commande.NumCommande == NumCommandeRecap.Text)
                {
                   
                    double poids = double.Parse(PoidsRecap.Text, CultureInfo.InvariantCulture);
                    commande.Poids = poids;
                    commande.Preparer = true;

                    constante.cashphotoBDD.SaveChanges();
                 
                    AfficherTestRecap(true);

                    //Rechargement des Datagrid
                    _commandes = getCommandesDateToday(false);
                    DataGridPrep.Items.Refresh();
                    DataGridExpe.Items.Refresh();
                }
                else if (!commandeExist(NumCommandeRecap.Text))
                {
                    commande.NumCommande = NumCommandeRecap.Text;
                    double poids = double.Parse(PoidsRecap.Text, CultureInfo.InvariantCulture);
                    commande.Poids = poids;

                    constante.cashphotoBDD.SaveChanges();
                    AfficherTestRecap(true);

                    //Rechargement des Datagrid
                    _commandes = getCommandesDateToday(false);
                    DataGridPrep.Items.Refresh();
                    DataGridExpe.Items.Refresh();
                }

                else
                    AfficherTestRecap(false);
            }
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
        private void SaisirCommande_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Shee Focus");
            SaisirPoids.IsEnabled = false;
            _focusedTextBox = SaisirCommande;
        }

        private void SaisirPoids_GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedTextBox = SaisirPoids;
        }

        private void RechercherCommande1_GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedTextBox = RechercherCommande1;
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
                if((bool)Expedier_CheckBox.IsChecked)
                    _commandes = getCommandesDateToday(true);
                else
                    _commandes = getCommandesDateToday(false);
                DataGridPrep.ItemsSource = _commandes;
            }
            else
            {
                Constante constante = Constante.GetConstante();
                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == SaisirCommande.Text).First();

                if (commande.Preparer == false)
                {
                    _commande = CompleterCommande(commande, SaisirPoids.Text, false);
                    SaisirPoids.Text = "";
                    SaisirCommande.Text = "";
                    SaisirCommande.IsEnabled = true;
                    SaisirCommande.Focus();
                    SaisirPoids.IsEnabled = false;
                    AfficherTestEnregistrementCommande(true);
                    ActualiserRecapEnregistrementCommande(null);

                    //Rechargement du Datagrid
                    if ((bool)Expedier_CheckBox.IsChecked)
                        _commandes = getCommandesDateToday(true);
                    else
                        _commandes = getCommandesDateToday(false);
                    DataGridPrep.ItemsSource = _commandes;
                }
                else if((bool)ColisSupplementaire_CheckBox.IsChecked)
                {

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
                        if ((bool)Expedier_CheckBox.IsChecked)
                            _commandes = getCommandesDateToday(true);
                        else
                            _commandes = getCommandesDateToday(false);
                        DataGridPrep.ItemsSource = _commandes;
                    }
                    else
                    {
                        Constante constante = Constante.GetConstante();
                        Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == SaisirCommande.Text).First();

                        if((bool)ColisSupplementaire_CheckBox.IsChecked)
                        {
                            _commande = CompleterCommande(commande, SaisirPoids.Text, true);
                            SaisirPoids.Text = "";
                            SaisirCommande.Text = "";
                            SaisirCommande.IsEnabled = true;
                            SaisirCommande.Focus();
                            SaisirPoids.IsEnabled = false;
                            AfficherTestEnregistrementCommande(true);
                            ActualiserRecapEnregistrementCommande(null);

                            //Rechargement du Datagrid
                            if ((bool)Expedier_CheckBox.IsChecked)
                                _commandes = getCommandesDateToday(true);
                            else
                                _commandes = getCommandesDateToday(false);
                            DataGridPrep.ItemsSource = _commandes;

                        }
                        else if (commande.Preparer == false)
                        {
                            _commande = CompleterCommande(commande, SaisirPoids.Text, false);
                            SaisirPoids.Text = "";
                            SaisirCommande.Text = "";
                            SaisirCommande.IsEnabled = true;
                            SaisirCommande.Focus();
                            SaisirPoids.IsEnabled = false;
                            AfficherTestEnregistrementCommande(true);
                            ActualiserRecapEnregistrementCommande(null);                         

                            //Rechargement du Datagrid
                            if ((bool)Expedier_CheckBox.IsChecked)
                                _commandes = getCommandesDateToday(true);
                            else
                                _commandes = getCommandesDateToday(false);
                            DataGridPrep.ItemsSource = _commandes;
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

        //-------------Clavier Virtuel-------------

        private void Key0_Click(object sender, RoutedEventArgs e)
        { 
            _focusedTextBox.Text += "0";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key1_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "1";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key2_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "2";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key3_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "3";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key4_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "4";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key5_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "5";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key6_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "6";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key7_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "7";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key8_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "8";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key9_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "9";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void KeyDot_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += ".";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void KeyDelete_Click(object sender, RoutedEventArgs e)
        {
            //_focusedTextBox
            var routedEvent = Keyboard.KeyDownEvent;
            _focusedTextBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(_focusedTextBox),0,Key.Back){ RoutedEvent = routedEvent });

        }

        private void KeyEnter_Click(object sender, RoutedEventArgs e)
        {
            var routedEvent = Keyboard.KeyDownEvent;
            _focusedTextBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(_focusedTextBox), 0, Key.Enter) { RoutedEvent = routedEvent });
        }
        //-------------DataGrid-------------

        /// <summary>
        /// On actualise la zone de récap en fonction de la ligne séléctionnée dans le DataGrid.
        /// </summary>
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender.Equals(DataGridPrep))
            {
       
                Commande commande = (Commande)DataGridPrep.SelectedItem;
                if (commande != null)
                {
                    _commande = commande;
                    ActualiserRecapEnregistrementCommande(commande.NumCommande);
                }
            }
            else if(sender.Equals(DataGridExpe))
            {
                Commande commande = (Commande)DataGridExpe.SelectedItem;
                if (commande != null)
                {
                    _commande = commande;
                    ActualiserDataGridArticle(commande.NumCommande);
                    ActualiserRecapExpe(commande.NumCommande);

                    if(commande.Site == "Cashphoto")
                    {
                        Coliposte.IsChecked = true;
                        AmazonPageBouton.IsEnabled = false;
                    }
                    else
                    {
                        AmazonPageBouton.IsEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Si la barre de recherche contient du texte, on affiche les commandes correspondantes.
        /// Sinon, si la barre de recherche est vide, on affiche les commandes qui ont été enregistré aujourd'hui.
        /// </summary>
        private void rechercheCommande_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(RechercherCommande1))
            {
                if (RechercherCommande1.Text.Equals(""))
                    _commandes = getCommandesDateToday(false);
                else
                    _commandes = getCommandesRecherche(RechercherCommande1.Text, false);

                DataGridPrep.ItemsSource = _commandes;

                if (DataGridPrep.Items.Count == 1)
                {
                    _commande = DataGridPrep.Items[0] as Commande;
                    ActualiserRecapEnregistrementCommande(_commande.NumCommande);
                }
            }
            else if(sender.Equals(RechercherCommande2))
            {
                bool expedier;
                if ((bool)Expedier_CheckBox.IsChecked)
                    expedier = true;
                else
                    expedier = false;
                

                if (RechercherCommande2.Text.Equals(""))
                    _commandes = getCommandesDateToday(expedier);
                else
                    _commandes = getCommandesRecherche(RechercherCommande2.Text, expedier);

                DataGridExpe.ItemsSource = _commandes;

                if (DataGridExpe.Items.Count == 1)
                {
                    _commande = DataGridExpe.Items[0] as Commande;
                    ActualiserRecapExpe(_commande.NumCommande);
                    
                    if (_commande.Site == "Cashphoto")
                    {
                        Coliposte.IsChecked = true;
                        AmazonPageBouton.IsEnabled = false;
                    }
                    else
                    {
                        AmazonPageBouton.IsEnabled = true;
                    }
                        
                }
            }
            
        }


        #endregion

        #region Expédition

        private void CommandeExpedier_Checked(object sender, RoutedEventArgs e)
        {
            _commandes = getCommandesDateToday(true);
            DataGridExpe.ItemsSource = _commandes;
        }

        private void CommandeExpedier_Unchecked(object sender, RoutedEventArgs e)
        {
            _commandes = getCommandesDateToday(false);
            DataGridExpe.ItemsSource = _commandes;
        }

        private void ModifierPoids_Click(object sender, RoutedEventArgs e)
        {
            if(_commande != null)
            {
                Constante constante = Constante.GetConstante();

                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                Poids poidsDialog = new Poids(this);
                if (poidsDialog.ShowDialog() == true)
                {
                    commande.Poids = double.Parse(poidsDialog.InputTextBox.Text, CultureInfo.InvariantCulture);
                    commande.Preparer = true;
                    constante.cashphotoBDD.SaveChanges();

                    _commandes = getCommandesDateToday((bool)Expedier_CheckBox.IsChecked);
                    DataGridExpe.ItemsSource = _commandes;
                    DataGridPrep.ItemsSource = _commandes;

                    DisplayTempMessage(Message, "Modification du poids validée.");
                }
            }   
        }

        private void ExpedierCommande_Click(object sender, RoutedEventArgs e)
        {
            if(_commande != null)
            {
                Constante constante = Constante.GetConstante();
                Expedition expedition = new Expedition();
                Suivi suivi = new Suivi(this);

                string export = "";
                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                if (commande.Expedier)
                    export = "La commande est déjà expédiée.";

                else
                {
                    if (isCompleteCommande(commande) && commande.Preparer)
                    {

                        commande.Expedier = true;

                        constante.cashphotoBDD.SaveChanges();

                        expedition.ExpedierCommande(commande);

                        if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                            suivi.createSuiviFromCommande(commande);

                        _commandes = getCommandesDateToday(false);
                        DataGridExpe.ItemsSource = _commandes;
                        DataGridPrep.ItemsSource = _commandes;

                        export = "La commande de " + commande.NomClientLivraison + " est expédiée.";
                    }
                    else if (isCompleteCommande(commande) && commande.Preparer == false)
                    {
                        ConfirmationPoids confirmationPoids = new ConfirmationPoids(this);
                        if (confirmationPoids.ShowDialog() == true)
                        {
                            commande.Expedier = true;
                            commande.Poids = double.Parse(confirmationPoids.InputTextBox.Text, CultureInfo.InvariantCulture);

                            constante.cashphotoBDD.SaveChanges();

                            expedition.ExpedierCommande(commande);

                            if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                                suivi.createSuiviFromCommande(commande);

                            _commandes = getCommandesDateToday(false);
                            DataGridExpe.ItemsSource = _commandes;
                            DataGridPrep.ItemsSource = _commandes;

                            export = "La commande de " + commande.NomClientLivraison + " est expédiée.";
                        }
                    }
                    else
                        export = "La commande ne peut pas être expédiée.";
                }
                DisplayTempMessage(Message, export);          
            }
        }


        private void ModifierTransporteur(object sender, RoutedEventArgs e)
        {
            Constante constante = Constante.GetConstante();
            if(sender.Equals(GLS))
            {
                constante.transporteur = Transporteur.Transporteurs.GLS;
                ExpediteurLabel.Content = "GLS";
            }
            else if (sender.Equals(Coliposte))
            {
                constante.transporteur = Transporteur.Transporteurs.Coliposte;
                ExpediteurLabel.Content = "Coliposte";
            }
        }

        private void ActualiserRecapExpe(string numCommande)
        {
            Constante constante = Constante.GetConstante();
            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();

            NumCommandeRecapExpe.Content = commande.NumCommande;
            PoidsCommandeRecapExpe.Content = commande.Poids;
            DateCommandeRecapExpe.Content = commande.Date;
            NomClientRecapExpe.Content = commande.NomClientLivraison;
            TelClientRecapExpe.Content = commande.TelClientLivraison;
            AdresseClientRecapExpe.Content = commande.Adresse1;
            AdresseClient2RecapExpe.Content = commande.Adresse2;
            AdresseClient3RecapExpe.Content = commande.Adresse3;
            CodePostaleVilleRecapExpe.Content = commande.CodePostal + " - " + commande.Ville;
            PaysRecapExpe.Content = commande.Pays;
            EmailClientRecapExpe.Content = commande.Mail;
        }

        private void ActualiserDataGridArticle(string numCommande)
        {
            DatagGridArticle.ItemsSource = getArticlesFromCommande(numCommande);
        }

        public void ImporterCommandes() 
        {
            Importation importation = new Importation(this);

            int nbcommandes = importation.ImportCommandes();
            if (nbcommandes != 0)
            {
                string import = nbcommandes + " commande(s) importée(s). " + Regex.Replace(System.DateTime.Now.TimeOfDay.ToString(), "\\.\\d+$", "");
                DisplayTempMessage(Message, import);

                _commandes = getCommandesDateToday(false);
                DataGridExpe.ItemsSource = _commandes;

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
                    _focusedTextBox = SaisirCommande;

                }
                else if (Expedition.IsSelected)
                {
                    constante.indexTabItem = 1;
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
