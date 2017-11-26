using M2Lib.m2;
using M2Lib.types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXParser
{
    public static class Extensions
    {

        #region M2Lib Extensions
        public static M2Array<T> ToM2Array<T>(this IEnumerable<T> values) where T : new()
        {
            var array = new M2Array<T>();
            if (values != null)
                array.AddRange(values);
            return array;
        }

        public static float Length(this C3Vector vector) => (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

        public static float Distance(this C3Vector vector, C3Vector target, bool normalised = true)
        {
            C3Vector temp = new C3Vector(vector.X - target.X, vector.Y - target.Y, vector.Z - target.Z);
            return normalised ? Math.Abs(temp.Length()) : temp.Length();
        }

        public static C3Vector Normalise(this C3Vector vector)
        {
            double lenSq = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
            if (lenSq > 0.00000023841858)
            {
                double invLen = 1.0 / Math.Sqrt(lenSq);
                var X = vector.X * invLen;
                var Y = vector.Y * invLen;
                var Z = vector.Z * invLen;
                return new C3Vector((float)X, (float)Y, (float)Z);
            }

            return vector;
        }

        #endregion
    }
}
