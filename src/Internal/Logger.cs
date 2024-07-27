using System.Drawing;

namespace Raven.Internal
{
    public static class Logger
    {
        public static void RaiseProblem(string message, bool fatal = true)
        {
            Console.WriteLine($"\x1B[31m{message}\x1B[0m");
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
