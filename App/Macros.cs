using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    static class Macros
    {
        public static System.Collections.Hashtable macroAliases = new System.Collections.Hashtable();
        public static void newMacroScreen()
        {

            Console.WriteLine("--------------------------------");
            Console.WriteLine("         --NEW MACRO--          ");
            Console.Write("Macro Name:");
            string macroName = Console.ReadLine();
            bool doneWithAliases = false;
            
            System.Collections.ArrayList aliases = new System.Collections.ArrayList();
            while (!doneWithAliases)
            {
                Console.Write("Add Alias (Enter for done):");
                string aName = Console.ReadLine();
                if (String.IsNullOrEmpty(aName.Trim())) doneWithAliases = true;
                else
                    aliases.Add(aName);
            }
            Console.WriteLine("Macro Code:");
            bool doneCode = false;
            string macrocode = "Macro:\r\nName:"+macroName+"\r\n";
            foreach (string alias in aliases)
                macrocode += "Alias:" + alias + "\r\n";

            macrocode += "\r\n\r\nbeginmacro\r\n";
            while (!doneCode)
            {
                Console.Write(">");
                string line = Console.ReadLine();
                macrocode += line + "\r\n";
                if (line.Trim().ToLower(CultureInfo.CurrentCulture) == "exit" || line.Trim().ToLower(CultureInfo.CurrentCulture) == "exit;")
                {
                    doneCode = true;
                }
            }
            macrocode += "\r\nendmacro\r\n";

            System.IO.File.WriteAllText("macros/" + macroName + ".txt", macrocode);
            registerMacros();
        }

        public static void registerMacros()
        {
            Macros.macroAliases.Clear();
            if (System.IO.Directory.Exists("macros") == false) return;
            System.IO.FileInfo[] files = new System.IO.DirectoryInfo("macros").GetFiles();
            foreach (System.IO.FileInfo fi in files)
            {
                string[] lines = System.IO.File.ReadAllText(fi.FullName).Split('\n');
                // gather macrocode and aliases
                string macrocode = "";
                Boolean isMacroCode = false;
                System.Collections.ArrayList aliases = new System.Collections.ArrayList();
                foreach (string line in lines)
                {
                    if (line.Length > 1 && line.Trim() != null)
                    {
                        if (line.Trim() == "beginmacro") isMacroCode = true;
                        else if (line.Trim() == "endmacro") isMacroCode = false;
                        else
                        {
                            if(line.Length > 3)
                            if (line.Substring(0, "Alias".Length) == "Alias")
                            {
                                aliases.Add(line.Substring("Alias:".Length).Trim().ToLower(CultureInfo.CurrentCulture));
                            }

                            if (isMacroCode)
                            {
                                macrocode += line + "\r\n";
                            }
                        }
                    }
                }
                foreach (string alias in aliases)
                {
                    Macros.macroAliases.Add(alias, macrocode);
                }
            }
        }

        public static void dumpAll()
        {
            foreach (string t in Macros.macroAliases.Keys)
            {
                Console.WriteLine(t);
            }
        }
    }
}
