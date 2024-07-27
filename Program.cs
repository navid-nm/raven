if (args.Length != 1)
{
    Console.WriteLine("Please provide the input file path as an argument.");
    return;
}

var inputFilePath = args[0];
if (!File.Exists(inputFilePath))
{
    Console.WriteLine("The input file does not exist.");
    return;
}

var sourceCode = File.ReadAllText(inputFilePath);
var parser = new Raven.Internal.RavenParser(sourceCode);
var jsCode = parser.Transpile();

if (inputFilePath != null)
{
    var dirname = Path.GetDirectoryName(inputFilePath);
    if (dirname != null)
    {
        var outputFilePath = Path.Combine(Path.GetFileNameWithoutExtension(dirname) + ".js");
        File.WriteAllText(outputFilePath, jsCode);
        Console.WriteLine($"Transpiled JavaScript code written to {outputFilePath}");
    }
}
