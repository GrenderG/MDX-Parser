using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.Structs
{
    public class CVertex
    {
        public ushort Vertex1;
        public ushort Vertex2;
        public ushort Vertex3;

        public CVertex()
        {

        }

        public CVertex(BinaryReader br)
        {
            Vertex1 = br.ReadUInt16();
            Vertex2 = br.ReadUInt16();
            Vertex3 = br.ReadUInt16();
        }

        public ushort[] ToArray(ushort offset = 0) => new ushort[] 
        {
            (ushort)(Vertex1 + offset),
            (ushort)(Vertex2 + offset),
            (ushort)(Vertex3 + offset)
        };

    }
}
