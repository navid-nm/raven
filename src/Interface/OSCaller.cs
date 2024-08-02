using System.Diagnostics;
using System.Text;
using Raven.Data;

namespace Raven.Interface
{
    public static class OSCaller
    {
        private static string CallOS(string binaryPath, string parameters = "")
        {
            var result = new StringBuilder();
            using (Process p = new())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.StartInfo.FileName = binaryPath;
                p.StartInfo.Arguments = parameters;
                p.OutputDataReceived += (sender, args) => result.AppendLine(args.Data);
                p.ErrorDataReceived += (sender, args) => result.AppendLine(args.Data);
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            return result.ToString().Trim();
        }

        public static void RunProgram(string outName)
        {
            var runTimeList = new List<string> { "node", "bun" };
            var errorCount = 0;

            foreach (var runtime in runTimeList)
            {
                try
                {
                    var output = CallOS(runtime, outName);
                    Console.WriteLine(output);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    errorCount++;
                    continue;
                }
            }
            if (errorCount == runTimeList.Count)
            {
                Logger.RaiseProblem("No ECMAScript runtime found.");
            }
        }
    }
}
