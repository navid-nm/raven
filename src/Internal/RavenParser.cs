using System.Text.RegularExpressions;
using Esprima;
using Raven.Data;

namespace Raven.Internal
{
    public partial class RavenParser(string sourceCode, string basePath)
    {
        private readonly string _sourceCode = sourceCode;
        private readonly string _basePath = basePath;
        private readonly Dictionary<string, string> _typeHints = [];
        private readonly Dictionary<string, string> _abbreviations = [];

        public string Transpile()
        {
            var code = HandleImports(_sourceCode);

            if (code == null)
            {
                return string.Empty;
            }
            if (code.Contains(Common.RavenRHTMLLiteral))
            {
                code = HandleTemplates(code);
                code = HandleTemplateLiterals(code);
            }

            code = ExtractAndProcessTypeHints(code);
            code = ExtractAndProcessAbbreviations(code);
            code = ReplaceContextAware(code);
            code = Common.StrictLiteral + code;

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
            // Replace periods with directory separators
            var relativePath = importPath.Replace('.', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_basePath, relativePath + ".rnm");

            if (!File.Exists(fullPath))
            {
                Logger.RaiseProblem($"Imported file '{importPath}' not found at '{fullPath}'.");
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
                    // Placeholder as type is currently not used further
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

            // Process inline abbreviations
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
                // Remove the inline abbrev statement
                code = code.Replace(match.Value, "");
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
                (@"\braw\((.*?)\)", match => match.Groups[1].Value),
                (
                    @"\bfn\s+(\w+)\s*(\([\w\s,]*\))?\s*=\s*(.+)",
                    match =>
                    {
                        var funcName = match.Groups[1].Value;
                        var parameters = match.Groups[2].Value;
                        var body = match.Groups[3].Value;
                        return $"function {funcName}{parameters} {{ return {body}; }}";
                    }
                ),
                (@"\bfn(\*)?(?=\s|\()", match => "function" + (match.Groups[1].Value ?? "")),
                (@"\bsay\.table\s*\(", match => "console.table("),
                (@"\bsay\.group\s*\(", match => "console.group("),
                (@"\bsay\.clear\s*\(", match => "console.clear("),
                (@"\bsay\.trace\s*\(", match => "console.trace("),
                (@"\bsay\.time\s*\(", match => "console.time("),
                (@"\bsay\.tend\s*\(", match => "console.timeEnd("),
                (@"\bsay\.tlog\s*\(", match => "console.timeLog("),
                (@"\bsay\.tstamp\s*\(", match => "console.timeStamp("),
                (@"\bsay\s*\(", match => "console.log("),
                (@"\buse\s*\(", match => "require("),
                (@"\bwarn\s*\(", match => "console.error("),
                (@"\bconsole\.console\.error\s*\(", match => "console.warn("),
                (@"\bsay\.amber\s*\(", match => "console.warn("),
                (@"\bsay\.", match => "console."),
                (@"}\s*die\s*\(", match => "} catch ("),
                (@"\)\s*\.die\s*\(", match => ").catch("),
                (@"\bdoc\.", match => "document."),
                (@"\bwin\.", match => "window."),
                (@"\bonready\s*\(", match => "document.addEventListener(\"DOMContentLoaded\","),
                (@"\.str\.", match => ".toString."),
                (@"\.str\s*\(\)", match => ".toString()"),
                (@"\.num\s*\(\)", match => ".toNumber()"),
                (@"\bdocument\.get\s*\(", match => "document.getElementById("),
                (@"\bwin\.onload\s*\(", match => "window.onload"),
                (@"\.listen\s*\(", match => ".addEventListener("),
                (@"\.unlisten\s*\(", match => ".removeEventListener("),
                (@"\bdocument\.make\s*\(", match => "document.createElement("),
                (@"\.put\s*\(", match => ".appendChild("),
                (@"&ready", match => "\"DOMContentLoaded\""),
                (@"\bwait\s*\(", match => "setTimeout("),
                (@"xlet\s*{([^}]*)}", ReplaceXlet),
                (@"xset\((.*?)\)\s*{([^}]*)}", ReplaceXset),
                (@"xval\s*{([^}]*)}", ReplaceXval),
                (@"xvar\s*{([^}]*)}", ReplaceXvar),
                (@"val\b", match => "const"),
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
                (@"\belif\b", match => "else if"),
                (@"\bend\s*\(\s*(\d+)\s*\)", match => $"process.exit({match.Groups[1].Value});"),
                // Matches end not preceded by dot and not followed by =
                (@"\b(?<!\.)end\b(?=\s|$|;)(?!\s*=)", match => "process.exit(0);"),
                (@"\bexpose\s*{([^}]*)}", match => $"module.exports = {{{match.Groups[1].Value}}}"),
                // New pattern for expose funcname
                (@"\bexpose\s+(\w+)", match => $"module.exports = {match.Groups[1].Value}"),
                // Replace '==' with '===' ensuring no '===' or '!=='
                (@"(?<![!=])==(?!=)", match => "==="),
                // Replace '!=' with '!==' ensuring no '!=='
                (@"(?<![=!])!=(?!=)", match => "!=="),
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

            // Remove use x* statements
            code = UseWithoutParenthesesRegex()
                .Replace(
                    code,
                    match =>
                    {
                        var moduleName = match.Groups[1].Value;
                        return $"const {{ {moduleName} }} = require(\"{moduleName}\");";
                    }
                );

            // Fix for # placement in static async and similar issues
            code = StaticRegex().Replace(code, "static $1#$2");
            code = StaticAsyncRegex().Replace(code, "async $1#$2");

            // Fix for space between # and variable name
            code = PrivateMemberSpaceRegex().Replace(code, "#$1");

            // Fix for private member references
            if (code.Contains("closed"))
            {
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

        [GeneratedRegex(@"import\s+([\w\.]+)\s*;?")]
        private static partial Regex ImportPatternRegex();

        [GeneratedRegex(@"use\s+(\w+)\s*")]
        private static partial Regex UseWithoutParenthesesRegex();

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
