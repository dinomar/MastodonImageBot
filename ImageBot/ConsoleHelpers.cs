using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot
{
    class ConsoleHelpers
    {
        public static void PrintWarning(string message)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }

        public static void PrintError(string message)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }

        public static void PrintSuccess(string message)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }
    }
}
