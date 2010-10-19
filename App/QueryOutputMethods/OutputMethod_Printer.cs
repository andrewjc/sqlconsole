using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SQLConsole.Data;

namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_Printer:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "Printer";
        }
        public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows,
            System.Collections.Hashtable rowLengthLookup)
        {
            string rowString = "";
            string columnString = "";
            string dataBuffer = "";
            int printWidth = 0;

            foreach (string columnName in columnHeaders)
            {
                if (printWidth < columnName.Length) printWidth = columnName.Length;
                columnString += columnName.PadRight((int)rowLengthLookup[columnName], ' ') + "|";
            }
            int tmp = 0;
            foreach (int width in rowLengthLookup.Values)
                if (tmp < width) tmp = width;

            printWidth += tmp;

            dataBuffer += ("".PadRight(printWidth, '_'));
            dataBuffer += ("\r\n"+columnString+"\r\n");
            dataBuffer += ("".PadRight(printWidth, '_') + "\r\n");


            foreach (System.Collections.ArrayList row in rows)
            {
                rowString = "";
                foreach (AbstractRowField val in row)
                {
                    rowString += val.fieldvalue.PadRight((int)rowLengthLookup[val.fieldname], ' ') + "|";
                }
                dataBuffer += rowString + "\r\n";
            }

            

            // Allow the user to select a printer.
            PrintDialog pd = new PrintDialog();
            pd.PrinterSettings = new PrinterSettings();
            if (DialogResult.OK == pd.ShowDialog())
            {
                // Send a printer-specific to the printer.
                System.IO.File.WriteAllText("C:\\temp\\queryresults.txt", dataBuffer);
            }
            return "";
        }
    }
}
