using Mono.Options;
using Raven.Data;
using Raven.Interface;

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
        Console.WriteLine("1.1.1");
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
            Execution.ProcessFile(ravenFile, useDistDirectory);
        }
    }
    else
    {
        foreach (var inputFilePath in inputFilePaths)
        {
            if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
            {
                Execution.ProcessFile(inputFilePath, useDistDirectory);
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
