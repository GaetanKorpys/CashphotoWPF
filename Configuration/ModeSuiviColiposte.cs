using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.Configuration
{
    public class ModeSuiviColiposte
    {
        private  ModeSuiviColiposte_LST _mode;
        private static  ModeSuiviColiposte _instance = null;
        private ModeSuiviColiposte() { }

        public static ModeSuiviColiposte GetInstance()
        {
            if(_instance == null)
            {
                _instance = new ModeSuiviColiposte();
            }
            return _instance;
        }
      

        //Liste des modes de suivi disponible avec Coliposte
        public enum ModeSuiviColiposte_LST 
        {
            ExpertInternational,
            AvecSignature,
            SansSignature,
            PointRelais,
            Autre
        };

        public void setMode(ModeSuiviColiposte_LST mode)
        {
            _mode = mode;
        }

        public ModeSuiviColiposte_LST getMode()
        {
            return _mode;
        }

        public string getString(ModeSuiviColiposte_LST mode)
        {
            string modestring;
            switch (mode)
            {
                case ModeSuiviColiposte_LST.ExpertInternational:
                    modestring = "COLISSIMO EXPERT INTERNATIONAL";
                    break;
                case ModeSuiviColiposte_LST.AvecSignature:
                    modestring = "COLISSIMO Livraison a domicile avec signature";
                    break;
                case ModeSuiviColiposte_LST.SansSignature:
                    modestring = "COLISSIMO Livraison a domicile sans signature";
                    break;
                case ModeSuiviColiposte_LST.PointRelais:
                    modestring = "COLISSIMO Livraison en point relais";
                    break;
                default:
                    modestring = "Autre";
                    break;
            }
            return modestring;
        }

    }
}

