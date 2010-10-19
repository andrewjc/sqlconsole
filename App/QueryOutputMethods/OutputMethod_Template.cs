using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    abstract public class OutputMethod_Template
    {
        abstract public string OutputMethodName();
        abstract public string output(
            System.Collections.ArrayList columnHeaders, 
            System.Collections.ArrayList rows,
            System.Collections.Hashtable rowLengthLookup);
    }
}
