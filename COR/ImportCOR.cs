using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.COR
{
    internal abstract class ImportCOR : Import
    {
        private ImportCOR _suivant;

        public void setSuivant(ImportCOR suivant)
        {
            _suivant = suivant;
        }

        public override bool resoudre(string typeImport)
        {
            if(typeImport.Equals(getType()))
                return true;
            return _suivant != null ? _suivant.resoudre(typeImport) : false;
        }

        public abstract string getType();

        public abstract void parserFile(string filename);
    }
}
