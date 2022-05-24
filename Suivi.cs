using CashphotoWPF.BDD;
using CashphotoWPF.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF
{
    internal class Suivi
    {
        private MainWindow _app;
        public Suivi(MainWindow app)
        {
            _app = app;
        }

        private List<string> getSuiviFilesFromColiposte(string folder)
        {
            List<string> FilesList = new List<string>();

            foreach (string data in Directory.EnumerateFiles(folder))
            {
                if (data.Contains(".csv"))
                    FilesList.Add(data);
            }

            return FilesList;
        }

        public void createSuiviFromCommande(Commande commande)
        {
            Constante constante = Constante.GetConstante();
            string delimiter = ";";
            string fileRow;
            string[] fileDataField;
          
            List<string> FilesList = getSuiviFilesFromColiposte(constante.numeroSuiviColiposte);

            foreach (string data in FilesList)
            {
                if (File.Exists(data))
                {
                    StreamReader fileReader = new StreamReader(data);

                    if (fileReader.Peek() != -1)
                    {
                        fileRow = fileReader.ReadLine();
                    }

                    while (fileReader.Peek() != -1)
                    {
                        fileRow = fileReader.ReadLine();
                        fileDataField = fileRow.Split(delimiter);

                        if(!fileDataField[0].Contains("-"))
                        {
                            fileDataField[0] = fileDataField[0].Substring(4);;
                        }

                        if (commande.NumCommande == fileDataField[0])
                        {
                            if (commande.Site == "Amazon")
                                createSuiviForAmazon(commande, fileDataField);
                            else if (commande.Site == "Cashphoto")
                                createSuiviForCashphoto(commande, fileDataField);
                        }
                    }
                    fileReader.Close();
                }
            }
        }

        private void createSuiviForAmazon(Commande c, string[] line)
        {
            Constante constante = Constante.GetConstante();

            string delimiter = "\t";
            string date = DateTime.Today.ToShortDateString().Replace("/", "_");
            string filename = "RetourNSuiviAmazon" + "_" + date + ".txt";
            string path = constante.numeroSuiviAmazon + "\\" + filename;
            string entete, row, lastFile;
            string[] tab;


            Commande cmd = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == c.NumCommande).First();
            cmd.NumeroSuivi = line[1];

            constante.cashphotoBDD.SaveChanges();

            entete = "order-id" + delimiter;
            entete += "order-item-id" + delimiter;
            entete += "quantity" + delimiter;
            entete += "ship-date" + delimiter;
            entete += "carrier-code" + delimiter;
            entete += "carrier-name" + delimiter;
            entete += "tracking-number" + delimiter;
            entete += "ship-method" + delimiter;
            entete += "transparency_code" + delimiter;
            entete += "ship_from_address_name" + delimiter;
            entete += "ship_from_address_line1" + delimiter;
            entete += "ship_from_address_line2" + delimiter;
            entete += "ship_from_address_line3" + delimiter;
            entete += "ship_from_address_city" + delimiter;
            entete += "ship_from_address_county" + delimiter;
            entete += "ship_from_address_state_or_region" + delimiter;
            entete += "ship_from_address_postalcode" + delimiter;
            entete += "ship_from_address_countrycode";

            tab = Directory.GetFiles(constante.numeroSuiviAmazon);
            if(tab.Length > 0)
            {
                lastFile = tab[0];

                if (!path.Equals(lastFile))                                                                                                   
                    File.Delete(tab[0]);
            }

            StreamWriter sw = File.AppendText(path);


            if (new FileInfo(path).Length == 0)
            {
                sw.WriteLine(entete);
            }
                
            
            List<Article> commandes = new List<Article>();
           
            IQueryable<Article> articlesTable;

            articlesTable = constante.cashphotoBDD.Articles.Where(article => article.NumCommande == c.NumCommande);
            commandes = articlesTable.ToList();

            ModeSuiviColiposte modeSuiviColiposte = ModeSuiviColiposte.GetInstance();

            foreach(Article article in commandes)
            {
                //Cf doc "Flat.File.ShippingConfirm.fr.xls" fournie par amazon (lst champs obligatoires)
                row = c.NumCommande + delimiter;                                             //order-id
                row += article.IdAmazon + delimiter;                                          //order-item-id
                row += article.Quantite + delimiter;                                          //quantity
                row += DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") + delimiter;           //ship-date
                row += "Colissimo" + delimiter;                                               //carrier-code
                row += delimiter;                                                             //carrier-name
                row += cmd.NumeroSuivi + delimiter;                                           //tracking-number
                row += modeSuiviColiposte.getString(constante.mode) + delimiter;              //ship-method
                row += delimiter;                                                             //transparency_code
                row += "SODEX FAGOT-THIL SAS" + delimiter;                                    //ship_from_address_name
                row += "57290 rue Charles de Gaulle" + delimiter;                             //ship_from_address_line1
                row += delimiter;                                                             //ship_from_address_line2
                row += delimiter;                                                             //ship_from_address_line3
                row += delimiter;                                                             //ship_from_address_city
                row += delimiter;                                                             //ship_from_address_county
                row += delimiter;                                                             //ship_from_address_state_or_region
                row += delimiter;                                                             //ship_from_address_postalcode
                row += "FR";

                sw.WriteLine(row);
            }

            sw.Close();

        }

        private void createSuiviForCashphoto(Commande c, string[] line)
        {
            Constante constante = Constante.GetConstante();

            string date = DateTime.Today.ToShortDateString().Replace("/", "_");
            string filename = "RetourNSuiviPrestashop" + "_" + date + ".csv";
            string path = constante.numeroSuiviCashphoto + "\\" + filename;
            string entete = "ReferenceExpedition;NumeroColis";
            string delimiter = ";";
            string row, lastFile;
            string[] tab;


            Commande cmd = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == c.NumCommande).First();
            cmd.NumeroSuivi = line[1];

            constante.cashphotoBDD.SaveChanges();

            tab = Directory.GetFiles(constante.numeroSuiviCashphoto);
            if (tab.Length > 0)
            {
                lastFile = tab[0];

                if (!path.Equals(lastFile))
                    File.Delete(tab[0]);
            }

            StreamWriter sw = File.AppendText(path);


            if (new FileInfo(path).Length == 0)
            {
                sw.WriteLine(entete);
            }
            

            row = "EXPP"+c.NumCommande;
            row += delimiter;
            row += line[1];

            sw.WriteLine(row);

            sw.Close();
        }
    }
}
