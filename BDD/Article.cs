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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdArticle { get; set; }
        public string NumCommande { get; set; }
        public string NomArticle { get; set; }
        public double Prix { get; set; }
        public string Sku { get; set; }
        public double Taxe { get; set; }
        public int Quantite { get; set; }

    }
}
