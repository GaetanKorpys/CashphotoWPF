using CashphotoWPF.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Data Source =";
            Constante constante = Constante.GetConstante();
            connectionString += constante.BDDIP;
            connectionString += ";";
            connectionString += constante.connectionString2;
            optionsBuilder.UseSqlServer(connectionString);
           
        }

        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Article> Articles { get; set; }

    }
}
