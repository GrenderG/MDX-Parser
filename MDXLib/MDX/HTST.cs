using MDXLib.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib.MDX
{
    public class HTST : BaseChunk, IReadOnlyCollection<HitTestShape>
    {
        HitTestShape[] HitTestShapes;

        public HTST(BinaryReader br) : base(br)
        {
            HitTestShapes = new HitTestShape[br.ReadInt32()];
            for (int i = 0; i < HitTestShapes.Length; i++)
                HitTestShapes[i] = new HitTestShape(br);
        }

        public int Count => HitTestShapes.Length;
        public IEnumerator<HitTestShape> GetEnumerator() => HitTestShapes.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => HitTestShapes.AsEnumerable().GetEnumerator();
    }

    public class HitTestShape : GenObject
    {
        public uint TotalSize;
        public GEOM_SHAPE Type;
        public CBox Box;
        public CCylinder Cylinder;
        public CSphere Sphere;
        public CPlane Plane;

        public HitTestShape(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();

			LoadTracks(br);

            Type = (GEOM_SHAPE)br.ReadByte();

            switch (Type)
            {
                case GEOM_SHAPE.SHAPE_BOX:
                    Box = new CBox(br);
                    break;
                case GEOM_SHAPE.SHAPE_CYLINDER:
                    Cylinder = new CCylinder(br);
                    break;
                case GEOM_SHAPE.SHAPE_PLANE:
                    Plane = new CPlane(br);
                    break;
                case GEOM_SHAPE.SHAPE_SPHERE:
                    Sphere = new CSphere(br);
                    break;
            }
        }
    }
}
