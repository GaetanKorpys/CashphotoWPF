using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF.COR
{
    internal class ExpertCashphoto : ImportCOR
    {
        public override string getType()
        {
            return "Cashphoto";
        }

        public override void parserFile(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
