using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    public static class ConsoleLoop
    {
        public static void consoleLoop()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.WriteLine("SQLConsole. Version 1.0");
            Console.WriteLine("Author: Andrew Cranston <andrewc@hendrygroup.com.au>");
            Console.WriteLine("");

            Macros.registerMacros();
            
            new Interpreter().mainInterpreterLoop();
        }
    }
}
