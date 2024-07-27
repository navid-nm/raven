using Raven.Internal;

if (args.Length != 1)
{
    Console.WriteLine("Provide the input file path as an argument.");
    return;
}

var inputFilePath = args[0];
if (!File.Exists(inputFilePath))
{
    Console.WriteLine("The input file does not exist.");
    return;
}

var sourceCode = File.ReadAllText(inputFilePath);
var path = Path.GetDirectoryName(inputFilePath);

if (path != null)
{
    var parser = new RavenParser(sourceCode, path);
    var jsCode = parser.Transpile();
    var dirname = Path.GetDirectoryName(inputFilePath);
    if (dirname != null)
    {
        var outputFilePath = Path.Combine(dirname, "out.js");
        File.WriteAllText(outputFilePath, jsCode);
        Console.WriteLine($"Transpiled JS written to {outputFilePath}");
    }
}
