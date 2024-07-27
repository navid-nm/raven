using Raven.Internal;

static void ProcessFile(string inputFilePath)
{
    if (!File.Exists(inputFilePath))
    {
        Logger.RaiseProblem($"The input file '{inputFilePath}' does not exist.");
    }

    var sourceCode = File.ReadAllText(inputFilePath);
    var path = Path.GetDirectoryName(inputFilePath);

    if (path != null)
    {
        var parser = new RavenParser(sourceCode, path);
        var jsCode = parser.Transpile();
        var outputFilePath = Path.Combine(
            path,
            $"{Path.GetFileNameWithoutExtension(inputFilePath)}.js"
        );
        File.WriteAllText(outputFilePath, jsCode);
        Console.WriteLine($"Transpiled JS written to {outputFilePath}");
    }
}

if (args.Length == 0)
{
    string currentDirectory = Directory.GetCurrentDirectory();
    string configFilePath = Path.Combine(currentDirectory, ".rconf");
    if (!File.Exists(configFilePath))
    {
        Logger.RaiseProblem(".rconf is missing in the current directory.");
    }
    var ravenFiles = Directory.GetFiles(currentDirectory, "*.raven", SearchOption.AllDirectories);
    if (ravenFiles.Length == 0)
    {
        Logger.RaiseProblem("No source files found in the current directory and subdirectories.");
    }
    foreach (var ravenFile in ravenFiles)
    {
        ProcessFile(ravenFile);
    }
}
else if (args.Length == 1)
{
    var inputFilePath = args[0];
    ProcessFile(inputFilePath);
}
else if (args.Length == 1 && args[0] == "--version")
{
    Console.WriteLine("0.0.1");
}
else
{
    Console.WriteLine("Provide the input file path as an argument.");
    return;
}
