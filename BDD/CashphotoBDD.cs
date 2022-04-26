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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Constante constante = Constante.GetConstante();
            string connectionString;
            if (constante.BDDIP.Equals("127.0.0.1"))
            {
                connectionString = constante.connectionStringLocal;
            }
            else
            {
                connectionString = "Data Source =";
                connectionString += constante.BDDIP;
                connectionString += ";";
                connectionString += constante.connectionString2;
            }
            System.Diagnostics.Debug.WriteLine(connectionString);
            optionsBuilder.UseSqlServer(connectionString);
           
        }

        public DbSet<Commande> Commandes { get; set; }
        public DbSet<Article> Articles { get; set; }

    }
}
