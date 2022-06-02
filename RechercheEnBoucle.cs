using CashphotoWPF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace CashphotoWPF
{
    internal class RechercheEnBoucle
    {
        private MainWindow _app; //un accès à la fenêtre principale
        private DispatcherTimer _timer;
        public bool _enMarche;
        public RechercheEnBoucle(MainWindow app)
        {
            _app = app;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 5);
            _timer.Tick += new EventHandler(OnTimedEvent);
            _timer.Start();
            _enMarche = true;
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            Constante constante = Constante.GetConstante();
            _app.TestConnexionBDD();

            if (constante.BDDOK)
                _app.ImporterCommandes();
            else
                StopRecherche();
        }

        public void StopRecherche()
        {
            _timer.Stop();
            _enMarche = false;
        }

        public void ActiverRecherche()
        {
            _timer.Start();
            _enMarche = true;
        }
    }
}
