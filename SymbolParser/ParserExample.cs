using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var tokens = Parser.Parse("void std::vector<KeyboardKeyBinding, std::allocator<KeyboardKeyBinding> >::_M_emplace_back_aux<char const (&) [13], Keyboard::{unnamed type#1}, FocusImpact>(char const (&) [13], Keyboard::{unnamed type#1}&&, FocusImpact&&)");
            Console.WriteLine();
        }
    }

    class Parser
    {
        public string Demangled { get; }

        private int Current = 0;
        private List<TOKEN> Tokens = new List<TOKEN>();

        private Parser(string demangled) => Demangled = demangled;

        private static string Sanitize(string demangled)
        {
            var result = demangled.Replace(", ", ",").Replace("> >", ">>").Replace("operator()", "op#2829");
            while (result.Contains("operator"))
            {
                var op_index = result.IndexOf("operator");
                var end_index = result.IndexOf("(", op_index);
                var op = result.Substring(op_index + 8, end_index - op_index - 8);
                result = result.Replace($"operator{op}", $"op#{string.Join("", op.Select(c => ((int)c).ToString("X2")).ToArray())}");
            }
            return result;
        }

        public static TOKEN[] Parse(string demangled)
        {
            var parser = new Parser(Sanitize(demangled));
            parser._Parse();
            return parser.Tokens.ToArray();
        }

        private void _Parse()
        {
            var start = "<([{";
            var name = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~#*&:"; // ":"は"::erase[abi:cxx11]"のようなシンボルのため。"::"を先にチェックすること！
            string buf = "";
            for (; Current < Demangled.Length; Current++)
            {
                if (start.Contains(Demangled[Current]))
                {
                    if(buf.Length > 0)
                    {
                        Tokens.Add(new NAME { Name = buf });
                        buf = "";
                    }
                    switch (Demangled[Current])
                    {
                        case '<':
                            Tokens.Add(new TEMPLATE { Args = _Split(_Cut()).Select(d => Parse(d)).ToArray() });
                            break;
                        case '(':
                            Tokens.Add(new BRACKET { Args = _Split(_Cut()).Select(d => Parse(d)).ToArray() });
                            break;
                        case '[':
                            Tokens.Add(new SQUARE_BRACKET { Args = _Split(_Cut()).Select(d => Parse(d)).ToArray() });
                            break;
                        case '{':
                            Tokens.Add(new TMP { Str = _Cut() });
                            break;
                    }
                }
                else if (_Check_Token("::"))
                {
                    if (buf.Length > 0)
                    {
                        Tokens.Add(new NAME { Name = buf });
                        buf = "";
                    }
                    Tokens.Add(new SCOPE());
                    Current++;
                }
                else if (!name.Contains(Demangled[Current]))
                {
                    if (buf.Length > 0)
                    {
                        Tokens.Add(new NAME { Name = buf });
                        buf = "";
                    }
                    Tokens.Add(new BOUNDARY());
                }
                else
                {
                    buf += Demangled[Current];
                }
            }
            if (buf.Length > 0)
            {
                Tokens.Add(new NAME { Name = buf });
            }
        }

        private bool _Check_Token(string str)
        {
            if (Current + str.Length > Demangled.Length)
                return false;
            for(int i = 0; i < str.Length; i++)
            {
                if(str[i] != Demangled[Current + i])
                {
                    return false;
                }
            }
            return true;
        }

        private string _Cut()
        {
            int nest = 0;
            int offset = Current;
            var start = "<([{";
            var end = ">)]}";
            if (!start.Contains(Demangled[Current]))
                throw new Exception();
            for(;Current < Demangled.Length; Current++)
            {
                if (start.Contains(Demangled[Current]))
                    nest++;
                else if (end.Contains(Demangled[Current]))
                {
                    if(--nest == 0)
                    {
                        break;
                    }
                }
            }
            if (nest > 0 || !end.Contains(Demangled[Current]))
                throw new Exception();
            return Demangled.Substring(offset + 1, Current - offset - 1);
        }

        private string[] _Split(string target)
        {
            int nest = 0;
            int offset = 0;
            var list = new List<string>();
            var start = "<([{";
            var end = ">)]}";
            for (int i = 0;i < target.Length; i++)
            {
                if (start.Contains(target[i]))
                    ++nest;
                else if (end.Contains(target[i]))
                    --nest;
                else if (target[i] == ',' && nest == 0)
                {
                    list.Add(target.Substring(offset, i - offset));
                    offset = i + 1;
                }
            }
            if (nest > 0)
                throw new Exception();
            list.Add(target.Substring(offset));
            return list.ToArray();
        }
    }

    interface TOKEN { }
    class NAME : TOKEN
    {
        public string Name { get; set; }
    }
    class SCOPE : TOKEN { }
    class BOUNDARY : TOKEN { }
    class TMP : TOKEN
    {
        public string Str { get; set; }
    }
    class TEMPLATE : TOKEN
    {
        public TOKEN[][] Args { get; set; }
    }
    class BRACKET : TOKEN
    {
        public TOKEN[][] Args { get; set; }
    }
    class SQUARE_BRACKET : TOKEN
    {
        public TOKEN[][] Args { get; set; }
    }
}
