using System;
using System.Collections.Generic;
using System.IO;
using bpac;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashphotoWPF
{
    internal class Etiquette
    {
        private string _chemin = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString() + "\\Etiquette.lbx";

        public void DoPrint()
        {
            //Document sample
            bpac.Document doc = new bpac.Document();
            doc.Open(_chemin);


            object[] printers = (object[])doc.Printer.GetInstalledPrinters();

            foreach (object printer in printers)
            {
                string printerName = printers[0].ToString();
                if (doc.Printer.IsPrinterOnline(printerName))
                {
                    System.Diagnostics.Debug.WriteLine("online " + printerName);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("offline " + printerName);
                }

            }

            bool b;

            b = doc.SetPrinter("Brother TD-2020", true);

            System.Diagnostics.Debug.WriteLine("    " + b);

            // Object collection acquisition
            bpac.IObjects objs = doc.Objects;

            // Object acquisition
            bpac.Object obj = doc.GetObject("QRCode");
            obj.Text = "12345";

            b = doc.StartPrint("", bpac.PrintOptionConstants.bpoDefault);
            System.Diagnostics.Debug.WriteLine("    " + b);

            b = doc.PrintOut(1, bpac.PrintOptionConstants.bpoDefault);

            System.Diagnostics.Debug.WriteLine("    " + b);

            System.Diagnostics.Debug.WriteLine("    " + doc.ErrorCode);

            doc.EndPrint();

            doc.Close();

        }
    }
}
