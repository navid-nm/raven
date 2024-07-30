using System.Text.RegularExpressions;

namespace Raven.Internal
{
    public partial class CSParserVariant : IParser
    {
        private readonly string _sourceCode;
        private readonly string _basePath;
        private readonly Dictionary<string, string> _typeHints = new();
        private readonly Dictionary<string, string> _abbreviations = new();

        public CSParserVariant(string sourceCode, string basePath)
        {
            _sourceCode = sourceCode;
            _basePath = basePath;
        }

        public string Transpile()
        {
            var code = HandleImports(_sourceCode);

            code = HandleTemplates(code);
            code = ExtractAndProcessTypeHints(code);
            code = ExtractAndProcessAbbreviations(code);
            code = HandleTemplateLiterals(code);
            code = ReplaceContextAware(code);

            ValidateGeneratedCSharp(code);

            return code;
        }

        private string HandleImports(string code)
        {
            var regex = ImportPatternRegex();
            var matches = regex.Matches(code);
            foreach (Match match in matches)
            {
                var importPath = match.Groups[1].Value;
                var transpiledImport = ReadAndTranspileImport(importPath);
                code = code.Replace(match.Value, transpiledImport);
            }
            return code;
        }

        private string HandleTemplates(string code)
        {
            var regex = TemplatePatternRegex();
            var matches = regex.Matches(code);
            foreach (Match match in matches)
            {
                var templatePath = match.Groups[1].Value;
                code = code.Replace(match.Value, ReadAndConvertTemplate(templatePath));
            }
            return code;
        }

        private string ReadAndTranspileImport(string importPath)
        {
            var relativePath = importPath.Replace('.', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_basePath, relativePath + ".rnm");

            if (!File.Exists(fullPath))
            {
                Logger.RaiseProblem($"Imported file '{importPath}' not found at '{fullPath}'.");
            }

            var importCode = File.ReadAllText(fullPath) + "\n";
            var importParser = new CSParserVariant(importCode, _basePath);
            return importParser.Transpile();
        }

        private string ReadAndConvertTemplate(string templatePath)
        {
            var fullPath = Path.Combine(_basePath, templatePath);
            if (!File.Exists(fullPath))
            {
                Logger.RaiseProblem($"Template file '{templatePath}' not found.");
            }
            var templateContent = File.ReadAllText(fullPath);
            return templateContent;
        }

        private static string HandleTemplateLiterals(string code)
        {
            var pattern = @"@""(.*?)""";
            var regex = new Regex(pattern, RegexOptions.Singleline);
            return regex.Replace(
                code,
                match =>
                {
                    var content = match.Groups[1].Value;
                    return $"\"{content}\"";
                }
            );
        }

        private string ExtractAndProcessTypeHints(string code)
        {
            var typeHintPattern = @"\|\|\s*(\w+)\s*->\s*[\w\[\]]+";
            var regex = new Regex(typeHintPattern);
            var matches = regex.Matches(code);

            foreach (Match match in matches)
            {
                var variable = match.Groups[1].Value;
                if (!_typeHints.ContainsKey(variable))
                {
                    // Placeholder as type is not used further
                    _typeHints[variable] = "type";
                }

                // Remove the type hint from the code
                code = code.Replace(match.Value, "");
            }

            return code;
        }

