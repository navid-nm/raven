using Raven.Internal;

static void ProcessFile(string inputFilePath, bool useDistDirectory)
{
    if (!File.Exists(inputFilePath))
    {
        Logger.RaiseProblem($"The input file '{inputFilePath}' does not exist.");
        return;
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

            Logger.Log($"Transpiled to -> {outputFilePath}", State.SUCCESS);
        }
        catch (Exception ex)
        {
            Logger.RaiseProblem($"Issue was encountered during transpilation: {ex.Message}");
        }
    }
}

bool useDistDirectory = false;

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
        ProcessFile(ravenFile, useDistDirectory);
    }
}
else if (args.Length == 1)
{
    if (args[0] == "--version")
    {
        Console.WriteLine("0.0.5");
        return;
    }
    if (args[0] == "--api")
    {
        Glob.IsApi = true;
    }
    else
    {
        var inputFilePath = args[0];
        if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
        {
            ProcessFile(inputFilePath, useDistDirectory);
        }
        else
        {
            Console.WriteLine("Provide a valid .rn file path as an argument.");
        }
    }
}
else if (args.Length == 2 && args[0] == "-d")
{
    useDistDirectory = true;
    var inputFilePath = args[1];
    if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
    {
        ProcessFile(inputFilePath, useDistDirectory);
    }
    else
    {
        Console.WriteLine("Provide a valid .rn file path as an argument.");
    }
}
else
{
    Console.WriteLine("Invalid arguments. Usage: [-d] [<input file path(s)>]");
}
