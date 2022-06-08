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
using System.Windows.Threading;
using MahApps.Metro.Controls;
using System.Drawing.Printing;
using IronBarCode;
using System.Drawing;
using Brushes = System.Drawing.Brushes;
using Color = System.Windows.Media.Color;
using QRCoder;
using System.Drawing.Imaging;

namespace CashphotoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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

        private TimerSuiviFile _timer;

        private TextBox _focusedTextBox { get; set; }

        private int _tabItem { get; set; }

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
            constante.nbCommandeBDD = 15;

            
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

            //On masque les colonnes contenant le poids si plusieurs colis sont nécéssaires
            DataGridPrep.Columns[2].Visibility = Visibility.Collapsed;
            DataGridPrep.Columns[3].Visibility = Visibility.Collapsed;
            DataGridPrep.Columns[4].Visibility = Visibility.Collapsed;

            DataGridExpe.Columns[2].Visibility = Visibility.Collapsed;
            DataGridExpe.Columns[3].Visibility = Visibility.Collapsed;
            DataGridExpe.Columns[4].Visibility = Visibility.Collapsed;

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
           
        }

        #endregion

        #region ToolBox

        private int getPoliceFromLength(string input)
        {
            int police;
            if (input.Length <= 14)
                police = 10;
            else if (input.Length <= 18)
                police = 9;
            else
                police = 8;
            return police;
        }

        private void PrintRecapIRL(Commande commande)
        {
            System.Diagnostics.Debug.WriteLine("OKKK");

            PrintDocument pd = new PrintDocument();
            int police;
            string nom = "";

            if (commande.NomClientLivraison != null && commande.NomClientLivraison != "")
                nom = commande.NomClientLivraison;
            else if (commande.NomClientFacturation != null && commande.NomClientFacturation != "")
                nom = commande.NomClientFacturation;

            police = getPoliceFromLength(nom);

            pd.PrinterSettings = new PrinterSettings
            {
                PrinterName = "Brother TD-2020"
            };
            pd.PrintPage += (sender, args) =>
            {
                System.Drawing.Image img = CreateQRCode(commande);
                img.Save("QRCode.png", ImageFormat.Png);
                System.Drawing.Rectangle m = args.PageBounds;

                if (commande.Site!= null && commande.Site == "Amazon")
                {
                    string debut, fin;
                    fin = commande.NumCommande.Substring(12);
                    debut = commande.NumCommande.Substring(0, 11);

                    if(nom != "")
                    {
                        string[] tokens = nom.Split(" ");
                        if (IsAllUpper(tokens[0]))
                            args.Graphics.DrawString(tokens[0] + " " + tokens[1], new Font("Arial", police), Brushes.Black, 75, 0);

                        else if (IsAllUpper(tokens[1]))
                            args.Graphics.DrawString(tokens[1] + " " + tokens[0], new Font("Arial", police), Brushes.Black, 75, 0);
                        else
                            args.Graphics.DrawString(nom, new Font("Arial", police), Brushes.Black, 75, 0);

                    }
                    args.Graphics.DrawString(debut, new Font("Arial", 11), Brushes.Black, 75, 25);
                    args.Graphics.DrawString(fin, new Font("Arial", 14), Brushes.Black, 75, 50);
                }
                else if (commande.Site != null && commande.Site == "Cashphoto")
                {
                    if(nom != "")
                    {
                        string[] tokens = commande.NomClientLivraison.Split(" ");
                        args.Graphics.DrawString(tokens[0].ToUpper() + " " + tokens[1], new Font("Arial", police), Brushes.Black, 75, 0);
                    }
                    args.Graphics.DrawString("CPC " + commande.NumCommande, new Font("Arial", 14), Brushes.Black, 75, 30);
                }

                m.Width = 70;
                m.Height = 70;

                args.Graphics.DrawImage(img, m);

            };
            pd.Print();
        }

        private Bitmap CreateQRCode(Commande commande)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(commande.NumCommande, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
            //QRCode.SaveAsPng(GetCheminQRCode());
        }

        bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }

        private string ToNumberAndDot(string s)
        {
            s = s.ToLower();

            s = s.Replace("&", "1");
            s = s.Replace("é", "2");
            s = s.Replace("\"", "3");
            s = s.Replace("\'", "4");
            s = s.Replace("(", "5");
            s = s.Replace("-", "6");
            s = s.Replace("è", "7");
            s = s.Replace("_", "8");
            s = s.Replace("ç", "9");
            s = s.Replace("à", "0");
            s = s.Replace(";", ".");

            return s;
        }

        /// <summary>
        /// Création des dossier (pour l'import et l'export) s'ils n'existent pas.
        /// </summary>
        private void CreerDossier()
        {
            Constante constante = Constante.GetConstante();

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
        private string GetCheminQRCode()
        {
            string chemin = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
            chemin += "\\";
            chemin += "QRCode.png";
            return chemin;
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
            commande.NbColis = 1;
            commande.Preparer = true;
            commande.Expedier = false;
           

            constante.cashphotoBDD.Add(commande);
            constante.cashphotoBDD.SaveChanges();

            return commande;
        }

        private Commande CompleterCommande(Commande commande, string poids, bool colisSupp)
        {
            Constante constante = Constante.GetConstante();

            Commande cmd = constante.cashphotoBDD.Commandes.Where(c => c.NumCommande == commande.NumCommande).First();

            if (colisSupp)
            {
                cmd.NbColis++;
                switch(cmd.NbColis)
                {
                    case 2:
                        cmd.Poids2 = double.Parse(poids, CultureInfo.InvariantCulture);
                        break;

                    case 3:
                        cmd.Poids3 = double.Parse(poids, CultureInfo.InvariantCulture);
                        break;

                    case 4:
                        cmd.Poids4 = double.Parse(poids, CultureInfo.InvariantCulture);
                        break;

                    default:
                        break;
                }
            }
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
                DisplayTempMessage(ReponseBDD, "Erreur connexion");
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
        public void TestConnexionBDD()
        {
            Constante constante = Constante.GetConstante();
      
            constante.cashphotoBDD = new CashphotoBDD();

            if (constante.cashphotoBDD.Database.CanConnect())
            {
                constante.BDDOK = true;

                if (constante.indexTabItem == 1 && _rechercheEnBoucle._enMarche == false)
                    _rechercheEnBoucle.ActiverRecherche();
            }                 
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

            constante.BDDIP = IP;
            DisplayTempMessage(ReponseBDD, "Adresse IP validée.");

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

            TestConnexionBDD();
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
            TestConnexionBDD();
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
            return commandes;
        }

        private List<Article> getArticlesFromCommande(string numCommande)
        {
            List<Article> articles = new List<Article>();
            IQueryable<Article> articlesTable;

            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if (constante.BDDOK)
            {
                articlesTable = constante.cashphotoBDD.Articles.Where(article => article.NumCommande == numCommande).Take(constante.nbCommandeBDD);
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
        private void ModifierConfigurationParametre(object sender, RoutedEventArgs e)
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
        private void setBDDRadioBouton(object sender, RoutedEventArgs e)
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
        private void ValiderIP_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (IsValidIPAddress(BDDAdresseTextBox.Text))
                    setBDDIP(BDDAdresseTextBox.Text);
            }
        }

        /// <summary>
        /// Modification de l'IP et connexion à la BDD avec le clic souris sur le bouton.
        /// </summary>
        private void ValiderIP_Click(object sender, EventArgs e)
        {
            if (IsValidIPAddress(BDDAdresseTextBox.Text))
                setBDDIP(BDDAdresseTextBox.Text);

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

        private void PoidsRecap_GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedTextBox = PoidsRecap;
        }

        private void NumCommandeRecap_GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedTextBox = NumCommandeRecap;
        }

        private void RecapNumCommande_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_focusedTextBox == NumCommandeRecap)
            {
                if (isValidNumCommande(NumCommandeRecap.Text))
                    BoutonNumCommandeRecap.IsEnabled = true;
                else
                    BoutonNumCommandeRecap.IsEnabled = false;
            }
        }

        private void BoutonNumCommandeRecap_Click(object sender, RoutedEventArgs e)
        {
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if (constante.BDDOK)
            {
                if (commandeExist(NumCommandeRecap.Text))
                    AfficherTestRecap(false, "La commande existe déjà.");
                else
                {
                    Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                    commande.NumCommande = NumCommandeRecap.Text;
                    constante.cashphotoBDD.SaveChanges();

                    _commande = commande;
                    AfficherTestRecap(true, "Numéro de commande modifié.");

                    Actualiser_2DataGrids(false);
                }
            }
        }


        private void RecapPoids_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_focusedTextBox == PoidsRecap)
            {
                if (isValidPoids(PoidsRecap.Text))
                    BoutonPoidsRecap.IsEnabled = true;
                else
                    BoutonPoidsRecap.IsEnabled = false;
            }
        }

        private void BoutonPoidsRecap_Click(object sender, RoutedEventArgs e)
        {
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if (constante.BDDOK)
            {
                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                commande.Poids = double.Parse(PoidsRecap.Text, CultureInfo.InvariantCulture);
                constante.cashphotoBDD.SaveChanges();

                _commande = commande;

                AfficherTestRecap(true, "Poids modifié.");

                Actualiser_2DataGrids(false);
            }
        }


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
        /// </summary>
        /// <param name="commande">La commande à afficher.</param>
        private void ActualiserRecapEnregistrementCommande(Commande commande)
        {
            Constante constante = Constante.GetConstante();

            NumCommandeRecap.Text = commande.NumCommande;

            if (commande.Poids2 != null)
                PoidsRecap.Text = commande.Poids.ToString().Replace(",", ".") + " | " + commande.Poids2.ToString().Replace(",", ".");
            else
                PoidsRecap.Text = commande.Poids.ToString().Replace(",", ".");
        }

        /// <summary>
        /// On affiche un messgae pour informer l'utilisateur.
        /// Si la modification est effectuée et validée, on affiche un message positif.
        /// Sinon, on affiche une erreur.
        /// </summary>
        /// <param name="status">Le résultat du test.</param>
        private void AfficherTestRecap(bool status, string message)
        {
            if (status)
            {
                DisplayTempMessage(RecapLabel, message);
                DisplayTempEllipse(LedEnregistrementRecap, 0, 255, 0);
            }
            else
            {
                DisplayTempMessage(RecapLabel, message);
                DisplayTempEllipse(LedEnregistrementRecap, 255, 0, 0);
            }

        }

        //-------------ZONE ENREGISTREMENT-------------

        private void EnregistrementCommande()
        {
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if (constante.BDDOK)
            {
                if (!commandeExist(SaisirCommande.Text))
                {
                    if (ColisSupplementaire_CheckBox.IsChecked == false)
                    {
                        _commande = validerCommande(SaisirCommande.Text, SaisirPoids.Text);
                        ResetFocusEnregistrementCommande();
                        AfficherTestEnregistrementCommande(true, "Enregistrement OK.");
                        ActualiserRecapEnregistrementCommande(_commande);

                        Actualiser_2DataGrids(false);

                        System.Diagnostics.Debug.WriteLine("dd " + constante.indexTabItem);
                        if(constante.indexTabItem == 0)
                            PrintRecapIRL(_commande);
                    }
                    else
                        AfficherTestEnregistrementCommande(false, "Veuillez créer le 1er colis avant d'ajouter le colis supplémentaire.");
                }
                else
                {
                    Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == SaisirCommande.Text).First();

                    if (commande.Preparer == false)
                    {
                        if (ColisSupplementaire_CheckBox.IsChecked == false)
                        {
                            _commande = CompleterCommande(commande, SaisirPoids.Text, false);
                            ResetFocusEnregistrementCommande();
                            AfficherTestEnregistrementCommande(true, "Enregistrement OK.");
                            ActualiserRecapEnregistrementCommande(_commande);

                            Actualiser_2DataGrids(false);

                            if (constante.indexTabItem == 0)
                                PrintRecapIRL(_commande);
                        }
                        else
                            AfficherTestEnregistrementCommande(false, "Veuillez créer le 1er colis avant d'ajouter le colis supplémentaire.");
                    }
                    else
                    {
                        if (ColisSupplementaire_CheckBox.IsChecked == true)
                        {
                            if (commande.NbColis == 4)
                                AfficherTestEnregistrementCommande(false, "Le nombre de colis maximum est atteint : 4 colis pour cette commande.");
                            else
                            {
                                _commande = CompleterCommande(commande, SaisirPoids.Text, true);
                                ResetFocusEnregistrementCommande();
                                AfficherTestEnregistrementCommande(true, "Enregistrement du second colis OK.");
                                ActualiserRecapEnregistrementCommande(_commande);

                                Actualiser_2DataGrids(false);

                                if (constante.indexTabItem == 0)
                                    PrintRecapIRL(_commande);
                            }
                        }
                        else
                            AfficherTestEnregistrementCommande(false, "La commande existe déjà.");
                    }
                }
            }
            
        }

        private void ResetFocusEnregistrementCommande()
        {
            SaisirPoids.Text = "";
            SaisirCommande.Text = "";
            SaisirCommande.IsEnabled = true;
            SaisirCommande.Focus();
            SaisirPoids.IsEnabled = false;
        }
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
                string stringToUpper = ToNumberAndDot(SaisirPoids.Text);
                SaisirPoids.CaretIndex = SaisirPoids.Text.Length;
                SaisirPoids.Text = stringToUpper;
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
            EnregistrementCommande();

            if ((bool)ColisSupplementaire_CheckBox.IsChecked)
                ColisSupplementaire_CheckBox.IsChecked = false;

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
                    EnregistrementCommande();

                    if ((bool)ColisSupplementaire_CheckBox.IsChecked)
                        ColisSupplementaire_CheckBox.IsChecked = false;
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
        private void AfficherTestEnregistrementCommande(bool statusLed, string message)
        {
            if (statusLed)
            {
                DisplayTempMessage(PrepCommandeLabel, message);
                DisplayTempEllipse(LedEnregistrementCommande, 0, 255, 0);
            }
            else
            {
                DisplayTempMessage(PrepCommandeLabel, message);
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

        private void ShowOrHideDatagridColumn(DataGrid dg, int NbColis)
        {
            switch (NbColis)
            {
                case 1:
                    dg.Columns[2].Visibility = Visibility.Collapsed;
                    dg.Columns[3].Visibility = Visibility.Collapsed;
                    dg.Columns[4].Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    dg.Columns[2].Visibility = Visibility.Visible;
                    dg.Columns[3].Visibility = Visibility.Collapsed;
                    dg.Columns[4].Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    dg.Columns[2].Visibility = Visibility.Visible;
                    dg.Columns[3].Visibility = Visibility.Visible;
                    dg.Columns[4].Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    dg.Columns[2].Visibility = Visibility.Visible;
                    dg.Columns[3].Visibility = Visibility.Visible;
                    dg.Columns[4].Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void Actualiser_DataGridPrep()
        {
            if (RechercherCommande1.Text.Equals(""))
                _commandes = getCommandesDateToday(false);
            else
                _commandes = getCommandesRecherche(RechercherCommande1.Text, false);
            DataGridPrep.ItemsSource = _commandes;

            int max = 1;

            foreach(Commande cmd in _commandes)
            {
                if (cmd.NbColis > max)
                    max = cmd.NbColis;
            }
            ShowOrHideDatagridColumn(DataGridPrep, max);
        }

        private void Actualiser_DataGridExpe(bool expedie)
        {
            if (RechercherCommande2.Text.Equals(""))
                _commandes = getCommandesDateToday(expedie);
            else
                _commandes = getCommandesRecherche(RechercherCommande2.Text, expedie);
            DataGridExpe.ItemsSource = _commandes;

            int max = 1;

            foreach (Commande cmd in _commandes)
            {
                if (cmd.NbColis > max)
                    max = cmd.NbColis;
            }
            ShowOrHideDatagridColumn(DataGridExpe, max);
        }

        private void Actualiser_2DataGrids(bool expedie)
        {
            Actualiser_DataGridPrep();
            Actualiser_DataGridExpe(expedie);
        }

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
                    SaisirCommande.Focus();
                    SaisirCommande.Text = _commande.NumCommande;
                    SaisirCommande.SelectAll();

                    ActualiserRecapEnregistrementCommande(_commande);

                    NumCommandeRecap_WrapPanel.Visibility = Visibility.Visible;
                    NumCommandeRecap.Focusable = true;

                    PoidsRecap_WrapPanel.Visibility = Visibility.Visible;
                    PoidsRecap.Focusable = true;
                }
            }
            else if(sender.Equals(DataGridExpe))
            {
                Commande commande = (Commande)DataGridExpe.SelectedItem;
                if (commande != null)
                {
                    _commande = commande;

                    RecapExpe_WrapPanel.Visibility = Visibility.Visible;
                    DatagGridArticle.Visibility = Visibility.Visible;

                    ActualiserDataGridArticle(commande.NumCommande);
                    ActualiserRecapExpe(commande.NumCommande);

                    if(commande.Site == "Cashphoto")
                    {
                        Coliposte.IsChecked = true;
                        GLS.IsEnabled = false;
                        AmazonPageBouton.IsEnabled = false;
                    }
                    else
                    {
                        GLS.IsEnabled = true;
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
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if(constante.BDDOK)
            {
                if (sender.Equals(RechercherCommande1))
                {
                    Actualiser_DataGridPrep();

                    if (DataGridPrep.Items.Count == 1)
                    {
                        _commande = DataGridPrep.Items[0] as Commande;

                        ActualiserRecapEnregistrementCommande(_commande);
                        PoidsRecap_WrapPanel.Visibility = Visibility.Visible;
                        NumCommandeRecap_WrapPanel.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        PoidsRecap_WrapPanel.Visibility = Visibility.Hidden;
                        NumCommandeRecap_WrapPanel.Visibility = Visibility.Hidden;
                    }

                }
                else if (sender.Equals(RechercherCommande2))
                {
                    bool expedier;
                    if ((bool)Expedier_CheckBox.IsChecked)
                        expedier = true;
                    else
                        expedier = false;

                    Actualiser_DataGridExpe(expedier);

                    if (DataGridExpe.Items.Count == 1)
                    {
                        _commande = DataGridExpe.Items[0] as Commande;
                        ActualiserRecapExpe(_commande.NumCommande);
                        RecapExpe_WrapPanel.Visibility = Visibility.Visible;

                        ActualiserDataGridArticle(_commande.NumCommande);
                        DatagGridArticle.Visibility = Visibility.Visible;

                        if (_commande.Site == "Cashphoto")
                        {
                            Coliposte.IsChecked = true;
                            AmazonPageBouton.IsEnabled = false;
                            GLS.IsEnabled = false;
                        }
                        else
                        {
                            AmazonPageBouton.IsEnabled = true;
                            GLS.IsEnabled = true;
                        }
                    }
                    else
                    {
                        RecapExpe_WrapPanel.Visibility = Visibility.Hidden;
                        DatagGridArticle.Visibility = Visibility.Hidden;
                    }
                }
            }         
        }


        #endregion

        #region Expédition

        private void RechercheCommandeAmazon_Click(object sender, RoutedEventArgs e)
        {
            if (_commande.Site != null && _commande.Site.Equals("Amazon"))
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://sellercentral-europe.amazon.com/orders-v3/order/" + _commande.NumCommande) { CreateNoWindow = true });

        }

        private void CommandeExpedier_Checked(object sender, RoutedEventArgs e)
        {
            Actualiser_DataGridExpe(true);
        }

        private void CommandeExpedier_Unchecked(object sender, RoutedEventArgs e)
        {
            Actualiser_DataGridExpe(false);
        }

        private void ModifierPoids_Click(object sender, RoutedEventArgs e)
        {
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if(constante.BDDOK)
            {
                if (_commande != null)
                {
                    Poids poidsDialog = new Poids(this);
                    if (poidsDialog.ShowDialog() == true)
                    {
                        Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                        commande.Poids = double.Parse(poidsDialog.InputTextBox.Text, CultureInfo.InvariantCulture);
                        commande.Preparer = true;
                        constante.cashphotoBDD.SaveChanges();

                        System.Diagnostics.Debug.WriteLine("aa " + _commande.NumCommande);
                        System.Diagnostics.Debug.WriteLine("aa " + _commande.Poids);

                        //_commande = commande;

                        System.Diagnostics.Debug.WriteLine("bb " + _commande.NumCommande);
                        System.Diagnostics.Debug.WriteLine("bb " + _commande.Poids);

                        Actualiser_2DataGrids(false);

                        DisplayTempMessage(Message, "Modification du poids validée.");


                    }
                }
            }
        }

        private async Task Expedition(Commande commande)
        {
            Expedition expedition = new Expedition(this);
            Constante constante = Constante.GetConstante();

            string[] tab;
            double hash = 0; //Si le fichier n'existe pas ou est vide, on met son hash à 0.
            string filename, path;

            //Calcul hash du fichier contenant les numéros de suivis forunis par Coliship
            if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
            {
                //On récupère le hash du fichier pour savoir s'il est modifié ou non
                tab = Directory.GetFiles(constante.numeroSuiviColiposte);
                if (tab.Length > 0)
                {
                    //Le ficher existe, on récupère son hash
                    filename = System.IO.Path.GetFileName(tab[0]);
                    path = constante.numeroSuiviColiposte + "//" + filename;
                    hash = new FileInfo(path).Length;
                    System.Diagnostics.Debug.WriteLine("H " + hash);
                }
            }

            //On boucle si la commande est expédié avec plusieurs colis.
            //On imprime autant d'étiquette (avec un numéros de suivi et un poids différent) que de colis.
            for (int i = 1; i <= commande.NbColis; i++)
            {
                expedition.ExpedierCommande(commande, i);

                if (commande.NbColis == 1)
                    continue;

                if(i != commande.NbColis)
                {
                    var taskDelay = Task.Delay(8000);
                    await taskDelay;
                }               
            }

            if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                _timer = new TimerSuiviFile(this, hash, commande);
            else
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://sellercentral-europe.amazon.com/orders-v3/order/" + commande.NumCommande + "/confirm-shipment") { CreateNoWindow = true });


            Actualiser_2DataGrids(false);
        }

        private void ExpeditionRedondance()
        {
            Constante constante = Constante.GetConstante();
            TestConnexionBDD();
            if (constante.BDDOK)
            {
                if (_commande != null)
                {
                    string export = "";

                    Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == _commande.NumCommande).First();

                    if (commande.Expedier)
                    {
                        ConfirmationExpedition confirmationExpedition = new ConfirmationExpedition();
                        if (confirmationExpedition.ShowDialog() == true)
                        {
                            Expedition(commande);
                            export = "La commande de " + commande.NomClientLivraison + " est expédiée.";
                        }
                    }
                        

                    else
                    {
                        export = "La commande de " + commande.NomClientLivraison + " est expédiée.";
                        if (isCompleteCommande(commande) && commande.Preparer)
                        {

                            commande.Expedier = true;

                            constante.cashphotoBDD.SaveChanges();

                            Expedition(commande);
                        }
                        else if (isCompleteCommande(commande) && commande.Preparer == false)
                        {
                            ConfirmationPoids confirmationPoids = new ConfirmationPoids(this);
                            if (confirmationPoids.ShowDialog() == true)
                            {
                                commande.Expedier = true;
                                commande.Poids = double.Parse(confirmationPoids.InputTextBox.Text, CultureInfo.InvariantCulture);

                                constante.cashphotoBDD.SaveChanges();

                                Expedition(commande);
                            }
                        }
                        else
                            export = "La commande ne peut pas être expédiée.";
                    }
                    DisplayTempMessage(Message, export);
                }
            }
        }

        private void ExpedierCommande_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(DataGridExpe.Items.Count == 1)
                {
                    ExpeditionRedondance();
                }
            }
        }

        private void ExpedierCommande_Click(object sender, RoutedEventArgs e)
        {
            ExpeditionRedondance();
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
            TestConnexionBDD();
            if(constante.BDDOK)
            {
                Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();

                NumCommandeRecapExpe.Content = commande.NumCommande;
                PoidsCommandeRecapExpe.Content = commande.Poids + " Kg";
                DateCommandeRecapExpe.Content = commande.Date.ToShortDateString();
                NomClientRecapExpe.Content = "Nom : "+commande.NomClientLivraison;
                TelClientRecapExpe.Content = commande.TelClientLivraison;
                AdresseClientRecapExpe.Content = commande.Adresse1;
                AdresseClient2RecapExpe.Content = commande.Adresse2;
                AdresseClient3RecapExpe.Content = commande.Adresse3;
                CodePostaleVilleRecapExpe.Content = commande.CodePostal + " - " + commande.Ville;
                PaysRecapExpe.Content = commande.Pays;
                EmailClientRecapExpe.Content = commande.Mail;
            }
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

                Actualiser_2DataGrids(false);

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
                if (PreparationTabItem.IsSelected)
                {
                    _tabItem = 0;     
                    _focusedTextBox = SaisirCommande;

                }
                else if (ExpeditionTabItem.IsSelected)
                {
                    _tabItem = 1;
                }
                else if (ConfigurationTabItem.IsSelected)
                {
                    ConfigurationDialog configurationDialog = new ConfigurationDialog();
                    if (configurationDialog.ShowDialog() == true)
                    {
                        _tabItem = 2;
                    }

                    else
                    {
                        //Important sinon la boite de dialogue prend le focus et perturbe le TabControl
                        //Cela entraine un bug qui déclenche 2 fois la boite de dialogue
                        TabControl.Focus();
                        TabControl.SelectedIndex = _tabItem;
                    }
                }
            }
        }
    }
}
