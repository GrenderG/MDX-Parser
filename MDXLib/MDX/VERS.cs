using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.MDX
{
    public class VERS : BaseChunk
    {
        public uint Version;

        public VERS(BinaryReader br) : base(br)
        {
            Version = br.ReadUInt32();
            if (Version != 1300)
                throw new Exception("Invalid version");
        }
    }
}
