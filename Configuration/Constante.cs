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
        public string commandeAmazon { get; set; }
        public string commandeCashphoto { get; set; }
        public string numeroSuiviColiposte { get; set; }

        //Dossier d'export

        public string numeroSuiviAmazon { get; set; }
        public string numeroSuiviCashphoto { get; set; }
        public string backupCommandeAmazon { get; set; } //Copie du fichier .txt qui contient les commandes importées via Amazon, un fichier = pls commandes
        public string backupCommandeCashphoto { get; set; } //Copie des fichiers .csv, un fichier = une commande
        public string backupNumeroSuiviAmazon { get; set; } //Copie du fichier .txt qui contient les numéros de suivie des commandes passées sur Amazon (GLS ou Coliposte)
        public string backupNumeroSuiviCashphoto { get; set; } //Copie du fichier qui contient les numéros de suivie des commandes passées sur Cashphoto (uniquement Coliposte)
        public string backupCommandeTransporteurGLS { get; set; } //Copie du fichier envoyé à l'appli GLS pour les commandes (uniquement Amazon)
        public string backupCommandeTransporteurColiposte { get; set; } //Copie du fichier envoyé à l'appli Coliship (Coliposte) pour les commandes Amzon et Cashphoto

        //Paramètres globaux

        public string email { get; set; } //Email par défaut
        public string telephone { get; set; } //Téléphone par défaut
        public bool BDDOK { get; set; } //Permet de controler l'état de la BDD
        public string BDDIP  { get; set; }
        public ModeSuiviColiposte mode { get; set; }
        public Transporteur transporteur { get; set; }
    }
}
