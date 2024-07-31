using Mono.Options;
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
            if (Glob.IsLoud)
            {
                Logger.Log($"Transpiled to -> {outputFilePath}", State.SUCCESS);
            }
        }
        catch (Exception ex)
        {
            Logger.RaiseProblem($"Issue during transpilation: {ex.Message}");
        }
    }
}

bool useDistDirectory = false;
bool showHelp = false;
bool showVersion = false;
bool isApi = false;
bool isLoud = false;

var options = new OptionSet
{
    { "d|dist", "Use the 'dist' directory for output.", v => useDistDirectory = v != null },
    { "l|loud", "Print detailed logs.", v => isLoud = v != null },
    { "a|api", "Enable API mode.", v => isApi = v != null },
    { "v|version", "Show version information.", v => showVersion = v != null },
    { "h|help", "Show help message.", v => showHelp = v != null }
};

try
{
    options.Parse(Environment.GetCommandLineArgs());

    if (showHelp)
    {
        Console.WriteLine("Usage: [options] [<input file path(s)>]");
        options.WriteOptionDescriptions(Console.Out);
        return;
    }

    if (showVersion)
    {
        Console.WriteLine("1.1.0");
        return;
    }

    if (isApi)
    {
        Glob.IsApi = true;
    }

    if (isLoud)
    {
        Glob.IsLoud = true;
    }

    var inputFilePaths = args.Length > 1 ? args[1..] : Array.Empty<string>();

    if (inputFilePaths.Length == 0)
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
    else
    {
        foreach (var inputFilePath in inputFilePaths)
        {
            if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
            {
                ProcessFile(inputFilePath, useDistDirectory);
            }
            else
            {
                Console.WriteLine(
                    $"Invalid file path or extension: {inputFilePath}. Provide a valid .rn file path."
                );
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
