using M2Lib.m2;
using M2Lib.types;
using MDXLib.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDXLib.MDX;

namespace MDXParser
{
	class Program
	{ 

		static void Main(string[] args)
		{
			//Tests.BulkParse();

			M2 comparison = new M2();
			using (var reader = new BinaryReader(new FileStream(@"Files\Boar.m2", FileMode.Open)))
				comparison.Load(reader);


			string file = @"Files\Boar.mdx";
			if (args.Length > 0 && File.Exists(args[0]))
				file = args[0];
			
			Model mdx = new Model(file);
			VERS version = mdx.Get<VERS>();
			MODL modl = mdx.Get<MODL>();
			SEQS sequences = mdx.Get<SEQS>();
			MTLS materials = mdx.Get<MTLS>();
			TEXS textures = mdx.Get<TEXS>();
			GEOS geosets = mdx.Get<GEOS>();
			GEOA geosetanims = mdx.Get<GEOA>();
			HELP helpers = mdx.Get<HELP>();
			ATCH attachments = mdx.Get<ATCH>();
			PIVT pivotpoints = mdx.Get<PIVT>();
			CAMS cameras = mdx.Get<CAMS>();
			EVTS events = mdx.Get<EVTS>();
			HTST hittestshapes = mdx.Get<HTST>();
			CLID collisions = mdx.Get<CLID>();
			GLBS globalsequences = mdx.Get<GLBS>();
			PRE2 particleemitter2s = mdx.Get<PRE2>();
			RIBB ribbonemitters = mdx.Get<RIBB>();
			LITE lights = mdx.Get<LITE>();
			TXAN textureanimations = mdx.Get<TXAN>();
			BONE bones = mdx.Get<BONE>();

			M2Converter converter = new M2Converter(mdx, M2.Format.LichKing);
			var m2 = converter.Model;
			m2.Attachments = converter.GetAttachments();
			m2.Bones = converter.GetBones();
			m2.BoundingBox = converter.GetBoundingBox();
			m2.BoundingSphereRadius = converter.GetBoundingSphereRadius();
			m2.Cameras = converter.GetCameras();
			m2.Colors = converter.GetColors();
			m2.Events = converter.GetEvents();
			m2.GlobalSequences = converter.GetGlobalSequences();
			m2.GlobalVertexList = converter.GetVertices();
			m2.Materials = converter.GetMaterials();
			m2.Textures = converter.GetTextures();
			m2.Sequences = converter.GetSequences();
			m2.TexLookup = converter.GetTextureLookup();
			m2.TexUnitLookup = converter.GetTexUnitLookup();
			m2.TextureTransforms = converter.GetTextureTransform();
			m2.Transparencies = converter.GetTextureWeights();

			m2.UvAnimLookup.Add(-1);

			m2.GlobalVertexList.ForEach(x => x.TexCoords[0] = new C2Vector(x.TexCoords[0].X, x.TexCoords[0].Y * -1f));

			m2.Views.Add(converter.GetSkin());
			m2.BoneLookup = converter.GetBoneLookup();

			converter.UpdateCollisions();

			using (var fs = new FileStream(Path.ChangeExtension(file, "m2"), FileMode.Create))
			using (var bw = new BinaryWriter(fs))
				m2.Save(bw, M2.Format.LichKing);
		}
	}
}
