﻿using CashphotoWPF.BDD;
using CashphotoWPF.Configuration;
using CashphotoWPF.COR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF
{
    internal class Importation
    {
        public int ImportCommandes()
        {
            int changements = 0;
            changements += ImportFromAmazon();
            changements += ImportFromCashphoto();
            return changements;
        }

        private int ImportFromAmazon()
        {
            Constante constante = Constante.GetConstante();
            Cor cor = Cor.getInstance();
            

            List<string> FilesList = getAmazonFiles();
            char delimiter = '\t';
            int import = 0;

            foreach (string data in FilesList)
            {
                if(File.Exists(data))
                {
                    StreamReader streamReader = new StreamReader(data);
                    //On ignore la 1ère ligne dans le ficher Amazon
                    string line = streamReader.ReadLine();
                    string[] fileDataField;
                    string numCommande;

                    while (streamReader.Peek() != -1)
                    {
                        line = streamReader.ReadLine();
                        fileDataField = line.Split(delimiter);
                        numCommande = fileDataField[0];

                        int exist = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).Count();

                        if (exist == 0)
                        {
                            CreerCommandeAmazon(fileDataField);
                            import ++;
                        }
                            
                        else
                        {
                            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();
                            if(!commande.Completer)
                            {
                                CompleterCommandeAmazon(fileDataField);
                                import ++;
                            }
                                
                        }
                            
                        AddArticleAmazon(fileDataField);
                    }

                    streamReader.Close();
                    File.Delete(data);
                    
                }
            }
            return import;
        }

        private int ImportFromCashphoto()
        {
            Constante constante = Constante.GetConstante();
            List<string> FilesList = getCashphotoFiles();
            char delimiter = ';';
            int import = 0;

            foreach (string data in FilesList)
            {
                if (File.Exists(data))
                {
                    //System.Text.Encoding.GetEncoding(PrestashopEncodage)
                    StreamReader streamReader = new StreamReader(data);
                    if (streamReader.Peek() != -1)
                    {
                        string line = streamReader.ReadLine();
                        line = line.Replace("\"","");
                        string[] fileDataField = line.Split(delimiter);
                        string numCommande = fileDataField[1];

                        int exist = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).Count();
                        if (exist == 0)
                        {
                            CreerCommandeCashphoto(fileDataField);
                            import++;
                        }
                            
                        else
                        {
                            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();
                            if (!commande.Completer)
                            {
                                CompleterCommandeCashphoto(fileDataField);
                                import++;
                            }
                                
                        }

                        line = streamReader.ReadLine();
                        line = line.Replace("\"", "");
                        fileDataField = line.Split(delimiter);
                        AddArticleCashphoto(fileDataField, numCommande);
                    }

                    streamReader.Close();
                    string filename = Path.GetFileName(data);
                       
                    File.Delete(data);
                    
                }
            }
            return import;
        }

        private List<string> getCashphotoFiles()
        {
            Constante constante = Constante.GetConstante();
            List<string> FilesList = new();
            foreach (string data in Directory.EnumerateFiles(constante.commandeCashphoto))
            {
                if (data.Contains(".csv"))
                {
                    string name = Path.GetFileName(data);
                    if (File.Exists(constante.commandeCashphoto + "\\" + name))
                         FilesList.Add(data);
                }
            }
            return FilesList;
        }

        private List<string> getAmazonFiles()
        {
            Constante constante = Constante.GetConstante();
            List<string> FilesList = new();


            foreach (string data in Directory.EnumerateFiles(constante.commandeAmazon))
            {
                if(data.Contains(".txt"))
                {
                    FilesList.Add(data);
                }
            }

            return FilesList;
          
        }

        private void CompleterCommandeAmazon(string[] Data)
        {
            Constante constante = Constante.GetConstante();
            string numCommande = Data[0];

            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();
            commande.NomClientFacturation = Data[5];
            commande.NomClientLivraison = Data[16];
            commande.TelClientFacturation = Data[6];
            commande.TelClientLivraison = Data[24];
            commande.Mail = Data[4];
            commande.Ville = Data[20];
            commande.Pays = Data[23];
            commande.CodePostal = Data[22];
            commande.Adresse1 = Data[17];
            commande.Adresse2 = Data[18];
            commande.Adresse3 = Data[19];
            commande.Site = "Amazon";
            commande.Completer = true;

            constante.cashphotoBDD.SaveChanges();
        }

        private void CreerCommandeAmazon(string[] Data)
        {
            Constante constante = Constante.GetConstante();
            string numCommande = Data[0];


            Commande commande = new Commande();
            commande.NumCommande = Data[0];
            commande.NomClientFacturation = Data[5];
            commande.NomClientLivraison = Data[16];
            commande.TelClientFacturation = Data[6];
            commande.TelClientLivraison = Data[24];
            commande.Mail = Data[4];
            commande.Ville = Data[20];
            commande.Pays = Data[23];
            commande.CodePostal = Data[22];
            commande.Adresse1 = Data[17];
            commande.Adresse2 = Data[18];
            commande.Adresse3 = Data[19];
            commande.Site = "Amazon";
            commande.Date = DateTime.Now;
            commande.Preparer = false;
            commande.Expedier = false;
            commande.Completer = false;
   
            constante.cashphotoBDD.Commandes.Add(commande);
            constante.cashphotoBDD.SaveChanges();

        }

        private void CompleterCommandeCashphoto(string[] Data)
        {
            Constante constante = Constante.GetConstante();
            string numCommande = Data[26].Substring(1);

            Commande commande = constante.cashphotoBDD.Commandes.Where(commande => commande.NumCommande == numCommande).First();
            commande.NomClientLivraison = Data[16] + " " + Data[2];
            commande.TelClientFacturation = Data[12];
            commande.TelClientLivraison = Data[18];
            commande.Mail = Data[13];
            commande.Ville = Data[7];
            commande.Pays = Data[8];
            commande.CodePostal = Data[6];
            commande.Adresse1 = Data[3];
            commande.Adresse2 = Data[4];
            commande.Adresse3 = Data[5];
            commande.Site = "Cashphoto";
            commande.Completer = true;

            constante.cashphotoBDD.SaveChanges();
        }

        private void CreerCommandeCashphoto(string[] Data)
        {
            Constante constante = Constante.GetConstante();
            string numCommande = Data[26].Substring(1);


            Commande commande = new Commande();
            commande.NumCommande = numCommande;
            commande.Poids = double.Parse(Data[9]) / 1000; //Poids approximatif
            commande.NomClientLivraison = Data[16] + " " + Data[2];
            commande.TelClientFacturation = Data[12];
            commande.TelClientLivraison = Data[18];
            commande.Mail = Data[13];
            commande.Ville = Data[7];
            commande.Pays = Data[8];
            commande.CodePostal = Data[6];
            commande.Adresse1 = Data[3];
            commande.Adresse2 = Data[4];
            commande.Adresse3 = Data[5];
            commande.Site = "Cashphoto";
            commande.Date = DateTime.Now;
            commande.Preparer = false;
            commande.Expedier = false;
            commande.Completer = false;

            constante.cashphotoBDD.Commandes.Add(commande);
            constante.cashphotoBDD.SaveChanges();

        }
        private void AddArticleAmazon(string[] Data)
        {
            Constante constante = Constante.GetConstante();

            Article article = new Article();
            System.Diagnostics.Debug.WriteLine(Data[12]);
            article.IdArticle = Data[1];
            article.NumCommande = Data[0];
            article.NomArticle = Data[8];
            article.Prix = double.Parse(Data[11], System.Globalization.CultureInfo.InvariantCulture);
            article.Sku = Data[7];
            article.Taxe = double.Parse(Data[12], System.Globalization.CultureInfo.InvariantCulture);
            article.Quantite = int.Parse(Data[9]);
            
            constante.cashphotoBDD.Add(article);
            constante.cashphotoBDD.SaveChanges();

        }

        private void AddArticleCashphoto(string[] Data, string numCommande)
        {
            Constante constante = Constante.GetConstante();

            Article article = new Article();


            article.NumCommande = numCommande;
            article.NomArticle = Data[1];
            article.Prix = double.Parse(Data[4]);
            article.Quantite = int.Parse(Data[2]);

            constante.cashphotoBDD.Add(article);
            constante.cashphotoBDD.SaveChanges();

        }
    }
}