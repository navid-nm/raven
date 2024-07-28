using System.Text.RegularExpressions;
using Esprima;

namespace Raven.Internal
{
    public partial class RavenParser(string sourceCode, string basePath)
    {
        private readonly string _sourceCode = sourceCode;
        private readonly string _basePath = basePath;

        public string Transpile()
        {
            var code = HandleImports(_sourceCode);
            code = HandleTemplates(code);

            // Handle template literals for multi-line strings
            code = HandleTemplateLiterals(code);

            // Context-aware replacements using regular expressions
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

        private string ReplaceContextAware(string code)
        {
            var patterns = new (string pattern, string replacement)[]
            {
                (@"\bfn\s*(\w*)\s*\(", "function $1("),
                (@"\bprint\s*\(", "console.log("),
                (@"\bwarn\s*\(", "console.error("),
                (@"}\s*die\s*\(", "} catch ("),
                (@"}\s*die\s*\(", "} catch ("),
                (@"\)\s*\.die\s*\(", ").catch("),
                (@"\bdoc\.", "document."),
                (@"\bonready\s*\(", "document.addEventListener(\"DOMContentLoaded\","),
                (@"\bonready\s*\(", "document.addEventListener(\"DOMContentLoaded\","),
                (@"\.str\s*\(\)", ".toString()"),
                (@"\bdocument\.get\s*\(", "document.getElementById("),
                (@"\bdocument\.make\s*\(", "document.createElement("),
                (@"\bdocument\.listen\s*\(", "document.addEventListener("),
                (@"\.AddSub\s*\(", ".appendChild("),
                (@"\.ClassName\b", ".className"),
                (@"\.InnerHTML\b", ".innerHTML"),
                (@"&ready", "\"DOMContentLoaded\"")
            };

            foreach (var (pattern, replacement) in patterns)
            {
                code = Regex.Replace(code, pattern, replacement);
            }

            return code;
        }

        private static void ValidateGeneratedECMA(string jsCode, bool api = false)
        {
            try
            {
                var parser = new JavaScriptParser();
                var program = parser.ParseScript(jsCode);
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
            using (var reader = new StringReader(code))
            {
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
            }
            return string.Empty;
        }

        [GeneratedRegex(@"import\s+(\w+)\s*;?")]
        private static partial Regex ImportPatternRegex();

        [GeneratedRegex(@"rhtml\(""(.*?)""\)")]
        private static partial Regex TemplatePatternRegex();
    }
}
