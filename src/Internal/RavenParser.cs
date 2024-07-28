using System.Text.RegularExpressions;
using Esprima;

namespace Raven.Internal
{
    public partial class RavenParser(string sourceCode, string basePath)
    {
        private readonly string _sourceCode = sourceCode;
        private readonly string _basePath = basePath;
        private readonly Dictionary<string, string> _typeHints = [];

        public string Transpile()
        {
            var code = HandleImports(_sourceCode);
            code = HandleTemplates(code);
            code = ExtractAndProcessTypeHints(code);
            code = HandleTemplateLiterals(code);
            code = ReplaceContextAware(code);

            ValidateGeneratedECMA(code, Glob.IsApi);

            return code;
        }

        private string HandleImports(string code)
        {
            var regex = ImportPatternRegex();
            var matches = regex.Matches(code);
            foreach (Match match in matches)
            {
                var importPath = match.Groups[1].Value;
                code = code.Replace(match.Value, ReadAndTranspileImport(importPath));
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
            var fullPath = Path.Combine(_basePath, importPath + ".rn");
            if (!File.Exists(fullPath))
            {
                Logger.RaiseProblem($"Imported file '{importPath}' not found.");
            }
            var importCode = File.ReadAllText(fullPath);
            var importParser = new RavenParser(importCode, _basePath);
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
            return $"`{templateContent}`";
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
                    return $"`{content}`";
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

        private static string ReplaceContextAware(string code)
        {
            var patterns = new (string pattern, Func<Match, string> replacement)[]
            {
                (@"raw\((.*?)\)", match => match.Groups[1].Value),
                (@"\bfn\s*(\w*)\s*\(", match => $"function {match.Groups[1].Value}("),
                (@"\bsay\s*\(", match => "console.log("),
                (@"\bwarn\s*\(", match => "console.error("),
                (@"}\s*die\s*\(", match => "} catch ("),
                (@"\)\s*\.die\s*\(", match => ").catch("),
                (@"\bdoc\.", match => "document."),
                (@"\bwin\.", match => "window."),
                (@"\bonready\s*\(", match => "document.addEventListener(\"DOMContentLoaded\","),
                (@"\.str\s*\(\)", match => ".toString()"),
                (@"\bdocument\.get\s*\(", match => "document.getElementById("),
                (@"\bdocument\.make\s*\(", match => "document.createElement("),
                (@"\bdocument\.listen\s*\(", match => "document.addEventListener("),
                (@"\.put\s*\(", match => ".appendChild("),
                (@"\.ClassName\b", match => ".className"),
                (@"\.InnerHTML\b", match => ".innerHTML"),
                (@"&ready", match => "\"DOMContentLoaded\""),
                (@"\bwait\s*\(", match => "setTimeout("),
                (@"xlet\s*{([^}]*)}", ReplaceXlet),
                (@"xset\((.*?)\)\s*{([^}]*)}", ReplaceXset)
            };

            foreach (var (pattern, replacement) in patterns)
            {
                code = Regex.Replace(
                    code,
                    pattern,
                    new MatchEvaluator(replacement),
                    RegexOptions.Singleline
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

        private static void ValidateGeneratedECMA(string jsCode, bool api = false)
        {
            try
            {
                var parser = new JavaScriptParser();
                parser.ParseScript(jsCode);
            }
            catch (ParserException ex)
            {
                var lineNumber = ex.LineNumber;
                var message = ex.Description;

                var identifierCode = Guid.NewGuid().ToString("N")[..5];
                var tempPath = Path.GetTempPath();
                var tempFilePath = Path.Combine(tempPath, $"{identifierCode}-debug.js");

                File.WriteAllText(tempFilePath, jsCode);

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true
                    }
                );
                string totalMessage =
                    $"ECMA validation error at line {lineNumber}, "
                    + $"column {ex.Column}: {message}"
                    + $"\nError identifier: {identifierCode}";
                if (api)
                {
                    totalMessage += $"\nBad generated code: {GetLine(jsCode, lineNumber)}";
                }
                Logger.RaiseProblem(message: totalMessage, fatal: true, colored: !api);
            }
        }

        private static string GetLine(string code, int lineNumber)
        {
            using var reader = new StringReader(code);
            string? line;
            int currentLine = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (currentLine == lineNumber - 1)
                {
                    return line;
                }
                currentLine++;
            }
            return string.Empty;
        }

        [GeneratedRegex(@"import\s+(\w+)\s*;?")]
        private static partial Regex ImportPatternRegex();

        [GeneratedRegex(@"rhtml\(""(.*?)""\)")]
        private static partial Regex TemplatePatternRegex();
    }
}
