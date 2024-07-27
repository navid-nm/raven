using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Raven.Internal
{
    public class RavenParser
    {
        private readonly string _sourceCode;

        public RavenParser(string sourceCode)
        {
            _sourceCode = sourceCode;
        }

        public string Transpile()
        {
            var sb = new StringBuilder(_sourceCode);

            // Replace function definitions
            sb.Replace("fn ", "function ");

            // Replace print statements
            sb.Replace("print(", "console.log(");

            // Replace print_error statements
            sb.Replace("print_error(", "console.error(");

            // Handle try...die
            sb.Replace("} die (", "} catch (");

            // Handle template literals for multi-line strings
            sb = new StringBuilder(HandleTemplateLiterals(sb.ToString()));

            return sb.ToString();
        }

        private string HandleTemplateLiterals(string code)
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
    }
}
