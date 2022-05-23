using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.BDD
{
    public class Commande
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCommande { get; set; }
        public string NumCommande { get; set; }
        public double? Poids { get; set; }
        public double? Poids2 { get; set; }
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
        public bool Preparer { get; set; }
        public bool Expedier { get; set; }
        public string? NumeroSuivi { get; set; }
        public string? NumeroSuivi2 { get; set; }
    }
}
