using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Esprima;

namespace Raven.Internal
{
    public partial class RavenParser
    {
        private readonly string _sourceCode;
        private readonly string _basePath;
        private readonly Dictionary<string, string> _typeHints = new();
        private readonly Dictionary<string, string> _abbreviations = new();

        public RavenParser(string sourceCode, string basePath)
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
            var fullPath = Path.Combine(_basePath, importPath + ".rnm");
            if (!File.Exists(fullPath))
            {
                Logger.RaiseProblem($"Imported file '{importPath}' not found.");
            }
            var importCode = File.ReadAllText(fullPath) + "\n";
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

            // Apply abbreviations
            foreach (var (abbrev, replacement) in _abbreviations)
            {
                // Use word boundaries to avoid partial matches
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
                (@"\bfn\s*(\w*)\s*\(", match => $"function {match.Groups[1].Value}("),
                (@"\bsay\s*\(", match => "console.log("),
                (@"\buse\s*\(", match => "require("),
                (@"\bwarn\s*\(", match => "console.error("),
                (@"}\s*die\s*\(", match => "} catch ("),
                (@"\)\s*\.die\s*\(", match => ").catch("),
                (@"\bdoc\.", match => "document."),
                (@"\bwin\.", match => "window."),
                (@"\bonready\s*\(", match => "document.addEventListener(\"DOMContentLoaded\","),
                (@"\.str\s*\(\)", match => ".toString()"),
                (@"\bdocument\.get\s*\(", match => "document.getElementById("),
                (@"\bwin\.onload\s*\(", match => "window.onload"),
                (@"\bdocument\.make\s*\(", match => "document.createElement("),
                (@"\bdocument\.listen\s*\(", match => "document.addEventListener("),
                (@"\.put\s*\(", match => ".appendChild("),
                (@"\.ClassName\b", match => ".className"),
                (@"\.InnerHTML\b", match => ".innerHTML"),
                (@"&ready", match => "\"DOMContentLoaded\""),
                (@"\bwait\s*\(", match => "setTimeout("),
                (@"xlet\s*{([^}]*)}", ReplaceXlet),
                (@"xset\((.*?)\)\s*{([^}]*)}", ReplaceXset),
                (@"xconst\s*{([^}]*)}", ReplaceXconst),
                (@"xvar\s*{([^}]*)}", ReplaceXvar),
                (@"\bclosed\s+stat\b", match => "static #"),
                (@"\bopen\s+stat\b", match => "static "),
                (@"\bclosed\s+async\b", match => "async #"),
                (@"\bopen\s+async\b", match => "async "),
                (@"\btmp\s+(\w+)\b", match => $"class {match.Groups[1].Value}"),
                (@"\bstat\s+(\w+)\b", match => $"static {match.Groups[1].Value}"),
                (@"\bmy\.", match => "this."),
                (
                    @"\binit\s*(\([^)]*\))?\s*{",
                    match =>
                        "constructor"
                        + (match.Groups[1].Value != string.Empty ? match.Groups[1].Value : "()")
                        + " {"
                ),
                (@"\belif\b", match => "else if")
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

            // Fix for # placement in static async and similar issues
            code = StaticRegex().Replace(code, "static $1#$2");
            code = StaticAsyncRegex().Replace(code, "async $1#$2");

            // Fix for space between # and variable name
            code = PrivateMemberSpaceRegex().Replace(code, "#$1");

            // Fix for private member references
            code = PrivateMemberReferenceRegex()
                .Replace(
                    code,
                    match =>
                    {
                        var member = match.Value;
                        if (member.StartsWith("this."))
                        {
                            return member.Replace("this.", "this.#");
                        }
                        return member;
                    }
                );

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

        private static string ReplaceXconst(Match match)
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

        [GeneratedRegex(@"static\s+#\s*(async\s+)?(\w+\s*\()", RegexOptions.Singleline)]
        private static partial Regex StaticRegex();

        [GeneratedRegex(@"async\s+#\s*(static\s+)?(\w+\s*\()", RegexOptions.Singleline)]
        private static partial Regex StaticAsyncRegex();

        [GeneratedRegex(@"#\s+(\w+)")]
        private static partial Regex PrivateMemberSpaceRegex();

        [GeneratedRegex(@"\b(this\.)\w+\b")]
        private static partial Regex PrivateMemberReferenceRegex();
    }
}
