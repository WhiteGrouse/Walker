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
            var tree = parser.Parse("_Z1f2ab");
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

            var SeqId = new RegexBasedTerminal("seq-id", "[0-9A-Z]+");

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

            var TemplateArg = new NonTerminal("template-arg");
            var ExprPrimary = new NonTerminal("expr-primary");

            var BracedExpression = new NonTerminal("braced-expression");
            var Initializer = new NonTerminal("initializer");
            var FunctionParam = new NonTerminal("function-param");
            var UnresolvedName = new NonTerminal("unresolved-name");
            var BaseUnresolvedName = new NonTerminal("base-unresolved-name");
            var UnresolvedQualifierLevel = new NonTerminal("unresolved-qualifier-level");
            var SimpleId = new NonTerminal("simple-id");
            var DestructorName = new NonTerminal("destructor-name");
            var Float = new NonTerminal("float");

            var Discriminator = new NonTerminal("discriminator");

            var ClosureTypeName = new NonTerminal("closure-type-name");
            var LambdaSig = new NonTerminal("lambda-sig");

            var SourceNameList = new NonTerminal("source-name_plus");
            SourceNameList.Rule = MakePlusRule(SourceNameList, SourceNameL);
            var ExtendedQualifierList = new NonTerminal("extended-qualifier_star");
            ExtendedQualifierList.Rule = MakeStarRule(ExtendedQualifierList, ExtendedQualifier);
            var TypeList = new NonTerminal("type_plus");
            TypeList.Rule = MakePlusRule(TypeList, Type);
            var TemplateArgPlusList = new NonTerminal("template-arg_plus");
            TemplateArgPlusList.Rule = MakePlusRule(TemplateArgPlusList, TemplateArg);
            var TemplateArgStarList = new NonTerminal("template-arg_star");
            TemplateArgStarList.Rule = MakeStarRule(TemplateArgStarList, TemplateArg);

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
            UnqualifiedName.Rule =
                (OperatorName + AbiTags.Q())
                | CtorDtorName
                | SourceNameL
                | UnnamedTypeName
                | (ToTerm("DC") + SourceNameList + ToTerm("E"));
            AbiTags.Rule = MakeStarRule(AbiTags, AbiTag);
            AbiTag.Rule = ToTerm("B") + SourceNameL;
            OperatorName.Rule =
                ToTerm("nw")
                | ToTerm("na")
                | ToTerm("dl")
                | ToTerm("da")
                | ToTerm("ps")
                | ToTerm("ng")
                | ToTerm("ad")
                | ToTerm("de")
                | ToTerm("co")
                | ToTerm("pl")
                | ToTerm("mi")
                | ToTerm("ml")
                | ToTerm("dv")
                | ToTerm("rm")
                | ToTerm("an")
                | ToTerm("or")
                | ToTerm("eo")
                | ToTerm("aS")
                | ToTerm("pL")
                | ToTerm("mI")
                | ToTerm("mL")
                | ToTerm("dV")
                | ToTerm("rM")
                | ToTerm("aN")
                | ToTerm("oR")
                | ToTerm("eO")
                | ToTerm("ls")
                | ToTerm("rs")
                | ToTerm("lS")
                | ToTerm("rS")
                | ToTerm("eq")
                | ToTerm("ne")
                | ToTerm("lt")
                | ToTerm("gt")
                | ToTerm("le")
                | ToTerm("ge")
                | ToTerm("ss")
                | ToTerm("nt")
                | ToTerm("aa")
                | ToTerm("oo")
                | ToTerm("pp")
                | ToTerm("mm")
                | ToTerm("cm")
                | ToTerm("pm")
                | ToTerm("pt")
                | ToTerm("cl")
                | ToTerm("ix")
                | ToTerm("qu")
                | (ToTerm("cv") + Type)
                | (ToTerm("li") + SourceNameL)
                | (ToTerm("v") + (ToTerm("0") | ToTerm("1") | ToTerm("2") | ToTerm("3") | ToTerm("4") | ToTerm("5") | ToTerm("6") | ToTerm("7") | ToTerm("8") | ToTerm("9")) + SourceNameL);
            CallOffset.Rule =
                (ToTerm("h") + NVOffset + ToTerm("_"))
                | (ToTerm("v") + VOffset + ToTerm("_"));
            NVOffset.Rule = NumberL;
            VOffset.Rule = NumberL;
            CtorDtorName.Rule =
                ToTerm("C1")
                | ToTerm("C2")
                | ToTerm("C3")
                | ToTerm("D0")
                | ToTerm("D1")
                | ToTerm("D2");
            Type.Rule =
                BuiltinType
                | QualifiedType
                | FunctionType
                | ClassEnumType
                | ArrayType
                | PointerToMemberType
                | TemplateParam
                | (TemplateTemplateParam + TemplateArgs)
                | Decltype
                | (ToTerm("P") + Type)
                | (ToTerm("R") + Type)
                | (ToTerm("O") + Type)
                | (ToTerm("C") + Type)
                | (ToTerm("G") + Type)
                | Substitution
                | (ToTerm("Dp") + Type);
            QualifiedType.Rule = Qualifiers + Type;
            Qualifiers.Rule = ExtendedQualifierList + CVQualifiers;
            ExtendedQualifier.Rule = ToTerm("U") + SourceNameL + TemplateArgs.Q();
            CVQualifiers.Rule = ToTerm("r").Q() + ToTerm("V").Q() + ToTerm("K").Q();
            RefQualifier.Rule = ToTerm("R") | ToTerm("O");
            BuiltinType.Rule =
                ToTerm("v")
                | ToTerm("w")
                | ToTerm("b")
                | ToTerm("c")
                | ToTerm("a")
                | ToTerm("h")
                | ToTerm("s")
                | ToTerm("t")
                | ToTerm("i")
                | ToTerm("j")
                | ToTerm("l")
                | ToTerm("m")
                | ToTerm("x")
                | ToTerm("y")
                | ToTerm("n")
                | ToTerm("o")
                | ToTerm("f")
                | ToTerm("d")
                | ToTerm("e")
                | ToTerm("g")
                | ToTerm("z")
                | ToTerm("Dd")
                | ToTerm("De")
                | ToTerm("Df")
                | ToTerm("Dh")
                | (ToTerm("DF") + NumberL + ToTerm("_"))
                | ToTerm("Di")
                | ToTerm("Ds")
                | ToTerm("Da")
                | ToTerm("Dc")
                | ToTerm("Dn")
                | (ToTerm("u") + SourceNameL);
            FunctionType.Rule = CVQualifiers.Q() + ExceptionSpec.Q() + ToTerm("Dx").Q() + ToTerm("F") + ToTerm("Y").Q() + BareFunctionType + RefQualifier.Q() + ToTerm("E");
            BareFunctionType.Rule = TypeList;
            ExceptionSpec.Rule =
                ToTerm("Do")
                | (ToTerm("DO") + Expression + ToTerm("E"))
                | (ToTerm("Dw") + TypeList + ToTerm("E"));
            Decltype.Rule =
                (ToTerm("Dt") + Expression + ToTerm("E"))
                | (ToTerm("DT") + Expression + ToTerm("E"));
            ClassEnumType.Rule =
                Name
                | (ToTerm("Ts") + Name)
                | (ToTerm("Tu") + Name)
                | (ToTerm("Te") + Name);
            UnnamedTypeName.Rule = ToTerm("Ut") + NumberL.Q() + ToTerm("_");
            ArrayType.Rule =
                (ToTerm("A") + NumberL + ToTerm("_") + Type)
                | (ToTerm("A") + Expression.Q() + ToTerm("_") + Type);
            PointerToMemberType.Rule = ToTerm("M") + Type + Type;
            TemplateParam.Rule = ToTerm("T") + NumberL.Q() + ToTerm("_");
            TemplateTemplateParam.Rule = TemplateParam | Substitution;
            FunctionParam.Rule =
                (ToTerm("fp") + CVQualifiers + ToTerm("_"))
                | (ToTerm("fp") + CVQualifiers + NumberL + ToTerm("_"))
                | (ToTerm("fL") + NumberL + ToTerm("p") + CVQualifiers + ToTerm("_"))
                | (ToTerm("fL") + NumberL + ToTerm("p") + CVQualifiers + NumberL + ToTerm("_"));
            TemplateArgs.Rule = ToTerm("I") + TemplateArgPlusList + ToTerm("E");
            TemplateArg.Rule =
                Type
                | (ToTerm("X") + Expression + ToTerm("E"))
                | ExprPrimary
                | (ToTerm("J") + TemplateArgStarList + ToTerm("E"));


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
