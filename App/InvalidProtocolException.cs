using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    class InvalidProtocolException : Exception
    {
        private string givenProtocolName;
        public InvalidProtocolException(string protocol)
        {
            this.givenProtocolName = protocol;
        }

        public override string Message
        {
            get
            {
                return "The given protocol " + givenProtocolName + " is invalid.";
            }
        }
    }
}
