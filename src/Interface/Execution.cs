using Raven.Data;
using Raven.Internal;

namespace Raven.Interface
{
    public static class Execution
    {
        public static string? ProcessFile(string inputFilePath, bool useDistDirectory)
        {
            if (!File.Exists(inputFilePath))
            {
                Logger.RaiseProblem($"The input file '{inputFilePath}' does not exist.");
                return "";
            }

            var sourceCode = File.ReadAllText(inputFilePath);
            var path = Path.GetDirectoryName(inputFilePath);

            if (path != null)
            {
                try
                {
                    var parser = new RavenParser(sourceCode, path);
                    var jsCode = parser.Transpile();

                    // Determine output directory
                    var outputDirectory = useDistDirectory ? Path.Combine(path, "dist") : path;

                    // Ensure the directory exists if using 'dist' directory
                    if (useDistDirectory)
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    // Save the transpiled code
                    var outputFilePath = Path.Combine(
                        outputDirectory,
                        $"{Path.GetFileNameWithoutExtension(inputFilePath)}.js"
                    );
                    File.WriteAllText(outputFilePath, jsCode);
                    if (Glob.IsLoud)
                    {
                        Logger.Log($"Transpiled to -> {outputFilePath}", State.SUCCESS);
                    }
                    return outputFilePath;
                }
                catch (Exception ex)
                {
                    Logger.RaiseProblem($"Issue during transpilation: {ex.Message}");
                }
            }
            return string.Empty;
        }
    }
}
