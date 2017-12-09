using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.MDX
{
    public class VERS : BaseChunk
    {
        public new uint Version;

        public VERS(BinaryReader br, uint version) : base(br)
        {
            Version = br.ReadUInt32();
        }
    }
}
