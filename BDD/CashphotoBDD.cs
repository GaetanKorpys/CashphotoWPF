using CashphotoWPF.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.BDD
{
    public class CashphotoBDD : DbContext
    {
        private void InitialiserBDD()
        {
            Constante constante = Constante.GetConstante();
            if (Database.EnsureCreated())
            {
                constante.BDDOK = true;
            }
        }

        public CashphotoBDD(): base()
        {
            InitialiserBDD();
        }

        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Article> Articles { get; set; }

    }
}
