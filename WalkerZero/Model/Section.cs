using System;
using System.Collections.Generic;
using System.Text;

namespace WalkerZero.Model
{
    class Section
    {
        public string Name { get; }
        public uint Address { get; }
        public uint Offset { get; }
        public uint Size { get; }

        public Section(string name, uint address, uint offset, uint size)
        {
            Name = name;
            Address = address;
            Offset = offset;
            Size = size;
        }
    }
}
