using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Demangler
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");
                string mangled = Console.ReadLine();
                if (!mangled.StartsWith("_Z"))
                {
                    Console.WriteLine("Please enter mangled string.");
                    continue;
                }

                Console.WriteLine(new FunctionDemangler(mangled).Demangled.GetNameString());
                Console.WriteLine();
            }
        }
    }

    class FunctionDemangler
    {
        public string MangledText { get; }
        protected int Index = 0;

        public S_Component Demangled { get; private set; }

        public FunctionDemangler(string mangled)
        {
            MangledText = mangled;
            if (!CheckChar('_') || !CheckChar('Z'))
                throw new ArgumentException("it is not mangled text.");
            Parse();
        }

        protected void Parse()
        {
            Demangled = ReadNameOrNested();
        }

        protected char ReadChar()
        {
            return MangledText[Index++];
        }
        protected bool CheckChar(char c)
        {
            return ReadChar() == c;
        }

        protected bool IsTermination()
        {
            return MangledText.Length <= Index;
        }

        protected char Peek => MangledText[Index];
        protected char PeekNext => MangledText[Index + 1];

        protected int ReadNumber()
        {
            int num = 0;
            while (char.IsDigit(Peek)) num += num * 10 + ReadChar() - '0';
            return num;

        }
        protected string ReadSourceName()
        {
            int length = ReadNumber();
            var source_name = MangledText.Substring(Index, length);
            Index += length;
            return source_name;
        }

        protected S_Name ReadName()
        {
            var source_name = ReadSourceName();
            S_Template template = null;
            if(Peek == 'I')
                template = ReadTemplate();
            return new S_Name()
            {
                SourceName = source_name,
                Template = template
            };
        }

        protected S_Component ReadNameOrNested()
        {
            if (char.IsDigit(Peek))
            {
                return ReadName();
            }
            else if(Peek == 'N')
            {
                return ReadNested();
            }
            else
            {
                return ReadSpecialName();
            }
        }

        protected S_Name ReadSpecialName()
        {
            throw new NotImplementedException();
        }

        protected S_Nested ReadNested()
        {
            if (!CheckChar('N'))
                throw new Exception("it is not nested...");
            var names = new List<S_Name>();
            do
            {
                var name = ReadName();
                names.Add(name);
            } while (Peek != 'E');
            CheckChar('E');
            return new S_Nested { Names = names };
        }

        protected S_Template ReadTemplate()
        {
            if (!CheckChar('I'))
                throw new Exception("it is not template...");
            var args = new List<S_Component>();
            do
            {
                var arg = ReadNameOrNested();
                args.Add(arg);
            } while (Peek != 'E');
            CheckChar('E');
            return new S_Template { Arguments = args };
        }
    }

    interface S_Component
    {
        string GetNameString();
    }

    class S_Name : S_Component
    {
        public string SourceName { get; set; }

        public S_Template Template { get; set; } = null;

        public string GetNameString()
        {
            var builder = new StringBuilder();
            builder.Append(SourceName);
            if(Template != null)
            {
                builder.Append("<");
                var comma = false;
                foreach(var arg in Template.Arguments)
                {
                    if (comma)
                        builder.Append(", ");
                    builder.Append(arg.GetNameString());
                    comma = true;
                }
                builder.Append(">");
            }
            return builder.ToString();
        }
    }

    class S_Nested : S_Component
    {
        public IEnumerable<S_Name> Names { get; set; }

        public string GetNameString()
        {
            return string.Join("::", Names.Select(name => name.GetNameString()));
        }
    }

    class S_Template
    {
        public IEnumerable<S_Component> Arguments { get; set; }
    }
}
