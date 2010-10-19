using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_None:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "None";
        }
       public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows, 
            System.Collections.Hashtable rowLengthLookup)
        {
            return "";
        }
    }
}
