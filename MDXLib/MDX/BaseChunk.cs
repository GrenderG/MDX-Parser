using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib.MDX
{
    public class BaseChunk
    {
        public string Type;
        public uint Size;
		protected uint Version;

        public BaseChunk(BinaryReader br)
        {
            Type = br.ReadString(4);
            Size = br.ReadUInt32();

            if (Type != GetType().Name)
                throw new Exception($"Expected {GetType().Name}, got {Type}");
        }
    }
}
