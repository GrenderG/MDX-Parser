using M2Lib.m2;
using M2Lib.types;
using MDXLib.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDXLib.MDX
{
	public class GEOS : BaseChunk, IReadOnlyCollection<Geoset>
	{
		Geoset[] Geosets;

		public GEOS(BinaryReader br) : base(br)
		{
			Geosets = new Geoset[br.ReadUInt32()];
			for (int i = 0; i < Geosets.Length; i++)
				Geosets[i] = new Geoset(br);
		}

		public int Count => Geosets.Length;
		public IEnumerator<Geoset> GetEnumerator() => Geosets.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Geosets.AsEnumerable().GetEnumerator();
	}

	public class Geoset
	{
		public uint TotalSize;
		public uint NrOfVertices;
		public List<CVector3> Vertices = new List<CVector3>();
		public uint NrOfNormals;
		public List<CVector3> Normals = new List<CVector3>();
		public uint NrOfTexCoords;
		public List<CVector2> TexCoords = new List<CVector2>();

		//MDLPRIMITIVES
		public uint NrOfFaceTypeGroups;
		public List<byte> FaceTypes = new List<byte>();
		public uint NrOfFaceGroups;
		public List<uint> FaceGroups = new List<uint>();
		public uint NrOfFaceVertices;
		public List<CVertex> FaceVertices = new List<CVertex>();

		public uint NrOfVertexGroupIndices;
		public List<byte> VertexGroupIndices = new List<byte>();
		public uint NrOfMatrixGroups;
		public List<uint> MatrixGroups = new List<uint>();
		public uint NrOfMatrixIndexes;
		public List<uint> MatrixIndexes = new List<uint>();
		public uint NrOfBoneIndexes;
		public List<uint> BoneIndexes = new List<uint>();
		public uint NrOfBoneWeights;
		public List<uint> BoneWeights = new List<uint>();

		public uint MaterialId;
		public CExtent Bounds;
		public uint SelectionGroup;
		public bool Unselectable;

		public uint NrOfExtents;
		public List<CExtent> Extents = new List<CExtent>();

		public Dictionary<byte, List<CVector3>> GroupedVertices = new Dictionary<byte, List<CVector3>>();


		public Geoset(BinaryReader br)
		{
			TotalSize = br.ReadUInt32();
			long end = TotalSize + br.BaseStream.Position;

			//Vertices
			if (br.HasTag("VRTX"))
			{
				NrOfVertices = br.ReadUInt32();
				for (int i = 0; i < NrOfVertices; i++)
					Vertices.Add(new CVector3(br));
			}

			//Normals
			if (br.HasTag("NRMS"))
			{
				NrOfNormals = br.ReadUInt32();
				for (int i = 0; i < NrOfNormals; i++)
					Normals.Add(new CVector3(br));
			}

			//TexCoords
			if (br.HasTag("UVAS"))
			{
				NrOfTexCoords = br.ReadUInt32(); //Amount of groups
				for (int i = 0; i < NrOfNormals * NrOfTexCoords; i++)
					TexCoords.Add(new CVector2(br));
			}

			//Face Group Type
			if (br.HasTag("PTYP"))
			{
				NrOfFaceTypeGroups = br.ReadUInt32();
				FaceTypes.AddRange(br.ReadBytes((int)NrOfFaceTypeGroups));
			}

			//Face Groups
			if (br.HasTag("PCNT"))
			{
				NrOfFaceGroups = br.ReadUInt32();
				for (int i = 0; i < NrOfFaceGroups; i++)
					FaceGroups.Add(br.ReadUInt32());
			}

			//Indexes
			if (br.HasTag("PVTX"))
			{
				NrOfFaceVertices = br.ReadUInt32();
				for (int i = 0; i < NrOfFaceVertices / 3; i++)
					FaceVertices.Add(new CVertex(br));
			}

			//Vertex Groups 
			if (br.HasTag("GNDX"))
			{
				NrOfVertexGroupIndices = br.ReadUInt32();
				VertexGroupIndices.AddRange(br.ReadBytes((int)NrOfVertexGroupIndices));
			}

			//Matrix Groups
			if (br.HasTag("MTGC"))
			{
				NrOfMatrixGroups = br.ReadUInt32();
				for (int i = 0; i < NrOfMatrixGroups; i++)
					MatrixGroups.Add(br.ReadUInt32());
			}

			//Matrix Indexes
			if (br.HasTag("MATS"))
			{
				NrOfMatrixIndexes = br.ReadUInt32();
				for (int i = 0; i < NrOfMatrixIndexes; i++)
					MatrixIndexes.Add(br.ReadUInt32());
			}

			//Bone Indexes
			if (br.HasTag("BIDX"))
			{
				NrOfBoneIndexes = br.ReadUInt32();
				for (int i = 0; i < NrOfBoneIndexes; i++)
					BoneIndexes.Add(br.ReadUInt32());
			}

			//Bone Weights
			if (br.HasTag("BWGT"))
			{
				NrOfBoneWeights = br.ReadUInt32();
				for (int i = 0; i < NrOfBoneWeights; i++)
					BoneWeights.Add(br.ReadUInt32());
			}

			MaterialId = br.ReadUInt32();
			SelectionGroup = br.ReadUInt32();
			Unselectable = br.ReadUInt32() == 1;
			Bounds = new CExtent(br);
			
			//Extents
			NrOfExtents = br.ReadUInt32();
			for (int i = 0; i < NrOfExtents; i++)
				Extents.Add(new CExtent(br));

			//Grouped Vertices
			for (int i = 0; i < NrOfVertices; i++)
			{
				if (!GroupedVertices.ContainsKey(VertexGroupIndices[i]))
					GroupedVertices.Add(VertexGroupIndices[i], new List<CVector3>());

				GroupedVertices[VertexGroupIndices[i]].Add(Vertices[i]);
			}
		}

		public List<byte[]> GetIndicies()
		{
			uint[] matrixindexes = MatrixIndexes.ToArray();

			//Parse the bone indices by slicing the matrix groups
			uint[][] slices = new uint[NrOfMatrixGroups][];
			for (int i = 0; i < NrOfMatrixGroups; i++)
			{
				int offset = (i == 0 ? 0 : (int)MatrixGroups[i - 1]);

				slices[i] = new uint[MatrixGroups[i]];
				Array.Copy(matrixindexes, offset, slices[i], 0, slices[i].Length);
			}

			//Construct the final bone arrays
			List<byte[]> boneIndices = new List<byte[]>();
			for (int i = 0; i < NrOfVertices; i++)
			{
				uint[] slice = slices[VertexGroupIndices[i]];
				byte[] indicies = new byte[4];

				//TODO some slices have more than 4 bone indicies what do??
				for (int j = 0; j < Math.Min(slice.Length, 4); j++)
					indicies[j] = (byte)(slice[j]);

				boneIndices.Add(indicies);
			}

			return boneIndices;
		}

		public C3Vector GetCenter() => new C3Vector(Vertices.Average(x => x.X), Vertices.Average(x => x.Y), Vertices.Average(x => x.Z));
	}
}
