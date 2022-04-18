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

            //On écrit les dossiers d'import
            streamWriter.WriteLine(constante.commandeAmazon);
            streamWriter.WriteLine(constante.commandeCashphoto);
            streamWriter.WriteLine(constante.numeroSuiviColiposte);

            //On écrit les fichiers d'export 
            streamWriter.WriteLine(constante.numeroSuiviAmazon);
            streamWriter.WriteLine(constante.numeroSuiviCashphoto);
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

            streamWriter.Close();
        }

        public void charger()
        {
            string chemin = _chemin + "\\Config.txt";

            //Singleton contenant les constantes dans le code source
            Constante constante = Constante.GetConstante();
            try
            {
                StreamReader reader = new StreamReader(chemin);

                //On lit les fichiers d'import
                constante.commandeAmazon = reader.ReadLine();
                constante.commandeCashphoto = reader.ReadLine();
                constante.numeroSuiviColiposte = reader.ReadLine();

                //On lit les fichiers d'export
                constante.numeroSuiviAmazon = reader.ReadLine();
                constante.numeroSuiviCashphoto = reader.ReadLine();
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
                constante.mode = (ModeSuiviColiposte)Enum.Parse(typeof(ModeSuiviColiposte), reader.ReadLine());
                constante.transporteur = (Transporteur)Enum.Parse (typeof(Transporteur), reader.ReadLine());

                reader.Close();
            }
            catch(Exception )
            {
                //Ouverture d'une boite de dialogue pour indiquer le problème
                System.Windows.MessageBox.Show("Erreur pour chargé le fichier de configuration.\n", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

    }
}
