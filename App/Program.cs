using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLConsole.Data;
using System.Windows.Forms;

namespace sqlconsole
{
    class Program
    {
        static public Database dbo;
        static public bool shutdown;
        
        [STAThread]
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.WriteLine(@"SQLConsole. Version 1.0");
            Console.WriteLine(@"Author: Andrew Cranston <andrew.cranston@gmail.com>");
            Console.WriteLine("");
           
            Macros.registerMacros();
            if (args.Length > 0)
            {
                //-runmacro macros/oracletest.txt
                if (args[0] == "-runmacro")
                {
                    string macroname = args[1];
                    
                    string[] macro = ((string)Macros.macroAliases[macroname]).Split('\n');
                    foreach (string Tline in macro)
                    {
                        string line = Tline;
                        if (line.Trim().Length > 0)
                        {

                            if (line.Substring(0, 1) == "@")
                            {
                                line = line.Substring(1);
                            }
                            new Interpreter().parseCommand(line);
                        }
                    }

                    Console.WriteLine("Closing connections please wait...");
                    return;
                }

                // -filename C:\my stuff\codebase\applications\BCA4Windows\Build\resources.dat -type SQLITE
                
            }
            new Interpreter().mainInterpreterLoop();
        }


        
    }
}
