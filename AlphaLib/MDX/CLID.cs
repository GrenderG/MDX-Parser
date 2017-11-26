using MDXLib.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.MDX
{
    public class CLID : BaseChunk
    {
        public uint NrOfVertices;
        public List<CVector3> Vertices = new List<CVector3>();
        public uint NrOfTriIndices;
        public List<ushort> TriIndices = new List<ushort>();
        public uint NrOfFacetNormals;
        public List<CVector3> FacetNormals = new List<CVector3>();

        public CLID(BinaryReader br) : base(br)
        {
            br.AssertTag("VRTX");
            NrOfVertices = br.ReadUInt32();
            for (int i = 0; i < NrOfVertices; i++)
                Vertices.Add(new CVector3(br));

            br.AssertTag("TRI ");
            NrOfTriIndices = br.ReadUInt32();
            for (int i = 0; i < NrOfTriIndices; i++)
                TriIndices.Add(br.ReadUInt16());

            br.AssertTag("NRMS");
            NrOfFacetNormals = br.ReadUInt32();
            for (int i = 0; i < NrOfFacetNormals; i++)
                FacetNormals.Add(new CVector3(br));
        }
    }
}
