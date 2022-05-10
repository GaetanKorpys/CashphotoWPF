using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.BDD
{
    public class Article
    {
        [Key]
        public string NomArticle { get; set; }
        public string NumCommande { get; set; }
        public string? IdArticle { get; set; }
        public double Prix { get; set; }
        public string? Sku { get; set; }
        public double? Taxe { get; set; }
        public int Quantite { get; set; }

    }
}
