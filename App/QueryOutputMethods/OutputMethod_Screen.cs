using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLConsole.Data;


namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_Screen:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "Screen";
        }
       public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows, 
            System.Collections.Hashtable rowLengthLookup)
        {
            OutputMethod_TXTOnly output = new OutputMethod_TXTOnly();
            Console.WriteLine(output.output(columnHeaders, rows, rowLengthLookup));
            return "";
        }
    }
}
