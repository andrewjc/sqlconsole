using System;
using System.Collections.Generic;
using SQLConsole.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    static class ConnectionPresets
    {
        static public void getDBOpts()
        {
            string resp = "";
            DATABASETYPES dbtype = DATABASETYPES.MSSQL2005;
            string host = "";
            string username = "";
            string password = "";
            string dbname = "";

            #region PresetListAndSelectCode
            bool connectok = false;
            // show the preset host list...
            Console.WriteLine("Select server preset to continue:");
            System.IO.FileInfo[] infos = new System.IO.DirectoryInfo("presets").GetFiles();
            int i = 1;
            foreach (System.IO.FileInfo fi in infos)
            {
                string friendlyName = "";
                friendlyName = sqlconsole.Utils.getFriendlyNameFromPreset(fi.FullName);

                Console.WriteLine(i++ + ": " + friendlyName);
            }
            
            while (true)
            {
                try
                {
                    Console.WriteLine("\r\nTip: Type NEW to create a new preset.");
                    Console.Write("Preset #:");
                    resp = Console.ReadLine();
                    if (resp.ToUpper(CultureInfo.CurrentCulture) == "NEW")
                    {
                        #region SpecifyServerCode
                        connectok = false;
                        while (!connectok)
                        {
                            Console.Write("SQL Host:");
                            host = Console.ReadLine();

                            Console.Write("Username:");
                            username = Console.ReadLine();

                            Console.Write("Password:");
                            password = Console.ReadLine();

                            string type = "";
                            while (String.IsNullOrEmpty(type))
                            {
                                Console.WriteLine("Available Types (1,2,3,4):");
                                Console.WriteLine("   1: MYSQL = 1, 2: MSSQL, 3: MSSQL 2005, 4: MSACCESS, 5: Oracle, 6: SQLITE");
                                Console.Write("Database Type:");
                                type = Console.ReadLine();

                                if (type == "4")
                                {
                                    Console.WriteLine("Error: ORACLE SUPPORT NOT IMPLIMENTED");
                                    type = "";
                                }
                            }

                            Console.Write("Database (Blank to list):");
                            dbname = Console.ReadLine();
                            if (String.IsNullOrEmpty(dbname))
                            { // MYSQL = 1, MSSQL = 2, MSSQL2005 = 3, MSACCESS = 4
                                // give a database list...
                                dbtype = LLDBA.getProviderFromInt(Convert.ToInt32(type,CultureInfo.CurrentCulture));
                                Program.dbo = new Database(new LLDBA(host, username, password, dbtype));
                                Program.dbo.Open();
                                System.Collections.ArrayList al = Program.dbo.getDatabaseList();
                                i = 1;
                                foreach (string t in al)
                                {
                                    Console.WriteLine(i++ + ":" + t);
                                }

                                while (true)
                                {
                                    try
                                    {
                                        Console.Write("Database Num:");
                                        dbname = (string)al[Convert.ToInt32(Console.ReadLine(),CultureInfo.CurrentCulture) - 1];
                                    }
                                    catch { Console.WriteLine("Please enter a valid database number from the list."); continue; }
                                    break;
                                }
                            }

                            try
                            {
                                Program.dbo = new Database(new LLDBA(host, username, password, dbname, dbtype));
                            }
                            catch (Exception e)
                            {
                            	System.Diagnostics.Debug.Print(e.Message);
                                Console.WriteLine("Unable to connect to the database server. Check settings and try again.");
                                connectok = false;
                                continue;
                            }
                            connectok = true;
                            Console.WriteLine();
                            Console.WriteLine("Connected Successfully to: " + dbname + "@" + host);
                            Interpreter.prompt = Program.dbo.GetDatabaseConnector().svrinfo._database + ">";
                            resp = "";
                            while (resp.ToUpper(CultureInfo.CurrentCulture) != "Y" && resp.ToUpper(CultureInfo.CurrentCulture) != "N")
                            {

                                Console.Write("Save these settings to a preset file (Y/N)");
                                resp = Console.ReadLine();
                                Console.WriteLine();
                            }
                            if (resp.ToUpper(CultureInfo.CurrentCulture) == "Y")
                            {
                                // save the preset...
                                settingsClass settings = new settingsClass();
                                settings.server = host;
                                settings.userid = username;
                                settings.password = password;
                                settings.dbtype = LLDBA.getProviderFromInt(Convert.ToInt32(dbtype,CultureInfo.CurrentCulture));
                                settings.database = dbname;
                                System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(settingsClass));
                                System.IO.TextWriter w = new System.IO.StreamWriter("presets/" + DateTime.Now.Ticks + ".xml");
                                s.Serialize(w, settings);
                                w.Close();
                            }
                        }
                        #endregion
                    }
                    else
                    {

                        int presetNumber = Convert.ToInt32(resp,CultureInfo.CurrentCulture);
                        settingsClass settings = sqlconsole.Utils.loadSettingsFile(infos[presetNumber - 1].FullName);
                        Program.dbo = new Database(new LLDBA(settings.server, settings.userid, settings.password, settings.database, settings.dbtype));
                        try
                        {
                            Program.dbo.Open();
                        }
                        catch (Exception e)
                        {
                        	System.Diagnostics.Debug.Print(e.Message);
                            Console.WriteLine("Unable to connect to the database server. Check settings and try again.");
                            Console.WriteLine("Reason: " + e.InnerException.Message);

                            connectok = false;
                            continue;
                        }
                        connectok = true;
                        Console.WriteLine();
                        Console.WriteLine("Connected Successfully to: " + sqlconsole.Utils.getFriendlyNameFromPreset(infos[presetNumber - 1].FullName));
                        Interpreter.prompt = Program.dbo.GetDatabaseConnector().svrinfo._database + ">";
                        break;
                    }
                }
                catch { continue; }
                break;
            }
            #endregion
        }
    }
}
