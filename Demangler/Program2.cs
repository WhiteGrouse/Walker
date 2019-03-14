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

            var MangledName = new NonTerminal("mangled-name");
            var Encoding = new NonTerminal("encoding");
            var BareFunctionType = new NonTerminal("bare-function-type");
            var SpecialName = new NonTerminal("special-name");

            var Name = new NonTerminal("name");
            var NestedName = new NonTerminal("nested-name");
            var UnscopedName = new NonTerminal("unscoped-name");
            var UnscopedTemplateName = new NonTerminal("unscoped-template-name");
            var TemplateArgs = new NonTerminal("template-args");
            var LocalName = new NonTerminal("local-name");
            var UnqualifiedName = new NonTerminal("unqualified-name");
            var Substitution = new NonTerminal("substitution");

            var CVQualifiers = new NonTerminal("CV-qualifiers");
            var RefQualifier = new NonTerminal("ref-qualifier");
            var Prefix = new NonTerminal("prefix");
            var TemplatePrefix = new NonTerminal("template-prefix");
            var TemplateParam = new NonTerminal("template-param");
            var Decltype = new NonTerminal("decltype");
            var DataMemberPrefix = new NonTerminal("data-member-prefix");
            var OperatorName = new NonTerminal("operator-name");
            var AbiTags = new NonTerminal("abi-tags");
            var CtorDtorName = new NonTerminal("ctor-dtor-name");
            var UnnamedTypeName = new NonTerminal("unnamed-type-name");

            var AbiTag = new NonTerminal("abi-tag");

            var SeqId = new NonTerminal("seq-id");

            var Type = new NonTerminal("type");
            var CallOffset = new NonTerminal("call-offset");
            var NVOffset = new NonTerminal("nv-offet");
            var VOffset = new NonTerminal("v-offset");

            var BuiltinType = new NonTerminal("builtin-type");
            var QualifiedType = new NonTerminal("qualified-type");
            var FunctionType = new NonTerminal("function-type");
            var ClassEnumType = new NonTerminal("class-enum-type");
            var ArrayType = new NonTerminal("array-type");
            var PointerToMemberType = new NonTerminal("pointer-to-member-type");
            var TemplateTemplateParam = new NonTerminal("template-template-param");

            var Qualifiers = new NonTerminal("qualifiers");
            var ExtendedQualifier = new NonTerminal("extended-qualifier");

            var ExceptionSpec = new NonTerminal("exception-spec");
            var Expression = new NonTerminal("expression");

            var ExprPrimary = new NonTerminal("expr-primary");

            var BracedExpression = new NonTerminal("braced-expression");
            var Initializer = new NonTerminal("initializer");
            var UnresolvedName = new NonTerminal("unresolved-name");
            var BaseUnresolvedName = new NonTerminal("base-unresolved-name");
            var UnresolvedQualifierLevel = new NonTerminal("unresolved-qualifier-level");
            var SimpleId = new NonTerminal("simple-id");
            var DestructorName = new NonTerminal("destructor-name");
            var Float = new NonTerminal("float");

            var Discriminator = new NonTerminal("discriminator");

            var ClosureTypeName = new NonTerminal("closure-type-name");
            var LambdaSig = new NonTerminal("lambda-sig");

            Name.Rule =
                  NestedName
                | UnscopedName
                | (UnscopedTemplateName + TemplateArgs)
                | LocalName;
            UnscopedName.Rule =
                  UnqualifiedName
                | (ToTerm("St") + UnqualifiedName);
            UnscopedTemplateName.Rule =
                  UnscopedName
                | Substitution;
            NestedName.Rule =
                  (ToTerm("N") + CVQualifiers.Q() + RefQualifier.Q() + Prefix + UnqualifiedName + ToTerm("E"))
                | (ToTerm("N") + CVQualifiers.Q() + RefQualifier.Q() + TemplatePrefix + TemplateArgs + ToTerm("E"));
            Prefix.Rule =
                UnqualifiedName
                | (Prefix + UnqualifiedName)
                | (TemplatePrefix + TemplateArgs)
                | TemplateParam
                | Decltype
                | (Prefix + DataMemberPrefix)
                | Substitution;
            TemplatePrefix.Rule =
                UnqualifiedName
                | (Prefix + UnqualifiedName)
                | TemplateParam
                | Substitution;

            MangledName.Rule = ToTerm("_Z") + Encoding;

            Root = MangledName;
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
