using M2Lib.m2;
using M2Lib.types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib
{
    public static class Extensions
    {
        #region BinaryReader

        public static string ReadCString(this BinaryReader br, int length) => Encoding.UTF8.GetString(br.ReadBytes(length)).TrimEnd('\0');

        public static string ReadString(this BinaryReader br, int length) => Encoding.UTF8.GetString(br.ReadBytes(length));

        public static void AssertTag(this BinaryReader br, string tag)
        {
            string _tag = br.ReadCString(Constants.SizeTag);
            if (_tag != tag)
                throw new Exception($"Expected '{tag}' at {br.BaseStream.Position - Constants.SizeTag} got '{_tag}'.");
        }

        public static bool HasTag(this BinaryReader br, string tag)
        {
            bool match = tag == br.ReadCString(Constants.SizeTag);
            if (!match)
                br.BaseStream.Position -= Constants.SizeTag;
            return match;
        }

        public static bool AtEnd(this BinaryReader br) => br.BaseStream.Position == br.BaseStream.Length;
        #endregion

        public static short ToShort(this float f) => (short)(f * short.MaxValue);

    }
}