        private string ExtractAndProcessAbbreviations(string code)
        {
            var abbrevPattern = @"abbrev\s*{([^}]*)}";
            var abbrevRegex = new Regex(abbrevPattern, RegexOptions.Singleline);
            var abbrevMatch = abbrevRegex.Match(code);

            if (abbrevMatch.Success)
            {
                var abbrevContent = abbrevMatch.Groups[1].Value;
                var abbrevLines = abbrevContent
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList();

                foreach (var line in abbrevLines)
                {
                    var parts = line.Split('=').Select(part => part.Trim()).ToArray();
                    if (parts.Length == 2)
                    {
                        var abbrev = parts[0];
                        var replacement = parts[1];
                        if (!_abbreviations.ContainsKey(abbrev))
                        {
                            _abbreviations[abbrev] = replacement;
                        }
                    }
                }

                // Remove the abbrev block from the code
                code = code.Replace(abbrevMatch.Value, "");
            }

            var inlineAbbrevPattern = @"abbrev\s+(\w+)\s*=\s*([\w.]+)";
            var inlineAbbrevRegex = new Regex(inlineAbbrevPattern);
            var inlineAbbrevMatches = inlineAbbrevRegex.Matches(code);

            foreach (Match match in inlineAbbrevMatches)
            {
                var abbrev = match.Groups[1].Value;
                var replacement = match.Groups[2].Value;
                if (!_abbreviations.ContainsKey(abbrev))
                {
                    _abbreviations[abbrev] = replacement;
                }
                code = code.Replace(match.Value, "");
            }

            foreach (var (abbrev, replacement) in _abbreviations)
            {
                var replacementPattern = $@"\b{Regex.Escape(abbrev)}\b";
                var replacementRegex = new Regex(replacementPattern);
                code = replacementRegex.Replace(code, replacement);
            }

            return code;
        }

        private static string ReplaceContextAware(string code)
        {
            var patterns = new (string pattern, Func<Match, string> replacement)[]
            {
                (@"raw\((.*?)\)", match => match.Groups[1].Value),
                (@"\bfn\s*(\w*)\s*\(", match => $"void {match.Groups[1].Value}("),
                (@"\bsay\s*\(", match => "Console.WriteLine("),
                (@"\buse\s*\(", match => "using ("),
                (@"\bwarn\s*\(", match => "Console.Error.WriteLine("),
                (@"}\s*die\s*\(", match => "} catch (Exception"),
                (@"\)\s*\.die\s*\(", match => ").catch (Exception"),
                (@"\bval\b", match => "var"),
                (@"xlet\s*{([^}]*)}", ReplaceXlet),
                (@"xset\((.*?)\)\s*{([^}]*)}", ReplaceXset),
                (@"xval\s*{([^}]*)}", ReplaceXval),
                (@"xvar\s*{([^}]*)}", ReplaceXvar),
                (@"\bclosed\s+stat\b", match => "private static "),
                (@"\bopen\s+stat\b", match => "public static "),
                (@"\bclosed\s+async\b", match => "private async "),
                (@"\bopen\s+async\b", match => "public async "),
                (@"\btmp\s+(\w+)\b", match => $"class {match.Groups[1].Value}"),
                (@"\bstat\s+(\w+)\b", match => $"static {match.Groups[1].Value}"),
                (@"\bmy\.", match => "this."),
                (
                    @"\binit\s*(\([^)]*\))?\s*{",
                    match => $"public {match.Groups[1].Value ?? string.Empty} {{"
                ),
                (@"\belif\b", match => "else if")
            };

            foreach (var (pattern, replacement) in patterns)
            {
                code = Regex.Replace(
                    code,
                    pattern,
                    new MatchEvaluator(replacement),
                    RegexOptions.IgnoreCase | RegexOptions.Multiline
                );
            }

            return code;
        }

        private static string ReplaceXlet(Match match)
        {
            var declarations = match
                .Groups[1]
                .Value.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => $"let {line.Trim()}");

            return string.Join("\n", declarations);
        }

        private static string ReplaceXset(Match match)
        {
            var objectName = match.Groups[1].Value.Trim();
            var properties = match
                .Groups[2]
                .Value.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => $"{objectName}.{line.Trim()}");

            return string.Join("\n", properties);
        }

        private static string ReplaceXval(Match match)
        {
            var declarations = match
                .Groups[1]
                .Value.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => $"const {line.Trim()}");

            return string.Join("\n", declarations);
        }

        private static string ReplaceXvar(Match match)
        {
            var declarations = match
                .Groups[1]
                .Value.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => $"var {line.Trim()}");

            return string.Join("\n", declarations);
        }

        private static void ValidateGeneratedCSharp(string csCode)
        {
            // Validation logic for C# code can be added here if needed
        }

        [GeneratedRegex(@"import\s+([\w\.]+)\s*;?")]
        private static partial Regex ImportPatternRegex();

        [GeneratedRegex(@"rhtml\(""(.*?)""\)")]
        private static partial Regex TemplatePatternRegex();
    }
}
