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
            string colorCode = state == State.SUCCESS ? "\033[32m" : "\033[33m";
            Console.WriteLine($"{colorCode}{message}\033[0m");
        }
    }
}
