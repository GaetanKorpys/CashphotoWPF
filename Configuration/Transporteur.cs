using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.Configuration
{
    public class Transporteur
    {
        private Transporteurs _transporteurs;
        private static Transporteur _instance = null;
        private Transporteur() { }

        public static Transporteur GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Transporteur();
            }
            return _instance;
        }


        //Liste des modes de suivi disponible avec Coliposte
        public enum Transporteurs
        {
           Coliposte,
           GLS
        };

        public void setTransporteur(Transporteurs mode)
        {
            _transporteurs = mode;
        }

        public Transporteurs getTransporteur()
        {
            return _transporteurs;
        }

        public string getString()
        {
            return _transporteurs.ToString();
        }
    }
}
