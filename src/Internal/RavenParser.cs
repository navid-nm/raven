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

            var sb = new StringBuilder(code);

            sb.Replace("fn(", "function(");
            sb.Replace("print(", "console.log(");
            sb.Replace("warn(", "console.error(");
            sb.Replace("} die (", "} catch (");
            sb.Replace("} die(", "} catch (");
            sb.Replace(").die(", ").catch(");
            sb.Replace(".str()", ".toString()");
            sb.Replace(".getnode(", ".getElementById(");
            sb.Replace(".makenode(", ".createElement(");
            sb.Replace(".addsubnode(", ".appendChild(");
            sb.Replace("(@dom_loaded", "(\"DOMContentLoaded\"");

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

        private string ReadAndTranspileImport(string importPath)
        {
            var fullPath = Path.Combine(_basePath, importPath + ".raven");
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Imported file '{importPath}' not found.");
            }
            var importCode = File.ReadAllText(fullPath);
            var importParser = new RavenParser(importCode, _basePath);
            return importParser.Transpile();
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

        [GeneratedRegex(@"import\s+""(.*?)""\s*;?")]
        private static partial Regex ImportPatternRegex();
    }
}
