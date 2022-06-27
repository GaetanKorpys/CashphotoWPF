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
        
        public void createSuiviFromCommande(Commande commande)
        {
            Constante constante = Constante.GetConstante();
            string delimiter = ";";
            string fileRow;
            string[] fileDataField;
            string[] tab;
          
            string pathNumSuiviColipost = _app.getSuiviFileFromColiposte();
            System.Diagnostics.Debug.WriteLine("oPPP "+ pathNumSuiviColipost);
            if (File.Exists(pathNumSuiviColipost))
            {
                StreamReader fileReader = new StreamReader(pathNumSuiviColipost);
                System.Diagnostics.Debug.WriteLine("oPPdd"); 

                while (fileReader.Peek() != -1)
                {
                    fileRow = fileReader.ReadLine();
                    fileDataField = fileRow.Split(delimiter);

                    //if(!fileDataField[0].Contains("-"))
                    //{
                    //    fileDataField[0] = fileDataField[0].Substring(4);;
                    //    System.Diagnostics.Debug.WriteLine("L "+ fileDataField[0]);
                    //}

                    System.Diagnostics.Debug.WriteLine("ok");

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

            if (cmd.NumeroSuivi == null)
                cmd.NumeroSuivi = line[1];
            else if (cmd.NumeroSuivi2 == null)
                cmd.NumeroSuivi2 = line[1];
            else if (cmd.NumeroSuivi3 == null)
                cmd.NumeroSuivi3 = line[1];
            else if (cmd.NumeroSuivi4 == null)
                cmd.NumeroSuivi4 = line[1];

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

            //On supprime manuellement l'ancien fichier
            foreach(string data in tab)
            {
                if (!path.Equals(data))
                    File.Delete(data);
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
                row += cmd.NumeroSuivi + delimiter;                                           //tracking-number | A modifier pour pouvoir gérer pls colis
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
                _app.putInBackup(row, constante.backupNumeroSuiviAmazon, filename);
            }

            sw.Close();

            

        }

        private void createSuiviForCashphoto(Commande c, string[] line)
        {
            Constante constante = Constante.GetConstante();

            string date = DateTime.Today.ToShortDateString().Replace("/", "_");
           
            string entete = "ReferenceExpedition;NumeroColis";
            string delimiter = ";";
            string row, lastFile;
            string[] tab;


            Commande cmd = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == c.NumCommande).First();

            string filename = cmd.NumCommande + ".csv";
            string path = constante.numeroSuiviCashphoto + "\\" + filename;

            if (cmd.NumeroSuivi == null)
                cmd.NumeroSuivi = line[1];
            else if(cmd.NumeroSuivi2 == null)
                cmd.NumeroSuivi2 = line[1];
            else if(cmd.NumeroSuivi3 == null)
                cmd.NumeroSuivi3 = line[1];
            else if(cmd.NumeroSuivi4 == null)
                cmd.NumeroSuivi4 = line[1];
                

            constante.cashphotoBDD.SaveChanges();

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

            //L'ancien fichier est supprimé par Prestashop
            //On s'occupe juste de backup le fichier à chaque modification, on écrase le fichier
            _app.putInBackup(row, constante.backupNumeroSuiviCashphoto, filename);
        }
    }
}
