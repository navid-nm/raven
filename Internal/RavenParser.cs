using System.Text.RegularExpressions;

namespace Raven.Internal
{
    public partial class RavenParser
    {
        private readonly string _sourceCode;

        public RavenParser(string sourceCode)
        {
            _sourceCode = sourceCode;
        }

        public string Transpile()
        {
            var ast = ParseSourceCode();
            var jsCode = GenerateJsCode(ast);
            return jsCode;
        }

        private RavenAst ParseSourceCode()
        {
            var lines = _sourceCode.Split('\n');
            var ast = new RavenAst();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("fn"))
                {
                    var funcDecl = ParseFunctionDeclaration(trimmedLine);
                    ast.Functions.Add(funcDecl);
                }
                else if (trimmedLine.StartsWith("print"))
                {
                    var printStmt = ParsePrintStatement(trimmedLine);
                    ast.Statements.Add(printStmt);
                }
            }

            return ast;
        }

        private static RavenFunction ParseFunctionDeclaration(string line)
        {
            var match = MyRegex().Match(line);
            if (!match.Success)
                throw new Exception("Invalid function declaration");

            var funcName = match.Groups[1].Value;
            var paramsStr = match.Groups[2].Value;
            var paramsa = paramsStr.Split(',').Select(p => p.Trim()).ToList();

            return new RavenFunction { Name = funcName, Parameters = paramsa };
        }

        private static RavenPrintStatement ParsePrintStatement(string line)
        {
            var match = MyRegex1().Match(line);
            if (!match.Success)
                throw new Exception("Invalid print statement");

            var expr = match.Groups[1].Value;
            return new RavenPrintStatement { Expression = expr };
        }

        private static string GenerateJsCode(RavenAst ast)
        {
            var jsCode = "";

            foreach (var func in ast.Functions)
            {
                jsCode += $"function {func.Name}({string.Join(", ", func.Parameters)}) {{\n";
                foreach (var stmt in ast.Statements)
                {
                    jsCode += $"  {stmt.Expression};\n";
                }
                jsCode += "}\n";
            }

            return jsCode;
        }

        [GeneratedRegex(@"fn\s+(\w+)\s*\((.*?)\)")]
        private static partial Regex MyRegex();

        [GeneratedRegex(@"print\((.*?)\)")]
        private static partial Regex MyRegex1();
    }
}
