using M2Lib.types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.Structs
{
    public class CVector2
    {
        public float X;
        public float Y;

        public CVector2() { }

        public CVector2(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
        }

        public C2Vector ToC2Vector => new C2Vector(X, -Y); //Inverse Y coord

        public override string ToString() => $"X: {X}, Y: {Y}";
    }
}
