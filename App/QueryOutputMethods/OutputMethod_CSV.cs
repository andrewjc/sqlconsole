using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_CSV:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "CSV";
        }
        public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows, 
            System.Collections.Hashtable rowLengthLookup)
        {
            OutputMethod_TXTOnly output = new OutputMethod_TXTOnly();
            System.IO.File.WriteAllText("C:\\temp\\csv_" + DateTime.Now.ToFileTime() + ".csv", output.output(columnHeaders, rows, rowLengthLookup));

            return "";
        }
    }
}
