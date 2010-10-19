using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    static class ConnectionTools
    {
        public static void Connect(string host, string username, string password, string db, SQLConsole.Data.DATABASETYPES type)
        {
            try
            {
                Program.dbo = new SQLConsole.Data.Database(new SQLConsole.Data.LLDBA(host, username, password, db, type));
                Program.dbo.Open();
                Console.WriteLine("Connected Successfully to: " + db + "@" + host);
                Interpreter.prompt = Program.dbo.GetDatabaseConnector().svrinfo._database + ">";
                            
                return;
            }
            catch {
                Console.WriteLine("Error: Cannot open database connection!");
            }
        }
    }
}
