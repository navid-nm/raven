using Mono.Options;
using Raven.Internal;

static void ProcessFile(string inputFilePath, bool useDistDirectory, bool useNative)
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
            IParser parser = useNative
                ? new CSParserVariant(sourceCode, path)
                : new RavenParser(sourceCode, path);

            var transpiledCode = parser.Transpile();

            // Determine output directory
            var outputDirectory = useDistDirectory ? Path.Combine(path, "dist") : path;

            // Ensure the directory exists if using 'dist' directory
            if (useDistDirectory)
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string nameToUse = $"{Path.GetFileNameWithoutExtension(inputFilePath)}.js";

            if (useNative)
            {
                nameToUse = "Program.cs";
            }
            // Save the transpiled code
            var outputFilePath = Path.Combine(outputDirectory, nameToUse);
            File.WriteAllText(outputFilePath, transpiledCode);
            if (!Glob.IsSilent)
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

bool showHelp = false;
bool useDistDirectory = false;
bool useNative = false;
bool isApi = false;
bool isSilent = false;

var options = new OptionSet
{
    { "d|dist", "Use 'dist' directory for output", d => useDistDirectory = d != null },
    { "n|native", "Use native parser for transpilation", n => useNative = n != null },
    { "v|version", "Display the version", v => showHelp = v != null },
    { "api", "Set API mode", a => isApi = a != null },
    { "s|silent", "Silent mode", s => isSilent = s != null },
    { "h|help", "Show this message and exit", h => showHelp = h != null }
};

List<string> extra;
try
{
    extra = options.Parse(args);
}
catch (OptionException e)
{
    Console.Write("raven: ");
    Console.WriteLine(e.Message);
    Console.WriteLine("Try `raven --help` for more information.");
    return;
}

if (showHelp)
{
    ShowHelp(options);
    return;
}

if (isApi)
{
    Glob.IsApi = true;
}

if (isSilent)
{
    Glob.IsSilent = true;
}

if (extra.Count == 0)
{
    string currentDirectory = Directory.GetCurrentDirectory();
    var ravenFiles = Directory.GetFiles(currentDirectory, "*.rn", SearchOption.AllDirectories);
    if (ravenFiles.Length == 0)
    {
        Logger.RaiseProblem(
            "No source files found in the current directory or its subdirectories."
        );
        return;
    }
    foreach (var ravenFile in ravenFiles)
    {
        ProcessFile(ravenFile, useDistDirectory, useNative);
    }
}
else
{
    foreach (var inputFilePath in extra)
    {
        if (File.Exists(inputFilePath) && Path.GetExtension(inputFilePath) == ".rn")
        {
            ProcessFile(inputFilePath, useDistDirectory, useNative);
        }
        else
        {
            Console.WriteLine(
                $"Provide a valid .rn file path as an argument. Invalid file: {inputFilePath}"
            );
        }
    }
}

static void ShowHelp(OptionSet p)
{
    Console.WriteLine("Usage: raven [OPTIONS]+ [FILES]");
    Console.WriteLine("Transpile .rn files to C#.");
    Console.WriteLine();
    Console.WriteLine("Options:");
    p.WriteOptionDescriptions(Console.Out);
}
