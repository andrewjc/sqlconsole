using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.CSharp;

namespace sqlconsole
{
    public class Interpreter
    {
        public static string prompt = "Disconnected:>";
        public static string lastStatement = "";
        Boolean commandEcho = true;
        Boolean commandConsumed = false;
        string commandBuffer;
        Boolean bufferContent = false;
        public void mainInterpreterLoop()
        {
            Boolean exitLoop =false;
           
            while (!exitLoop)
            {
                Console.Title = "SQL Console";
                Console.Write(prompt);
                string rawdata = Console.ReadLine();
                
                if (commandEcho)
                    Console.WriteLine(">" + rawdata);

                rawdata = rawdata.Trim();


                // Run macros...
                string macroNamePeek = rawdata;
                string[] macroParameters = new string[0];
                if (rawdata.Trim().Contains(" "))
                {
                    macroNamePeek = rawdata.Trim().Substring(0, rawdata.Trim().IndexOf(" "));
                    macroParameters = rawdata.Trim().Substring(rawdata.Trim().IndexOf(" ") + 1).Split(' ');
                }
                if (Macros.macroAliases.ContainsKey(macroNamePeek))
                {
                    commandConsumed = true;
                    string[] macro = ((string)Macros.macroAliases[macroNamePeek.ToLower(CultureInfo.CurrentCulture)]).Split('\n');
                    foreach (string Tline in macro)
                    {
                        string line = Tline;
                        if (line.Trim().Length > 0)
                        {
                            if (commandEcho && line.Substring(0, 1) != "@")
                                Console.WriteLine(">" + line);

                            if (line.Substring(0, 1) == "@")
                            {
                                line = line.Substring(1);
                            }

                            if (line.Contains("{{0}}"))
                            {
                                if (macroParameters.Count() == 0)
                                {
                                    Console.WriteLine("Command requires a parameter but none was given.\r\nSource:" + line);
                                    break;
                                }
                                line = line.Replace("{{0}}", macroParameters[0]);
                            }
                            if (line.Contains("{{1}}"))
                            {
                                if (macroParameters.Count() <= 2)
                                {
                                    Console.WriteLine("Command requires 2 parameters but none was given.\r\nSource:" + line);
                                    break;
                                }
                                line = line.Replace("{{1}}", macroParameters[1]);
                            }
                            if (line.Contains("{{2}}"))
                            {
                                if (macroParameters.Count() <= 3)
                                {
                                    Console.WriteLine("Command requires 3 parameters but none was given.\r\nSource:" + line);
                                    break;
                                }
                                line = line.Replace("{{2}}", macroParameters[2]);
                            }

                            commandConsumed = parseCommand(line);
                        }
                    }
                }
                else
                {

                    commandConsumed = parseCommand(rawdata);
                }

               

                if ((Program.shutdown)) {
                	exitLoop = true;
                }
            }
            Console.WriteLine("Exiting interpreter loop...");
        }

