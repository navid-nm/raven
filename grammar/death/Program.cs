using System;
using System.IO;
using Antlr4.Runtime;

namespace RavenCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check for correct usage
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RavenCompiler <input-file>");
                return;
            }

            // Read the input file
            var inputFilePath = args[0];
            var inputStream = new AntlrFileStream(inputFilePath);

            // Create a lexer and a parser
            var lexer = new RavenLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new RavenParser(commonTokenStream);

            // Parse the input and get the parse tree
            var context = parser.program();

            // Print the parse tree
            Console.WriteLine(context.ToStringTree(parser));

            // Create an instance of RavenVisitor
            var visitor = new RavenVisitor();

            // Visit the parse tree
            visitor.Visit(context);
        }
    }
}
