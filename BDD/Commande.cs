﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.BDD
{
    public class Commande
    {
        [Key]
        public string IdCommande { get; set; }
        public double Poids { get; set; }
        public string? NomClientFacturation { get; set; }
        public string? NomClientLivraison { get; set; }
        public string? TelClientFacturation { get; set; }
        public string? TelClientLivraison { get; set; }
        public string? Mail { get; set; }
        public string? Ville { get; set; }
        public string? Pays { get; set; }
        public string? CodePostal { get; set; }
        public string? Adresse1 { get; set; }
        public string? Adresse2 { get; set; }
        public string? Adresse3 { get; set; }
        public string? Site { get; set; }
        public DateTime Date { get; set; }
        public bool Prepare { get; set; }
        public bool Expedie { get; set; }
        public string? NumeroSuivi { get; set; }
    }
}