        public Boolean parseCommand(string Trawdata)
        {
        	if (String.IsNullOrEmpty(Trawdata)) return true;
            if (Trawdata.Length > 2 && Trawdata.Trim()[0] == '/' && Trawdata.Trim()[1] == '/') return true;
            string rawdata = Trawdata.Trim();
            Boolean commandConsumed = false;
            if (rawdata[rawdata.Length - 1] == ';')
                rawdata = rawdata.Substring(0, rawdata.Length - 1);
            // deal with parameter bodies... eg {something... }
            if (rawdata.Trim().LastIndexOf("}") > (rawdata.Length - 10) && rawdata.Trim().Contains("}"))
            {
                bufferContent = false;
                commandBuffer += rawdata + "\r\n";
                rawdata = commandBuffer;
                commandBuffer = "";
                //System.Diagnostics.Debugger.Break();
                commandConsumed = true;
            }
            else if ((rawdata.Trim().IndexOf("{") < 10 && rawdata.Trim().IndexOf("{") > 0) || bufferContent)
            {
                
                commandBuffer += rawdata+"\r\n";
                bufferContent = true;
                commandConsumed = true;
                return commandConsumed;
            }
           
            lastStatement = rawdata;
            
            if (!String.IsNullOrEmpty(rawdata.Trim()))
            {
                // single command
                switch (rawdata.Trim().ToLower(CultureInfo.CurrentCulture))
                {
                    case "help":
                        commandConsumed = true;
                        break;
                	case "namespaces":
                		commandConsumed= true;
                		Namespaces.Instance.modifyMode();
                		break;
                	case "show tables":case "get tables":case "gettables":case "usertables":case "user tables":
                		commandConsumed = true;
                		System.Collections.ArrayList tableList = Program.dbo.GetDatabaseProvider().getTableList();
                		OutputMethod_Template output = new sqlconsole.QueryOutputMethods.OutputMethod_Screen();
                		System.Collections.ArrayList rowCollection = new System.Collections.ArrayList();
                		int rowsize = 0;
                        if(tableList != null)
                		foreach(string tableName in tableList) {
                			System.Collections.ArrayList newRow = new System.Collections.ArrayList();
                			if(tableName.Length > rowsize)rowsize = tableName.Length;
                			newRow.Add(new SQLConsole.Data.AbstractRowField("Table Name", tableName));
                			rowCollection.Add(newRow);
                		}
                		System.Collections.Hashtable rowLookup = new System.Collections.Hashtable();
                		rowLookup.Add("Table Name", rowsize);
                		output.output(new System.Collections.ArrayList(new string[]{"Table Name"}), rowCollection, rowLookup);
                		
                		break;
                    case "dumpmacros":case "macros":
                        commandConsumed = true;
                        Macros.dumpAll();
                        break;
                    case "query":
                    case "queries":
                    case "new query":
                    case "enter query mode":
                    case "enter new query":
                    case "enter query":
                        commandConsumed = true;
                        Queries.singleQueryScreen();
                        break;
                    case "show database info":
                    case "this":
                    case "who are you":
                    case "dbinfo":
                    case "showdbinfo":
                    case "show dbinfo":
                    case "show db":
                    case "showdb":
                        commandConsumed = true;
                        Helpers.showDatabaseInfo();
                        break;
                    case "new macro":
                    case "macro.new":
                    case "new macro()":
                    case "newmacro":
                    case "system.macros.new":
                    case "macros.new":
                        commandConsumed = true;
                        Macros.newMacroScreen();
                        break;
                    case "connect":
                    case "servers":
                    case "presets":
                        commandConsumed = true;
                        ConnectionPresets.getDBOpts();
                        break;
                    case "echo on":case "command echo on":
                        commandEcho = true;
                        commandConsumed = true;
                        break;
                    case "echo off":case "command echo off":
                        commandEcho = false;
                        commandConsumed = true;
                        break;
                    case "exit":
                        Program.shutdown = true;
                        commandConsumed = true;
                        break;
                    case "test":
                       	break;
                    case "benchmark":
                        Queries.benchMark(Queries.sqlQuery);
                        break;
                }
            }
            if (rawdata.IndexOf(" ") > 0)
            {
                // multipart command
                if(commandBuffer != null)
                if (commandBuffer.Length > 0)
                {
                    rawdata = commandBuffer;
                    bufferContent = false;
                }
                string command = rawdata.Substring(0, rawdata.IndexOf(" "));
                string parameters = rawdata.Substring(rawdata.IndexOf(" ") + 1);
                switch (command.Trim().ToLower(CultureInfo.CurrentCulture))
                {
                    case "show":
                        break;
                    case "getstruct":
                    case "showstruct":
                    case "tblstruct":
                    case "tabledetails":
                    case "tablestruct":
                        commandConsumed = true;
                        Helpers.showTableStructure(parameters);
                        break;
                    case "set":
                        if (parameters.Trim().Contains(" "))
                        {
                            string[] setParams = parameters.Split(' ');
                            Command_Set_Handler.handle(setParams);
                            commandConsumed = true;
                           
                        }
                        break;
                    case "connect":
                        string[] bits = parameters.Split(' ');
                        try
                        {
                            if (bits.Length == 1)
                            {
                                ConnectionTools.Connect(null, null, null, bits[0], SQLConsole.Data.DATABASETYPES.SQLITE);
                            }
                            else if (bits.Length == 2)
                            {
                                ConnectionTools.Connect(null, null, bits[1], bits[0], SQLConsole.Data.DATABASETYPES.SQLITE);
                            }
                            else
                            {
                                ConnectionTools.Connect(bits[0].Trim(), bits[1].Trim(), bits[2].Trim(), bits[3].Trim(), SQLConsole.Data.LLDBA.getProviderFromString(bits[4].Trim()));
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Invalid command usage.");
                            Console.WriteLine("Required Format:");
                            Console.WriteLine("   >connect [host, username, password, db, dbtype]");
                            Console.WriteLine(" or:");
                            Console.WriteLine("   >connect [file]");
                        }
                        commandConsumed = true;
                        
                        break;
                    case "execute":
                        Queries.executeDirect(parameters);
                        commandConsumed = true;
                        break;
                    case "select":
                        Queries.executeDirect("select " + parameters);
                        commandConsumed = true;
                        break;
                        /*
                    case "dexecute": //direct execute (raw sql)
                        Queries.executeDirect(parameters);
                        commandConsumed = true;
                        break;
                         * */
                }
                // settings.passwords.showpasswords = false;
            }
            commandBuffer = "";
            if (!commandConsumed && rawdata.Trim().Length > 1)
            {
                string basecommand = rawdata;

                if (basecommand.Length > 15)
                    basecommand = basecommand.Substring(0, 15) + "...";
                
                
                if(prompt.Contains("Disconnect"))
                {
                    Console.WriteLine("\r\nConnect to a server first. Type servers to view list.\r\n");
                }
                else
                {
                    Console.WriteLine("\r\nUnknown SQL console command: " + basecommand + ".\r\n Type help for suggestions.\r\n");
                }

                
            }
            return commandConsumed;
        }
    }
}
