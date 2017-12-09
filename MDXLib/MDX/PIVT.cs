using MDXLib.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib.MDX
{
    public class PIVT : BaseChunk, IReadOnlyCollection<CVector3>
    {
        CVector3[] PivotPoints;

        public PIVT(BinaryReader br, uint version) : base(br)
		{
            PivotPoints = new CVector3[Size / 0xC];
            for (int i = 0; i < PivotPoints.Length; i++)
                PivotPoints[i] = new CVector3(br);
        }

        public int Count => PivotPoints.Length;
        public IEnumerator<CVector3> GetEnumerator() => PivotPoints.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => PivotPoints.AsEnumerable().GetEnumerator();
    }
}
