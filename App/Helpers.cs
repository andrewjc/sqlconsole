using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLConsole.Data;
namespace sqlconsole
{
    static class Helpers
    {
        public static void showDatabaseInfo()
        {
            try
            {
                Console.WriteLine("");
                Console.WriteLine("   Server: " + Program.dbo.GetDatabaseConnector().svrinfo._server);
                Console.WriteLine(" Database: " + Program.dbo.GetDatabaseConnector().svrinfo._database);
                Console.WriteLine(" Username: " + Program.dbo.GetDatabaseConnector().svrinfo._username);
                Console.WriteLine(" Password: " + "".PadRight(Program.dbo.GetDatabaseConnector().svrinfo._password.Length), '*');
                Console.WriteLine("  DB Type: " + Program.dbo.GetDatabaseConnector().svrinfo._dbtype.ToString());
                Console.WriteLine("");


            }
            catch { Console.WriteLine("Not Connected!"); }
        }

        public static void showTableStructure(string tablename)
        {
            Console.WriteLine("");
            Console.WriteLine("Getting table structure for:" + tablename);
            try
            {
                ATable tbl = Program.dbo.getTableObject(tablename);
                if (tbl == null) throw (new Exception("Table not found."));
                foreach (AField a in tbl.getFieldList())
                {
                    string mods = "";
                    if (a.hasModifier(ABSTRACTFIELDMODIFIERS.PrimaryKey))
                        mods += "PK ";
                    if (a.hasModifier(ABSTRACTFIELDMODIFIERS.NotNull))
                        mods += "NN ";
                    if (a.hasModifier(ABSTRACTFIELDMODIFIERS.IndexKey))
                        mods += "IDX ";
                    if (a.hasModifier(ABSTRACTFIELDMODIFIERS.ForeignKey))
                        mods += "FK ";
                    if (a.hasModifier(ABSTRACTFIELDMODIFIERS.AutoIncrement))
                        mods += "AI ";

                    Console.Write("   " +a.name + " : " + a.type + "("+a.maxsize+") "+mods+"\r\n");
                }


            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
