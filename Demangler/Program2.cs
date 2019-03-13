using Irony.Parsing;
using System;

namespace Demangler
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = new ManglingGrammar();
            var parser = new Parser(grammar);
            var tree = parser.Parse("_Z1f");
            Console.WriteLine();
        }
    }

    [Language("ManglingGrammar")]
    class ManglingGrammar : Grammar
    {
        public ManglingGrammar() : base(true)
        {
            var NumberL = new NumberLiteral("number", NumberOptions.IntOnly | NumberOptions.AllowLetterAfter);
            var SourceNameL = new StringWithLengthLiteral("source-name");
            var Identifier = new IdentifierTerminal("identifier");

            var Mangled = new NonTerminal("mangled-name");
            var Encoding = new NonTerminal("encoding");
            var Name = new NonTerminal("name");
            var FunctionName = new NonTerminal("function name");
            var BareFunctionType = new NonTerminal("bare-function-type");
            var DataName = new NonTerminal("data name");
            var SpecialName = new NonTerminal("special-name");
            var NestedName = new NonTerminal("nested-name");
            var UnscopedName = new NonTerminal("unscoped-name");
            var UnscopedTemplateName = new NonTerminal("unscoped-template-name");
            var TemplateArgs = new NonTerminal("template-args");
            var LocalName = new NonTerminal("local-name");
            var UnqualifiedName = new NonTerminal("unqualified-name");
            var CVQualifiers = new NonTerminal("CV-qualifiers");
            var RefQualifier = new NonTerminal("ref-qualifier");
            var Prefix = new NonTerminal("prefix");
            var TemplatePrefix = new NonTerminal("template-prefix");
            var TemplateParam = new NonTerminal("template-param");
            var Decltype = new NonTerminal("decltype");
            var DataMemberPrefix = new NonTerminal("data-member-prefix");
            var OperatorName = new NonTerminal("operator-name");
            var AbiTags = new NonTerminal("abi-tags");
            var AbiTag = new NonTerminal("abi-tag");
            var CtorDtorName = new NonTerminal("ctor-dtor-name");
            var UnnamedTypeName = new NonTerminal("unnamed-type-name");
            var Type = new NonTerminal("type");
            
            Mangled.Rule = "_Z" + Encoding;

            Root = Mangled;
        }
    }

    class StringWithLengthLiteral : Terminal
    {
        public StringWithLengthLiteral(string name) : base(name) { }

        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            int? length_ = GetLength(source);
            if (length_ == null)
                return context.CreateErrorToken("invalid StringWithLengthLiteral");
            int length = length_.GetValueOrDefault();
            source.Position = source.PreviewPosition;
            source.PreviewPosition += length;
            string str = source.Text.Substring(source.Location.Position, length);

            return source.CreateToken(this.OutputTerminal);
        }

        private int? GetLength(ISourceStream source)
        {
            int length = 0;
            for (; !source.EOF() && char.IsDigit(source.PreviewChar); ++source.PreviewPosition)
                length = length * 10 + int.Parse(source.PreviewChar.ToString());
            if (length == 0 || source.PreviewPosition + length > source.Text.Length)
                return null;
            return length;
        }
    }
}
