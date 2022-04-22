using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.BDD
{
    public class Article
    {
        [Key]
        public string IdArticle { get; set; }
        public string IdCommande { get; set; }
        public string NomArticle { get; set; }
        public double Prix { get; set; }
        public string Sku { get; set; }
        public double Taxe { get; set; }
        public int Quantite { get; set; }

    }
}
