using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.COR
{
    internal class Cor
    {
        private static Cor _instance = null;
        private Import _import;

        private Cor()
        {
            ImportCOR amazon = new ExpertAmazon();
            ImportCOR cashphoto = new ExpertCashphoto();
            amazon.setSuivant(cashphoto);
            _import = amazon;
        }


        public static Cor getInstance()
        {
            if (_instance == null)
                _instance = new Cor();
            return _instance;
        }

        public Import getImport()
        {
            return _import;
        }
    }
}