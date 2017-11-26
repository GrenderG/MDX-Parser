using MDXLib.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDXLib.MDX
{
	public class MODL : BaseChunk
	{
		public string Name;
		public string AnimationFile;
		public CExtent Bounds;
		public uint BlendTime;
		public byte Flags;

		public MODL(BinaryReader br) : base(br)
		{
			Name = br.ReadCString(Constants.SizeName);
			AnimationFile = br.ReadCString(Constants.SizeFileName);
			Bounds = new CExtent(br);
			BlendTime = br.ReadUInt32();
			Flags = br.ReadByte();
		}
	}
}
