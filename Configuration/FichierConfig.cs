using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.Configuration
{
    public class FichierConfig
    {
        private static FichierConfig _instance = null;
        private FichierConfig() { }

        private string _chemin = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();

        public static FichierConfig GetInstance()
        {
            if( _instance == null )
                _instance = new FichierConfig();
            return _instance;
        }


        public void sauvegarder()
        {
            string chemin = _chemin + "\\Config.txt";


            //On écrase la sauvegarde précédente
            File.Create(chemin).Close();
            StreamWriter streamWriter = new StreamWriter(chemin);

            //Singleton contenant les constantes dans le code source
            Constante constante = Constante.GetConstante();
            constante.cheminFichierConfig = chemin;

            //On écrit les dossiers d'import
            streamWriter.WriteLine(constante.commandeAmazon);
            streamWriter.WriteLine(constante.commandeCashphoto);
            streamWriter.WriteLine(constante.numeroSuiviColiposte);

            //On écrit les fichiers d'export 
            //Les numéros de suivi
            streamWriter.WriteLine(constante.numeroSuiviAmazon);
            streamWriter.WriteLine(constante.numeroSuiviCashphoto);

            //Les fichiers parsés correctement pour les applis GLS et Coliship
            streamWriter.WriteLine(constante.commandeParsePourGLS);
            streamWriter.WriteLine(constante.commandeParsePourColiposte);

            //Les backup
            streamWriter.WriteLine(constante.backupCommandeAmazon);
            streamWriter.WriteLine(constante.backupCommandeCashphoto);
            streamWriter.WriteLine(constante.backupNumeroSuiviAmazon);
            streamWriter.WriteLine(constante.backupNumeroSuiviCashphoto);
            streamWriter.WriteLine(constante.backupCommandeTransporteurGLS);
            streamWriter.WriteLine(constante.backupCommandeTransporteurColiposte);

            //On ecrit les paramètres globaux
            streamWriter.WriteLine(constante.email);
            streamWriter.WriteLine(constante.telephone);
            streamWriter.WriteLine(constante.BDDIP);
            streamWriter.WriteLine(constante.mode.ToString());
            streamWriter.WriteLine(constante.transporteur.ToString());

            streamWriter.WriteLine(constante.indexTabItem.ToString());
            streamWriter.WriteLine(constante.connectionString2);
            streamWriter.WriteLine(constante.connectionStringLocal);
            streamWriter.WriteLine(constante.regexCommandeAmazon);
            streamWriter.WriteLine(constante.regexCommandeCashphoto);

            streamWriter.Close();
        }


        public void charger()
        {
            charger(_chemin+"\\Config.txt");
        }

        public void charger(string chemin)
        {

            //Singleton contenant les constantes dans le code source
            Constante constante = Constante.GetConstante();
            constante.cheminFichierConfig = chemin;

            //Idem pour Transporteur et ModeSuiviColiposte
            Transporteur transporteur = Transporteur.GetInstance();
            ModeSuiviColiposte modeSuiviColiposte = ModeSuiviColiposte.GetInstance();

            try
            {
                StreamReader reader = new StreamReader(chemin);

                //On lit les fichiers d'import
                constante.commandeAmazon = reader.ReadLine();
                constante.commandeCashphoto = reader.ReadLine();
                constante.numeroSuiviColiposte = reader.ReadLine();

                //On lit les fichiers d'export
                //Les numéros de suivi
                constante.numeroSuiviAmazon = reader.ReadLine();
                constante.numeroSuiviCashphoto = reader.ReadLine();

                //Les fichiers parsés correctement pour les applis GLS et Coliship
                constante.commandeParsePourGLS = reader.ReadLine();
                constante.commandeParsePourColiposte = reader.ReadLine();
                

                //Fichiers de backup
                constante.backupCommandeAmazon = reader.ReadLine();
                constante.backupCommandeCashphoto = reader.ReadLine();
                constante.backupNumeroSuiviAmazon = reader.ReadLine();
                constante.backupNumeroSuiviCashphoto = reader.ReadLine();
                constante.backupCommandeTransporteurGLS = reader.ReadLine();
                constante.backupCommandeTransporteurColiposte = reader.ReadLine();


                //On lit les paramètres globaux
                constante.email = reader.ReadLine();
                constante.telephone = reader.ReadLine();
                constante.BDDIP = reader.ReadLine();

                Enum.TryParse(reader.ReadLine(), out ModeSuiviColiposte.ModeSuiviColiposte_LST mode); //On convertit la string en type Enum
                modeSuiviColiposte.setMode(mode);
                constante.mode = modeSuiviColiposte.getMode();

                Enum.TryParse(reader.ReadLine(), out Transporteur.Transporteurs transporteurs); //On convertit la string en type Enum
                transporteur.setTransporteur(transporteurs);
                constante.transporteur = transporteur.getTransporteur();

                int.TryParse(reader.ReadLine(), out int res);
                constante.indexTabItem = res;

                constante.connectionString2 = reader.ReadLine();
                constante.connectionStringLocal = reader.ReadLine();
                constante.regexCommandeAmazon = reader.ReadLine();
                constante.regexCommandeCashphoto = reader.ReadLine();

                constante.fichierConfigExist = true;
                reader.Close();
            }
            catch(Exception e)
            {
                constante.fichierConfigExist=false;
                //Ouverture d'une boite de dialogue pour indiquer le problème
                System.Windows.MessageBox.Show("Erreur lors du chargement du fichier de configuration.\n", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

    }
}
