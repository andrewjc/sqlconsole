using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_TXTOnly:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "TXTOnly";
        }
        public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows, 
            System.Collections.Hashtable rowLengthLookup)
        {
           string rowString = "";
           string columnString = "";
           string dataBuffer = "";
           foreach (string columnName in columnHeaders)
           {
                columnString += columnName.PadRight((int)rowLengthLookup[columnName], ' ') + ",";
           }

           dataBuffer += columnString + "\r\n";

           foreach (System.Collections.ArrayList row in rows)
           {
                rowString = "";
                foreach (SQLConsole.Data.AbstractRowField val in row)
                {
                    rowString += "\""+val.fieldvalue.PadRight((int)rowLengthLookup[val.fieldname], ' ') + "\",";
                }
                dataBuffer += rowString + "\r\n";
           }

           return dataBuffer;
           
        }
    }
}
