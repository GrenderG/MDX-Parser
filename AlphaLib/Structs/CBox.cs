using M2Lib.types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.Structs
{
    public class CBox
    {
        public CVector3 Min;
        public CVector3 Max;

        public CBox()
        {
            Min = new CVector3();
            Max = new CVector3();
        }

        public CBox(BinaryReader br)
        {
            Min = new CVector3(br);
            Max = new CVector3(br);
        }

        public CAaBox ToCAaBox => new CAaBox(Min.ToC3Vector, Max.ToC3Vector);
    }
}
