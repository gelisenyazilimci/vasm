using System;
using System.IO;
using Irony.Parsing;

namespace CustomParser
{
    class Program
    {
        public static void Main(string[] args)
        {
            string test = ReadTest();
            ParseAndPrintAst(test);
        }

        static string ReadTest()
        {
            string filePath = "test.txt";
            string result = "";

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        result += line + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return result;
        }

        private static void ParseAndPrintAst(string code)
        {
            var grammar = new SimpleGrammar();
            var parser = new Parser(grammar);

            var parseTree = parser.Parse(code);

            if (parseTree.HasErrors())
            {
                Console.WriteLine("ERROR!");
                foreach (var error in parseTree.ParserMessages)
                {
                    Console.WriteLine($"ERROR message: {error.Message} - Locations: Line {error.Location.Line}, column {error.Location.Column}");
                }
                return;
            }

            Console.WriteLine("AST:");
            PrintParseTree(parseTree.Root);
        }

        static void PrintParseTree(ParseTreeNode node, string indent = "")
        {
            Console.WriteLine($"{indent}{node.Term.Name} ({node.Token?.ValueString ?? "No Value"})");
            foreach (var child in node.ChildNodes)
            {
                PrintParseTree(child, indent + "  ");
            }
        }
    }
}
