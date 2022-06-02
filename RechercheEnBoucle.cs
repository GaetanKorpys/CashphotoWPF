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
        public RechercheEnBoucle(MainWindow app)
        {
            _app = app;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 5);
            _timer.Tick += new EventHandler(OnTimedEvent);
            _timer.Start();
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            _app.ImporterCommandes();
        }

        public void StopRecherche()
        {
            _timer.Stop();
        }
    }
}
