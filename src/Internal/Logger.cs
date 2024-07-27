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
    }
}
