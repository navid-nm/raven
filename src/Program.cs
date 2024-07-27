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
        Console.WriteLine($"Transpiled to -> {outputFilePath}");
    }
}

if (args.Length == 0)
{
    string currentDirectory = Directory.GetCurrentDirectory();
    var ravenFiles = Directory.GetFiles(currentDirectory, "*.rn", SearchOption.AllDirectories);
    if (ravenFiles.Length == 0)
    {
        Logger.RaiseProblem(
            "No source files found in the current directory or its subdirectories."
        );
    }
    foreach (var ravenFile in ravenFiles)
    {
        ProcessFile(ravenFile);
    }
}
else if (args.Length == 1)
{
    if (args[0] == "--version")
    {
        Console.WriteLine("0.0.1");
        return;
    }
    var inputFilePath = args[0];
    if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
    {
        ProcessFile(inputFilePath);
    }
    else
    {
        Console.WriteLine("Provide a valid .rn file path as an argument.");
    }
}
else
{
    Console.WriteLine("Provide the input file path as an argument.");
}
