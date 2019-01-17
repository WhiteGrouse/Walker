using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WalkerZero.Model;
using WalkerZero.Extensions.BinaryStream;

namespace WalkerZero
{
    class Program
    {
        static ElfHeader ReadElfHdr(FileStream stream)
        {
            stream.Seek(16, SeekOrigin.Begin);//IDENT
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var type = reader.ReadUInt16();
                var machine = reader.ReadUInt16();
                var version = reader.ReadUInt32();
                var entry = reader.ReadUInt32();
                var phoff = reader.ReadUInt32();
                var shoff = reader.ReadUInt32();
                var flags = reader.ReadUInt32();
                var ehsize = reader.ReadUInt16();
                var phentsize = reader.ReadUInt16();
                var phnum = reader.ReadUInt16();
                var shentsize = reader.ReadUInt16();
                var shnum = reader.ReadUInt16();
                var shstrndx = reader.ReadUInt16();
                return new ElfHeader(type, machine, version, entry, phoff, shoff, flags, ehsize, phentsize, phnum, shentsize, shnum, shstrndx);
            }
        }

        static Section[] ReadSectionHdrs(FileStream stream, ElfHeader elfHdr)
        {
            using(var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var noname_sections = new (uint name_index, uint addr, uint offset, uint size)[elfHdr.SectionHdrEntryNum];
                for(int i = 0; i < elfHdr.SectionHdrEntryNum; i++)
                {
                    stream.Seek(elfHdr.SectionHdrOffset + elfHdr.SectionHdrEntrySize * i, SeekOrigin.Begin);
                    var name = reader.ReadUInt32();
                    var type = reader.ReadUInt32();
                    var flags = reader.ReadUInt32();
                    var addr = reader.ReadUInt32();
                    var offset = reader.ReadUInt32();
                    var size = reader.ReadUInt32();
                    var link = reader.ReadUInt32();
                    var info = reader.ReadUInt32();
                    var addralign = reader.ReadUInt32();
                    var entsize = reader.ReadUInt32();
                    noname_sections[i] = (name, addr, offset, size);
                }

                var sections = new Section[elfHdr.SectionHdrEntryNum];
                for(int i = 0; i < elfHdr.SectionHdrEntryNum; i++)
                {
                    stream.Seek(noname_sections[elfHdr.SectionHdrStrTableIndex].offset + noname_sections[i].name_index, SeekOrigin.Begin);
                    var name = reader.ReadCString();
                    sections[i] = new Section(name, noname_sections[i].addr, noname_sections[i].offset, noname_sections[i].size);
                }

                return sections;
            }
        }

        static Symbol[] ReadSymbols(FileStream stream, ElfHeader elfHdr, Section[] sections)
        {
            var dynsym = sections.First(d => d.Name == ".dynsym");
            var dynstr = sections.First(d => d.Name == ".dynstr");
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                //EntrySize = 16
                var entry_num = dynsym.Size >> 4;
                var symbols = new Symbol[entry_num];
                for (int i = 0; i < entry_num; i++)
                {
                    stream.Seek(dynsym.Offset + 16 * i, SeekOrigin.Begin);
                    var name_index = reader.ReadUInt32();
                    var address = reader.ReadUInt32();
                    var size = reader.ReadUInt32();
                    var info = reader.ReadByte();
                    var other = reader.ReadByte();
                    var shndx = reader.ReadInt16();

                    stream.Seek(dynstr.Offset + name_index, SeekOrigin.Begin);
                    var name = reader.ReadCString();
                    symbols[i] = new Symbol(name, address, size, info, other, sections.Length < shndx ? sections[shndx] : null);
                }
                return symbols;
            }
        }

        static void Main(string[] args)
        {
            using(var stream = new FileStream("libminecraftpe.so", FileMode.Open, FileAccess.Read))
            {
                var elfHdr = ReadElfHdr(stream);
                var sectionHdrs = ReadSectionHdrs(stream, elfHdr);
                var symbols = ReadSymbols(stream, elfHdr, sectionHdrs);
            }
        }
    }
}
