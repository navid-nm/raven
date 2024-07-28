using System.Text;
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

            var sb = new StringBuilder(code);

            sb.Replace("fn(", "function(");
            sb.Replace("print(", "console.log(");
            sb.Replace("warn(", "console.error(");
            sb.Replace("} die (", "} catch (");
            sb.Replace("} die(", "} catch (");
            sb.Replace(").die(", ").catch(");
            sb.Replace("doc.", "document.");
            sb.Replace("onready(", "document.addEventListener(\"DOMContentLoaded\",");
            sb.Replace("onready (", "document.addEventListener(\"DOMContentLoaded\",");
            sb.Replace(".str()", ".toString()");
            sb.Replace("document.get(\"", "document.getElementById(\"");
            sb.Replace("document.make(\"", "document.createElement(\"");
            sb.Replace("document.listen(", "document.addEventListener(");
            sb.Replace(".AddSub(", ".appendChild(");
            sb.Replace(".ClassName", ".className");
            sb.Replace(".InnerHTML", ".innerHTML");
            sb.Replace("&ready", "\"DOMContentLoaded\"");

            // Handle template literals for multi-line strings
            sb = new StringBuilder(HandleTemplateLiterals(sb.ToString()));

            var transpiledCode = sb.ToString();
            ValidateGeneratedECMA(transpiledCode, Glob.IsApi);

            return transpiledCode;
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
