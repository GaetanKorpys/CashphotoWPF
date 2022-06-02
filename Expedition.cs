using CashphotoWPF.BDD;
using CashphotoWPF.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CashphotoWPF
{
    internal class Expedition
    {
        private MainWindow _app;
        public Expedition(MainWindow app)
        {
            _app = app;
        }

        public void ExpedierCommande(Commande commande, int NbColis)
        {
            Constante constante = Constante.GetConstante();
           
            if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                ExpedierColiposte(commande, NbColis);
            else
                ExpedierGLS(commande, NbColis);
            
        }

        private void ExpedierColiposte(Commande commande, int NbColis)
        {
            Constante constante = Constante.GetConstante();
            string separateur = ";";
            string line;
            string linePoids = "";

            switch (NbColis)
            {
                case 1:
                    linePoids = (commande.Poids*1000).ToString();
                    break;
                case 2:
                    linePoids = (commande.Poids2*1000).ToString();
                    break;
                case 3:
                    linePoids = (commande.Poids3*1000).ToString();
                    break;
                case 4:
                    linePoids = (commande.Poids4*1000).ToString();
                    break;
                default:
                    break;
            }


            line = separateur + commande.NumCommande + separateur + commande.NomClientLivraison + separateur;
            line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
            line += commande.CodePostal + separateur + commande.Ville + separateur + commande.Pays + separateur + linePoids + separateur;
            line += "0" + separateur + "N" + separateur + commande.TelClientLivraison + separateur + commande.Mail + separateur + separateur + separateur + separateur + separateur;
            line += commande.TelClientLivraison + separateur + separateur + separateur + separateur + "Cashphoto.com" + separateur + separateur + separateur + "1" + separateur + commande.NumCommande;
            //line.Replace("/", "\\");
            //line.Replace("\r", "");
            //line.Replace("\n", "");


            ExportCSV(line, constante.commandeParsePourColiposte, commande, Transporteur.Transporteurs.Coliposte, NbColis);

            //if(commande.Poids2 != null)
            //{
            //    line = separateur + commande.NumCommande + separateur + commande.NomClientLivraison + separateur;
            //    line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
            //    line += commande.CodePostal + separateur + commande.Ville + separateur + commande.Pays + separateur + (commande.Poids2 * 1000).ToString() + separateur; //Poids2
            //    line += "0" + separateur + "N" + separateur + commande.TelClientLivraison + separateur + commande.Mail + separateur + separateur + separateur + separateur + separateur;
            //    line += commande.TelClientLivraison + separateur + separateur + separateur + separateur + "Cashphoto.com" + separateur + separateur + separateur + "1" + separateur + commande.NumCommande;

            //    ExportCSV(line, constante.commandeParsePourColiposte, commande, Transporteur.Transporteurs.Coliposte, true);
            //}
            
        }

        private void ExpedierGLS(Commande commande, int NbColis)
        {
            Constante constante = Constante.GetConstante();
            string separateur = ";";
            string line;
            string linePoids = "";

            switch (NbColis)
            {
                case 1:
                    linePoids = commande.Poids.ToString();
                    break;
                case 2:
                    linePoids = commande.Poids2.ToString();
                    break;
                case 3:
                    linePoids = commande.Poids3.ToString();
                    break;
                case 4:
                    linePoids = commande.Poids4.ToString();
                    break;
                default:
                    break;
            }

            line = commande.NumCommande + separateur + separateur + separateur;
            line += commande.NomClientLivraison + separateur + "2502657001" + separateur + "18" + separateur + separateur + "1" + separateur;
            line += linePoids + separateur + separateur + separateur;
            line += commande.NumCommande + separateur + commande.NomClientLivraison + separateur + separateur; //FixeNumTel
            line += commande.TelClientLivraison + separateur + separateur + commande.Mail + separateur;
            line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
            line += commande.Pays + separateur + commande.CodePostal + separateur + commande.Ville + separateur + separateur + separateur + separateur;
            line += "SODEX FAGOT-THIL SAS" + separateur + "13 rue De Gaulle" + separateur + separateur + "FR" + separateur + "57290" + separateur + "Seremange Erzange" + separateur + separateur;
            //line.Replace("/", "\\");
            //line.Replace("\r", "");
            //line.Replace("\n", "");
            
            ExportCSV(line, constante.commandeParsePourGLS, commande, Transporteur.Transporteurs.GLS, NbColis);

            //if(commande.Poids2 != null)
            //{
            //    line = commande.NumCommande + separateur + separateur + separateur;
            //    line += commande.NomClientLivraison + separateur + "2502657001" + separateur + "18" + separateur + separateur + "1" + separateur;
            //    line += commande.Poids2.ToString() + separateur + separateur + separateur; //Poids2
            //    line += commande.NumCommande + separateur + commande.NomClientLivraison + separateur + separateur; //FixeNumTel
            //    line += commande.TelClientLivraison + separateur + separateur + commande.Mail + separateur;
            //    line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
            //    line += commande.Pays + separateur + commande.CodePostal + separateur + commande.Ville + separateur + separateur + separateur + separateur;
            //    line += "SODEX FAGOT-THIL SAS" + separateur + "13 rue De Gaulle" + separateur + separateur + "FR" + separateur + "57290" + separateur + "Seremange Erzange" + separateur + separateur;

            //    ExportCSV(line, constante.commandeParsePourGLS, commande, Transporteur.Transporteurs.GLS, true);
            //}
            
        }

        private void ExportCSV(string line, string path, Commande commande, Transporteur.Transporteurs transporteurs, int NbColis)
        {
            string completepath = path + "\\" + commande.NumCommande + "_" + commande.NomClientLivraison + "_" + NbColis + ".csv";
          
            
            Encoding enc;
            if (transporteurs.Equals(Transporteur.Transporteurs.GLS))
            {
                line = RemoveDiacritics(line); //Le logiciel GLS est faible face aux caractères spéciaux
                enc = Encoding.ASCII;    
            }
            else
            {
                enc = Encoding.UTF8; //Coliposte lit l'UTF8 en revanche
            }
            StreamWriter fileWriter = new StreamWriter(completepath, false, enc);

            fileWriter.WriteLine(line);
            fileWriter.Flush();
            fileWriter.Close();
        }

        private static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            string s = sb.ToString().Normalize(NormalizationForm.FormC);
            Regex.Replace(s, "[/\\\\]", "-"); //remplace les / et \ par -
            Regex.Replace(s, "[\"\']", " "); //remplace les " et ' par un espace
            return s;
        }
    }
}

