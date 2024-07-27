using System.Text;
using System.Text.RegularExpressions;

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
            sb.Replace(".str()", ".toString()");
            sb.Replace(".nget(", ".getElementById(");
            sb.Replace(".nmake(", ".createElement(");
            sb.Replace(".listenf(", ".addEventListener(");
            sb.Replace(".AddSub(", ".appendChild(");
            sb.Replace(".ClassName", ".className");
            sb.Replace(".InnerHTML", ".innerHTML");
            sb.Replace("(__ready__", "(\"DOMContentLoaded\"");

            // Handle template literals for multi-line strings
            sb = new StringBuilder(HandleTemplateLiterals(sb.ToString()));

            return sb.ToString();
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

        [GeneratedRegex(@"import\s+(\w+)\s*;?")]
        private static partial Regex ImportPatternRegex();

        [GeneratedRegex(@"rhtml\(""(.*?)""\)")]
        private static partial Regex TemplatePatternRegex();
    }
}
