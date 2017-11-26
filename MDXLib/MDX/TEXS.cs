using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib.MDX
{
    public class TEXS : BaseChunk, IReadOnlyCollection<Texture>
    {
        Texture[] Textures;

        public TEXS(BinaryReader br) : base(br)
        {
            Textures = new Texture[Size / 268];
            for (int i = 0; i < Textures.Length; i++)
                Textures[i] = new Texture(br);
        }

        public int Count => Textures.Length;
        public IEnumerator<Texture> GetEnumerator() => Textures.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Textures.AsEnumerable().GetEnumerator();
    }

    public class Texture
    {
        public uint ReplaceableId;
        public string Image;
        public TEXFLAGS Flags;

        public Texture(BinaryReader br)
        {
            ReplaceableId = br.ReadUInt32();
            Image = br.ReadCString(Constants.SizeFileName);
            Flags = (TEXFLAGS)br.ReadUInt32();
		}
    }
}
