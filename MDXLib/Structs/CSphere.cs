using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.Structs
{
    public class CSphere
    {
        public CVector3 Center;
        public float Radius;

        public CSphere(BinaryReader br)
        {
            Center = new CVector3(br);
            Radius = br.ReadSingle();
        }
    }
}
