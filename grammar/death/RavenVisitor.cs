using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace RavenCompiler
{
    public class RavenVisitor : RavenBaseVisitor<object>
    {
        public override object VisitFunctionDef([NotNull] RavenParser.FunctionDefContext context)
        {
            // Handle function definition
            Console.WriteLine($"Function: {context.identifier().GetText()}");
            return base.VisitFunctionDef(context);
        }

        public override object VisitVariableDef([NotNull] RavenParser.VariableDefContext context)
        {
            // Handle variable definition
            Console.WriteLine(
                $"Variable: {context.identifier().GetText()} = {context.expression().GetText()}"
            );
            return base.VisitVariableDef(context);
        }

        public override object VisitImportStatement(
            [NotNull] RavenParser.ImportStatementContext context
        )
        {
            // Handle import statement
            Console.WriteLine($"Import: {context.identifier().GetText()}");
            return base.VisitImportStatement(context);
        }

        // Implement other visit methods as needed
    }
}
