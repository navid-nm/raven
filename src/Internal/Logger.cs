using System.Drawing;

namespace Raven.Internal
{
    public static class Logger
    {
        public static void RaiseProblem(string message, bool fatal = true, bool colored = true)
        {
            string prepend = "";
            string suffix = "";
            string msg = message;

            if (colored)
            {
                prepend = $"\x1B[31m";
                suffix = $"\x1B[0m";
            }
            Console.WriteLine($"{prepend}{msg}{suffix}");
            if (fatal)
            {
                Environment.Exit(1);
            }
        }

        public static void Log(string message, State state)
        {
            ConsoleColor colorCode =
                state == State.SUCCESS ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.ForegroundColor = colorCode;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
