using System;
using System.Collections.Generic;
using System.Text;

namespace WalkerZero.Model
{
    class Symbol
    {
        public enum T_Kind
        {
            NOTYPE,
            OBJECT,
            FUNC,
            SECTION,
            FILE,
            COMMON,
            TLS,
            PROC
        }

        public enum T_Bind
        {
            LOCAL,
            GLOBAL,
            WEAK,
            PROC
        }

        public enum T_Visibility
        {
            DEFAULT,
            INTERNAL,
            HIDDEN,
            PROTECTED
        }

        public string Name { get; }
        public uint Address { get; }
        public uint Size { get; }
        public T_Kind Type { get; }
        public T_Bind Bind { get; }
        public T_Visibility Visibility { get; }
        public Section Section { get; }

        public Symbol(string name, uint address, uint size, byte info, byte other, Section section)
        {
            Name = name;
            Address = address;
            Size = size;
            Type = (T_Kind)(info & 0x0f);
            Bind = (T_Bind)(info >> 4);
            Visibility = (T_Visibility)other;
            Section = section;
        }
        
    }
}
