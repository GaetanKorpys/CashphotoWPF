using CashphotoWPF.BDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.Configuration
{
    public class Constante
    {
        private static Constante _instance = null;
        private Constante() { }

        public static Constante GetConstante()
        {
            if(_instance == null)
            {
                _instance = new Constante();
            }
            return _instance;
        }

        //Dosser d'import
        public string commandeAmazon { get; set; } //Fichier .txt qui contient les commandes d'Amazon
        public string commandeCashphoto { get; set; }//Pls fichiers .csv avec 1 fichier = 1 commande pour Cashphoto
        public string numeroSuiviColiposte { get; set; }//Numéros de suivi obtenu avec l'appli Coliship (Coliposte)

        //Dossier d'export

        //Les numéros de suivi
        public string numeroSuiviAmazon { get; set; }//Numéros de suivi pour faire la maj des commandes sur Amazon
        public string numeroSuiviCashphoto { get; set; }//Numéros de suivi pour faire la maj des commandes sur Prestashop (Cashphoto)

        //Les commandes parsées pour GLS et Coliship (Coliposte)
        public string commandeParsePourGLS { get; set; }//Ficher pour GLS
        public string commandeParsePourColiposte { get; set; }//Fichier pour Coliship (Coliposte)

        //Les backups (tout sauf le fichier de numéro de suivi fourni par Coliship)
        public string backupCommandeAmazon { get; set; } //Copie du fichier .txt qui contient les commandes importées via Amazon, un fichier = pls commandes
        public string backupCommandeCashphoto { get; set; } //Copie des fichiers .csv, un fichier = une commande
        public string backupNumeroSuiviAmazon { get; set; } //Copie du fichier .txt qui contient les numéros de suivie des commandes passées sur Amazon (GLS ou Coliposte)
        public string backupNumeroSuiviCashphoto { get; set; } //Copie du fichier qui contient les numéros de suivie des commandes passées sur Cashphoto (uniquement Coliposte)
        public string backupCommandeTransporteurGLS { get; set; } //Copie du fichier envoyé à l'appli GLS pour les commandes (uniquement Amazon)
        public string backupCommandeTransporteurColiposte { get; set; } //Copie du fichier envoyé à l'appli Coliship (Coliposte) pour les commandes Amzon et Cashphoto

        //Paramètres globaux

        public string email { get; set; } //Email par défaut
        public string telephone { get; set; } //Téléphone par défaut
        public string BDDIP { get; set; }
        public ModeSuiviColiposte.ModeSuiviColiposte_LST mode { get; set; }
        public Transporteur.Transporteurs transporteur { get; set; }
        public int indexTabItem { get; set; }
        public string connectionString2 { get; set; } //2eme partie de la chaine de connexion, contient l'id et le mdp
        public string connectionStringLocal { get; set; } //Chaine de connexion pour BDD Locale
        public string regexCommandeAmazon { get; set; } //Pattern des numéros de commandes venant d'Amazon | Ex: 408-0983123-2430735
        public string regexCommandeCashphoto { get; set; } //Pattern des numéros de commandes venant de Cashphoto (Prestashop) | Ex: 56656

        //Paramètres non chargé dans le fichier de config
        public bool fichierConfigExist { get; set; }
        public bool BDDOK { get; set; } //Permet de controler l'état de la BDD
        public string cheminFichierConfig { get; set; }
        public CashphotoBDD cashphotoBDD { get; set; }
        public bool commandeExpedie { get; set; }
        public bool commandePrepare { get; set; }
    }
}
