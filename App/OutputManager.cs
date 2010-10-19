using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* OutputManager
 *  Handles getting the right output manager.
 * */

namespace sqlconsole
{
    class OutputManager
    {
        protected static OutputManager _instance;
        private System.Collections.Hashtable outputCollection;
        protected OutputManager()
        {
            this.outputCollection = new System.Collections.Hashtable();

            RegisterOutputMethod(new QueryOutputMethods.OutputMethod_CSV());
            RegisterOutputMethod(new QueryOutputMethods.OutputMethod_Email());
            RegisterOutputMethod(new QueryOutputMethods.OutputMethod_Printer());
            RegisterOutputMethod(new QueryOutputMethods.OutputMethod_Screen());
        }

        public static OutputMethod_Template FromProtocol(string protocol)
        {
            if (protocol == null || _Instance.outputCollection.Contains(protocol) == false)
                throw (new InvalidProgramException(protocol));

            return (OutputMethod_Template)_Instance.outputCollection[protocol];
        }

        private void RegisterOutputMethod(OutputMethod_Template method)
        {
            this.outputCollection.Add(method.OutputMethodName().ToLower(), method);
        }

        protected static OutputManager _Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OutputManager();

                return _instance;
            }
        }
        
    }
}
