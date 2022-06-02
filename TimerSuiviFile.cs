using CashphotoWPF.BDD;
using CashphotoWPF.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CashphotoWPF
{
    internal class TimerSuiviFile
    {
        private MainWindow _app; //un accès à la fenêtre principale
        private double _hash;
        private DispatcherTimer _timer;
        private Commande _commande;

        public TimerSuiviFile(MainWindow app, double hash, Commande commande)
        {
            _app = app;
            _hash = hash;
            _commande = commande;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += new EventHandler(OnTimedEvent);
            _timer.Start();
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            string[] tab;
            string filename;
            double hash;

            System.Diagnostics.Debug.WriteLine("ON CHERCHE");

            tab = Directory.GetFiles(constante.numeroSuiviColiposte);
            if (tab.Length > 0)
            {
                filename = System.IO.Path.GetFileName(tab[0]);
                string path = constante.numeroSuiviColiposte + "//" + filename;

                hash = new FileInfo(path).Length;

                if (_hash != hash)
                {
                    Suivi suivi = new Suivi(_app);
                    suivi.createSuiviFromCommande(_commande);
                    StopRecherche();
                }
            }
        }

        private void StopRecherche()
        {
            _timer.Stop();
            System.Diagnostics.Debug.WriteLine("stop");
        }
    }
}
