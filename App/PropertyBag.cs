using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* PropertyBag
 *  Implements a key/value store for in-memory properties. 
 *  Command Usage:
 *  set some_key_here = some_value_here
 *  set some key here = some value here
 * */

namespace sqlconsole
{
    class PropertyBag
    {
        public System.Collections.Hashtable KeyValueTable;
        protected static PropertyBag _instance;
        protected PropertyBag()
        {
            KeyValueTable = new System.Collections.Hashtable();
        }

        protected static PropertyBag _Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PropertyBag();

                return _instance;
            }
        }
        internal static string GetProperty(string key, string DefaultValue = null)
        {
            return (string)PropertyBag._Instance.KeyValueTable[key] ?? DefaultValue;
        }

        internal static void SetProperty(string key, string value)
        {
            PropertyBag._Instance.KeyValueTable[key] = value;
        }
    }
}
