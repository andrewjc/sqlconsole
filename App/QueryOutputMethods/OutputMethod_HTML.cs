using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_HTML:OutputMethod_Template
    {
        public override string OutputMethodName()
        {
            return "HTML";
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
               columnString += String.Format("<td>{0}</td>", columnName);
           }

           dataBuffer += String.Format("<tr>{0}</tr>", columnString);

           foreach (System.Collections.ArrayList row in rows)
           {
                rowString = "";
                foreach (SQLConsole.Data.AbstractRowField val in row)
                {
                    rowString += String.Format("<td>{0}</td>",val.fieldvalue);
                }
                dataBuffer += String.Format("<tr>{0}</tr>",rowString);
           }

           return String.Format("<table>{0}</table>",dataBuffer);
           
        }
    }
}
