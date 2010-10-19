using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    static class Command_Set_Handler
    {
        public static void handle(string[] paramlist)
        {
            string key = null;
            string value = null;
            bool flagA = false;
            for(int i=0;i<paramlist.Count();i++) {
                if(paramlist[i] == "=")
                {
                    flagA = true;
                }
                else
                if(!flagA)
                    key = String.IsNullOrWhiteSpace(key)?paramlist[i]:key + " " + paramlist[i];
                else
                    value = String.IsNullOrWhiteSpace(value)?paramlist[i]:value + " " + paramlist[i];

            }

            if(String.IsNullOrWhiteSpace(key) == false) {

                if (value.Trim()[0] == '\"' && value.Trim()[value.Trim().Length - 1] == '\"')
                {
                    value = value.Trim();
                    value = value.Substring(1, value.Length - 2);
                }
                PropertyBag.SetProperty(key,value);
            }

#region OLDSETHANDLERS 
            /*
            switch (paramlist[0].ToLower(CultureInfo.CurrentCulture))
            {
                case "query":
                    Queries.setCurrentQuery(paramlist[2]);
                    break;
                case "data.view":
                    Queries.recordsetViewMode = Convert.ToInt32(paramlist[2],CultureInfo.CurrentCulture);
                    break;
                case "data.benchmark":
                    Queries.benchMarkMode = true;
                    break;
                case "data.output.emailaddress":
                    ((sqlconsole.QueryOutputMethods.OutputMethod_Email)Queries.recordsetOutputMethod).EmailAddress = paramlist[2];
                    break;                    
                case "data.output":
                    OutputMethod_Template outputMethod = null;
                    if (paramlist[2].ToLower(CultureInfo.CurrentCulture)=="screen")
                    {
                        outputMethod = new sqlconsole.QueryOutputMethods.OutputMethod_Screen();
                    }
                    else if (paramlist[2].ToLower(CultureInfo.CurrentCulture) == "printer")
                    {
                        outputMethod = new sqlconsole.QueryOutputMethods.OutputMethod_Printer();
                    }
                    else if (paramlist[2].ToLower(CultureInfo.CurrentCulture) == "email")
                    {
                        outputMethod = new sqlconsole.QueryOutputMethods.OutputMethod_Email();
                    }
                    else if (paramlist[2].ToLower(CultureInfo.CurrentCulture) == "csv")
                    {
						outputMethod = new sqlconsole.QueryOutputMethods.OutputMethod_CSV();
                    }
                    else if (paramlist[2].ToLower(CultureInfo.CurrentCulture) == "none")
                    {
                        outputMethod = new sqlconsole.QueryOutputMethods.OutputMethod_None();
                    }

                    Queries.recordsetOutputMethod = outputMethod;
                    break;
            }
             * */
#endregion
        }
    }
}
