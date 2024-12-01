using Irony.Parsing;
using System.Collections.Generic;

namespace CustomParser
{
    public class SimpleGrammar : Grammar
    {
        public SimpleGrammar()
        {
            LanguageFlags = LanguageFlags.CreateAst;

            // Terminals
            var identifier = new IdentifierTerminal("identifier");
            identifier.AstConfig.NodeType = typeof(IdentifierNode);

            var number = TerminalFactory.CreateCSharpNumber("number");
            number.AstConfig.NodeType = typeof(NumberNode);

            var charLiteral = new RegexBasedTerminal("charLiteral", @"'[^']'");
            charLiteral.AstConfig.NodeType = typeof(CharLiteralNode);

            var lessThan = ToTerm("<", "lessThan");
            var equals = ToTerm("=", "equals");

            var printKeyword = ToTerm("print", "printKeyword");
            var ifKeyword = ToTerm("if", "ifKeyword");
            var elseKeyword = ToTerm("else", "elseKeyword");
            var lBrace = ToTerm("{", "lBrace");
            var rBrace = ToTerm("}", "rBrace");
            var lParen = ToTerm("(", "lParen");
            var rParen = ToTerm(")", "rParen");
            var semicolon = ToTerm(";", "semicolon");

            // Non-terminals
            var expr = new NonTerminal("expr", typeof(ExpressionNode));
            var stmt = new NonTerminal("stmt", typeof(StatementNode));
            var block = new NonTerminal("block", typeof(BlockNode));
            var program = new NonTerminal("program", typeof(ProgramNode));
            var optionalElse = new NonTerminal("optionalElse");

            // Rulles
            expr.Rule = identifier + equals + number
                      | identifier + lessThan + number;

            stmt.Rule = ifKeyword + expr + block + optionalElse
                      | printKeyword + lParen + (identifier | charLiteral) + rParen + semicolon
                      | expr + semicolon;

            optionalElse.Rule = Empty | elseKeyword + block;

            block.Rule = lBrace + MakeStarRule(block, stmt) + rBrace;

            program.Rule = block;

           
            Root = program;

         
            MarkPunctuation("=", "if", "else", "{", "}", "(", ")", ";", "<");
        }

        public class ProgramNode
        {
            public BlockNode Block { get; set; }

            public ProgramNode(ParseTreeNode node)
            {
                Block = new BlockNode(node.ChildNodes[0]);
            }

            public override string ToString()
            {
                return "ProgramNode";
            }
        }

        public class BlockNode
        {
            public List<StatementNode> Statements { get; set; } = new List<StatementNode>();

            public BlockNode(ParseTreeNode node)
            {
                foreach (var child in node.ChildNodes)
                {
                    Statements.Add(new StatementNode(child));
                }
            }

            public override string ToString()
            {
                return "BlockNode";
            }
        }

        public class StatementNode
        {
            public string StatementType { get; set; }
            public ExpressionNode Expression { get; set; }
            public BlockNode IfBlock { get; set; }
            public BlockNode ElseBlock { get; set; }
            public object PrintValue { get; set; }

            public StatementNode(ParseTreeNode node)
            {
                if (node.ChildNodes[0].Term.Name == "ifKeyword")
                {
                    StatementType = "if";
                    Expression = new ExpressionNode(node.ChildNodes[1]);
                    IfBlock = new BlockNode(node.ChildNodes[2]);

                    if (node.ChildNodes.Count > 3 && node.ChildNodes[3].ChildNodes.Count > 0)
                        ElseBlock = new BlockNode(node.ChildNodes[3].ChildNodes[1]);
                }
                else if (node.ChildNodes[0].Term.Name == "printKeyword")
                {
                    StatementType = "print";
                    PrintValue = node.ChildNodes[2].Token.Value;
                }
                else
                {
                    StatementType = "expr";
                    Expression = new ExpressionNode(node.ChildNodes[0]);
                }
            }

            public override string ToString()
            {
                return $"StatementNode ({StatementType})";
            }
        }

        public class ExpressionNode
        {
            public string Identifier { get; set; }
            public string Operator { get; set; }
            public object Value { get; set; }

            public ExpressionNode(ParseTreeNode node)
            {
                Identifier = node.ChildNodes[0].FindTokenAndGetText();
                Operator = node.ChildNodes[1].FindTokenAndGetText();
                Value = node.ChildNodes[2].FindTokenAndGetText();
            }

            public override string ToString()
            {
                return $"ExpressionNode ({Identifier} {Operator} {Value})";
            }
        }

        public class IdentifierNode
        {
            public string Name { get; set; }

            public IdentifierNode(ParseTreeNode node)
            {
                Name = node.Token.Text;
            }

            public override string ToString()
            {
                return $"IdentifierNode ({Name})";
            }
        }

        public class NumberNode
        {
            public int Value { get; set; }

            public NumberNode(ParseTreeNode node)
            {
                int.TryParse(node.Token.Text, out int value);
                Value = value;
            }

            public override string ToString()
            {
                return $"NumberNode ({Value})";
            }
        }

        public class CharLiteralNode
        {
            public char Value { get; set; }

            public CharLiteralNode(ParseTreeNode node)
            {
                Value = node.Token.Text[1]; 
            }

            public override string ToString()
            {
                return $"CharLiteralNode ('{Value}')";
            }
        }
    }
}
