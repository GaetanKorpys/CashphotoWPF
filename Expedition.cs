﻿using CashPhoto;
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
           
            if(commande.Site == "Cashphoto")
                ExpedierColiposte(commande, NbColis);
            else if(commande.Site == "Amazon")
            {
                if (constante.transporteur == Transporteur.Transporteurs.Coliposte)
                    ExpedierColiposte(commande, NbColis);
                else
                    ExpedierGLS(commande, NbColis);
            }        
        }

        private void ExpedierColiposte(Commande commande, int NbColis)
        {
            Constante constante = Constante.GetConstante();
            string separateur = ";";
            string line = "";
            string linePoids = "";
            string nom = commande.NomClientLivraison;
            string prenom = "";
            string row = "";
            string[] fileDataField;

            PhoneNumber pn = new PhoneNumber(commande);

            switch (NbColis)
            {
                case 1:
                    linePoids = (commande.Poids * 1000).ToString();
                    break;
                case 2:
                    linePoids = (commande.Poids2 * 1000).ToString();
                    break;
                case 3:
                    linePoids = (commande.Poids3 * 1000).ToString();
                    break;
                case 4:
                    linePoids = (commande.Poids4 * 1000).ToString();
                    break;
                default:
                    break;
            }

            string path = constante.backupCommandeCashphoto + "\\" + commande.NumCommande + ".csv";
            string CodeRetrait = "";
            System.Diagnostics.Debug.WriteLine("gggggg " + path);
            if (File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine("dddd ");
                StreamReader streamReader = new StreamReader(path, System.Text.Encoding.GetEncoding(1252));
                if (streamReader.Peek() != -1)
                {
                    row = streamReader.ReadLine();
                    row = row.Replace("\"", "");
                    fileDataField = row.Split(";");
                    CodeRetrait = fileDataField[21];
                    System.Diagnostics.Debug.WriteLine("gggggg " + CodeRetrait);
                }
                streamReader.Close();
            }


            if (commande.Site == "Cashphoto")
            {
                //nom = "";
                //string[] tab = commande.NomClientLivraison.Split(" ");
                //foreach (string s in tab)
                //{
                //    if (_app.IsAllUpper(s))
                //        nom = nom + s + " ";
                //    else
                //        prenom = prenom + s + " ";
                //}

                System.Diagnostics.Debug.WriteLine(row);
                fileDataField = row.Split(";");
                fileDataField[1] = commande.NumCommande;
                fileDataField[2] = fileDataField[2].ToUpper();
                fileDataField[9] = linePoids;
                fileDataField[12] = pn.getFixe();
                fileDataField[13] = commande.Mail;
                fileDataField[18] = pn.getMobile();
                fileDataField[26] = commande.NumCommande;


                foreach (string s in fileDataField) line += s + ";";

            }
            else
            {
                line += separateur + commande.NumCommande + separateur + nom + separateur;
                line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
                line += commande.CodePostal + separateur + commande.Ville + separateur + commande.Pays + separateur + linePoids + separateur;
                line += "0" + separateur + "N" + separateur + pn.getFixe() + separateur + commande.Mail + separateur + separateur + separateur + prenom + separateur + separateur;
                line += pn.getMobile() + separateur + separateur + separateur + CodeRetrait + separateur + "Cashphoto.com" + separateur + separateur + separateur + "1" + separateur + commande.NumCommande;
            }

            System.Diagnostics.Debug.WriteLine(line);
            ExportCSV(line, constante.commandeParsePourColiposte, commande, Transporteur.Transporteurs.Coliposte, NbColis);
            ExportCSV(line, constante.backupCommandeTransporteurColiposte, commande, Transporteur.Transporteurs.Coliposte, NbColis);

        }

        private void ExpedierGLS(Commande commande, int NbColis)
        {
            Constante constante = Constante.GetConstante();
            string separateur = ";";
            string line;
            string linePoids = "";

            PhoneNumber pn = new PhoneNumber(commande);

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
            line += commande.NumCommande + separateur + commande.NomClientLivraison + separateur + pn.getFixe() + separateur;
            line += pn.getMobile() + separateur + separateur + commande.Mail + separateur;
            line += commande.Adresse1 + separateur + commande.Adresse2 + separateur + commande.Adresse3 + separateur;
            line += commande.Pays + separateur + commande.CodePostal + separateur + commande.Ville + separateur + separateur + separateur + separateur;
            line += "SODEX FAGOT-THIL SAS" + separateur + "13 rue De Gaulle" + separateur + separateur + "FR" + separateur + "57290" + separateur + "Seremange Erzange" + separateur + separateur;
            
            ExportCSV(line, constante.commandeParsePourGLS, commande, Transporteur.Transporteurs.GLS, NbColis);
            ExportCSV(line, constante.backupCommandeTransporteurGLS, commande, Transporteur.Transporteurs.GLS, NbColis);

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

            line.Replace("/", "");
            line.Replace("\r", "");
            line.Replace("\n", "");

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

