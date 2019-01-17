using System;
using System.Collections.Generic;
using System.Text;

namespace WalkerZero.Model
{
    /// <summary>
    /// elf32_hdr
    /// </summary>
    class ElfHeader
    {
        public enum T_Elf
        {
            NONE,
            REL,
            EXEC,
            DYN,
            CORE,
            PROC
        }

        public ushort Type { get; }
        public T_Elf Kind => Type < (int)T_Elf.PROC ? (T_Elf)Type : T_Elf.PROC;

        public ushort Machine { get; }
        public uint Version { get; }
        public uint EntryPoint { get; }
        public uint ProgramHdrOffset { get; }
        public uint SectionHdrOffset { get; }
        public uint Flags { get; }
        public ushort ElfHdrSize { get; }
        public ushort ProgramHdrEntrySize { get; }
        public ushort ProgramHdrEntryNum { get; }
        public ushort SectionHdrEntrySize { get; }
        public ushort SectionHdrEntryNum { get; }
        public ushort SectionHdrStrTableIndex { get; }

        public ElfHeader(ushort type, ushort machine, uint version, uint entry, uint phoff, uint shoff, uint flags, ushort ehsize, ushort phentsize, ushort phnum, ushort shentsize, ushort shnum, ushort shstrndx)
        {
            Type = type;
            Machine = machine;
            Version = version;
            EntryPoint = entry;
            ProgramHdrOffset = phoff;
            SectionHdrOffset = shoff;
            Flags = flags;
            ElfHdrSize = ehsize;
            ProgramHdrEntrySize = phentsize;
            ProgramHdrEntryNum = phnum;
            SectionHdrEntrySize = shentsize;
            SectionHdrEntryNum = shnum;
            SectionHdrStrTableIndex = shstrndx;
        }
    }
}
