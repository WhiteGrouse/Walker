using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WalkerZero.Extensions.BinaryStream
{
    static class Extensions
    {
        public static string ReadCString(this BinaryReader reader)
        {
            using(var buf = new MemoryStream())
            {
                byte b;
                while((b = reader.ReadByte()) > 0x00)
                    buf.WriteByte(b);
                return Encoding.ASCII.GetString(buf.ToArray());
            }
        }
    }
}
