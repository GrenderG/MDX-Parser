using MDXLib;
using MDXLib.MDX;
using M2Lib.m2;
using M2Lib.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDXParser
{
    public class M2Converter
    {
        public M2 Model { get; private set; }

        private readonly Model _model;

        public M2Converter(Model model, M2.Format version)
        {
			if (model.Version > 1400)
				throw new Exception($"Invalid version {model.Version}");
			
            _model = model;

            Model = new M2()
            {
                Name = _model.Name + "_Alpha",
                Version = version
            };
        }

        public CAaBox GetBoundingBox() => _model.Bounds.Extent.ToCAaBox;

        public float GetBoundingSphereRadius() => (Model.BoundingBox.Min.Length() + Model.BoundingBox.Max.Length()) / 2f;


        public void UpdateCollisions()
        {
            if (_model.Has<CLID>())
            {
                var v = _model.Get<CLID>().Vertices;
                C3Vector min = new C3Vector(v.Min(x => x.X), v.Min(x => x.Y), v.Min(x => x.Z));
                C3Vector max = new C3Vector(v.Max(x => x.X), v.Max(x => x.Y), v.Max(x => x.Z));

                Model.CollisionBox = new CAaBox(min, max);
                Model.CollisionSphereRadius = (Model.CollisionBox.Min.Length() + Model.CollisionBox.Max.Length()) / 2f;
                Model.CollisionNormals = _model.Get<CLID>().FacetNormals.Select(x => x.ToC3Vector).ToM2Array();
                Model.CollisionTriangles = _model.Get<CLID>().TriIndices.ToM2Array();
                Model.CollisionVertices = _model.Get<CLID>().Vertices.Select(x => x.ToC3Vector).ToM2Array();
            }
            else
            {
                var view = Model.Views[0];

                //Calculate Vertices
                Model.CollisionVertices = new M2Array<C3Vector>();
                for (int i = 0; i < view.Indices.Count; i++)
                    Model.CollisionVertices.Add(Model.GlobalVertexList[i].Position);

                //Calculate normals and triangles
                Model.CollisionNormals = new M2Array<C3Vector>();
                Model.CollisionTriangles = new M2Array<ushort>();
                for (int i = 0; i < view.Triangles.Count / 3; i++)
                {
                    ushort i1 = view.Triangles[i];
                    ushort i2 = view.Triangles[i + 1];
                    ushort i3 = view.Triangles[i + 2];

                    C3Vector v1 = Model.GlobalVertexList[i1].Position;
                    C3Vector v2 = Model.GlobalVertexList[i2].Position;
                    C3Vector v3 = Model.GlobalVertexList[i3].Position;

                    C3Vector U = new C3Vector(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
                    C3Vector V = new C3Vector(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
                    C3Vector N = new C3Vector(U.Y * V.Z - U.Z * V.Y, U.Z * V.X - U.X * V.Z, U.X * V.Y - U.Y * V.X).Normalise();

                    Model.CollisionNormals.Add(N);
                    Model.CollisionTriangles.AddRange(new[] { i1, i2, i3 });
                }

                //Calculate box and Sphere
                var v = Model.CollisionVertices;
                C3Vector min = new C3Vector(v.Min(x => x.X), v.Min(x => x.Y), v.Min(x => x.Z));
                C3Vector max = new C3Vector(v.Max(x => x.X), v.Max(x => x.Y), v.Max(x => x.Z));
                Model.CollisionBox = new CAaBox(min, max);
                Model.CollisionSphereRadius = (Model.CollisionBox.Min.Length() + Model.CollisionBox.Max.Length()) / 2f;
            }
        }


        public M2Array<int> GetGlobalSequences() => _model.Get<GLBS>().ToM2Array();

        public M2Array<M2Bone> GetBones()
        {
            var keybones = Enum.GetNames(typeof(M2Bone.KeyBone)).Select(x => x.ToLower()).ToList();

            M2Array<M2Bone> bones = new M2Array<M2Bone>();

            for (int i = 0; i < _model.Get<PIVT>().Count; i++)
            {
                var obj = _model.Hierachy[i];
                M2Bone bone = new M2Bone()
                {
                    KeyBoneId = (M2Bone.KeyBone)keybones.IndexOf(obj.Name.ToLower().Replace("_", "").TrimStart('$')), //Dirty but works
                    ParentBone = (short)obj.ParentId,
                    Pivot = _model.Get<PIVT>().ElementAt(i).ToC3Vector,
                };

                obj.RotationKeys?.PopulateM2Track(bone.Rotation, _model.Get<SEQS>());
                obj.ScaleKeys?.PopulateM2Track(bone.Scale, _model.Get<SEQS>());
                obj.TranslationKeys?.PopulateM2Track(bone.Translation, _model.Get<SEQS>());

                if (obj.TranslationKeys != null)
                    bone.Flags |= M2Bone.BoneFlags.Transformed;

                if (obj is Bone b)
                {
                    if (b.Flags.HasFlag(GENOBJECTFLAGS.BILLBOARD_LOCK_X))
                        bone.Flags |= M2Bone.BoneFlags.CylindricalBillboardLockX;
					if (b.Flags.HasFlag(GENOBJECTFLAGS.BILLBOARD_LOCK_Y))
						bone.Flags |= M2Bone.BoneFlags.CylindricalBillboardLockY;
					if (b.Flags.HasFlag(GENOBJECTFLAGS.BILLBOARD_LOCK_Z))
						bone.Flags |= M2Bone.BoneFlags.CylindricalBillboardLockZ;
                }

                bones.Add(bone);
            }

            return bones;
        }

        public M2Array<M2Attachment> GetAttachments()
        {
            M2Array<M2Attachment> attachments = new M2Array<M2Attachment>();

            if (!_model.Has<ATCH>())
                return attachments;

            foreach (var a in _model.Get<ATCH>())
            {
                M2Attachment attach = new M2Attachment()
                {
                    Id = (uint)a.AttachmentId,
                    Position = _model.Get<PIVT>().ElementAt(a.ObjectId).ToC3Vector,
                    Bone = (uint)a.ParentId,
                };

                attach.AnimateAttached.Timestamps.Add(new M2Array<uint>() { 0 });
                attach.AnimateAttached.Values.Add(new M2Array<bool>() { true });

                attachments.Add(attach);
            }

            return attachments;
        }

        public M2Array<M2Sequence> GetSequences()
        {
            M2Array<M2Sequence> sequences = new M2Array<M2Sequence>();

            foreach (var s in _model.Get<SEQS>())
            {
                M2Sequence sequence = new M2Sequence()
                {
                    AnimationId = 0, //Todo match animation ids
                    Length = (uint)(s.MaxTime - s.MinTime),
                    MovingSpeed = s.MoveSpeed,
                    BlendTimeStart = (ushort)s.BlendTime,
                    BlendTimeEnd = (ushort)s.BlendTime,
                    Probability = (short)(short.MaxValue * (1f - s.Frequency)),
                    MinimumRepetitions = (uint)s.MinReplay,
                    MaximumRepetitions = (uint)s.MaxReplay,
                    Bounds = s.Bounds.Extent.ToCAaBox,
                };

                if (!s.NonLooping)
                    sequence.Flags |= M2Sequence.SequenceFlags.Looped;

                sequences.Add(sequence);
            }

            return sequences;
        }

        public M2Array<M2Event> GetEvents()
        {
            M2Array<M2Event> events = new M2Array<M2Event>();

            if (!_model.Has<EVTS>())
                return events;

            foreach (var e in _model.Get<EVTS>())
            {
                events.Add(new M2Event()
                {
                    Identifier = e.Name,
                    Position = _model.Get<PIVT>().ElementAt(e.ObjectId).ToC3Vector
                });
            }

            return events;
        }

        public M2SkinProfile GetSkin()
        {
            var geos = _model.Get<GEOS>().ToList();
            var materials = _model.Get<MTLS>()?.ToArray();
            var geoa = _model.Get<GEOA>()?.ToList();

            M2SkinProfile profile = new M2SkinProfile()
            {
                Bones = 256,
                Indices = Enumerable.Range(0, geos.Sum(x => x.Vertices.Count)).Select(x => (ushort)x).ToM2Array(),
                Triangles = new M2Array<ushort>()
            };

            ushort offset = 0;
            foreach (var geo in geos)
            {
                profile.Triangles.AddRange(geo.FaceVertices.SelectMany(y => y.ToArray(offset)));
                offset += (ushort)geo.NrOfVertices;

                M2SkinSection section = new M2SkinSection()
                {
                    StartBones = (ushort)(profile.Submeshes?.Sum(x => x.NBones) ?? 0),
                    StartTriangle = (ushort)(profile.Submeshes?.Sum(x => x.NTriangles) ?? 0),
                    StartVertex = (ushort)(profile.Submeshes?.Sum(x => x.NVertices) ?? 0),
                    NBones = (ushort)geo.GroupedVertices.Count,
                    NTriangles = (ushort)geo.NrOfFaceVertices,
                    NVertices = (ushort)geo.NrOfVertices,
                    CenterBoundingBox = geo.GetCenter(),
                    CenterMass = geo.GetCenter(),
                };
                section.Radius = geo.Vertices.Select(x => x.ToC3Vector).Max(x => x.Distance(section.CenterBoundingBox));

                profile.Submeshes.Add(section);
            }

            //M2 Batches
            if (materials == null)
                return profile;

            for (int m = 0; m < materials.Length; m++)
            {
                for (int l = 0; l < materials[m].Layers.Count; l++)
                {
                    var layer = materials[m].Layers[l];
                    M2Batch batch = new M2Batch()
                    {
                        Flags = 16, //Usually 16 for static textures 
                        Flags2 = (byte)layer.PriorityPlane,
                        ShaderId = 0,
                        SubmeshIndex = (ushort)geos.FindIndex(x => x.MaterialId == m),
                        SubmeshIndex2 = (ushort)geos.FindIndex(x => x.MaterialId == m),
                        ColorIndex = -1,
                        OpCount = 1,
                        RenderFlags = (ushort)profile.TextureUnits.Count, 
                        Texture = (ushort)layer.TextureId,
                        TexUnitNumber2 = (ushort)layer.TextureId,
                        Layer = (ushort)l,
                        TextureAnim = (ushort)(layer.TextureAnimationId + 1), //MDX is -1 based
                    };

                    if (batch.TextureAnim > 0)
                        batch.Flags = 0; //0 for animated textures

                    if (geoa != null)
                        batch.ColorIndex = (short)geoa.FindIndex(x => x.GeosetId - 1 == batch.SubmeshIndex);

                    Model.TransLookup.Add((short)Model.TransLookup.Count);

                    profile.TextureUnits.Add(batch);
                }
            }

            return profile;
        }

        public M2Array<M2Vertex> GetVertices()
        {
            M2Array<M2Vertex> vertices = new M2Array<M2Vertex>();

            Func<int, byte[]> GetWeights = (c) =>
            {
                byte[] w = new byte[4];
                for (int i = 0; i < c; i++)
                    w[i] = (byte)Math.Ceiling(255f / c);

                if ((255 % c) != 0)
                    w[c - 1]--; //Must add up to 255

                return w;
            };

            foreach (var geo in _model.Get<GEOS>())
            {
                var indicies = geo.GetIndicies();

                for (int i = 0; i < geo.Vertices.Count; i++)
                {
                    //Calculate weights
                    int count = indicies[i].Skip(1).TakeWhile(x => x != 0).Count() + 1; //Count non-zero indicies excluding first
                    byte[] weights = GetWeights(count); //Build weights

                    M2Vertex vertex = new M2Vertex()
                    {
                        BoneIndices = indicies[i],
                        BoneWeights = weights,
                        Normal = geo.Normals[i].ToC3Vector,
                        Position = geo.Vertices[i].ToC3Vector,
                        TexCoords = new[] { geo.TexCoords[i].ToC2Vector, new C2Vector() }
                    };
                    vertices.Add(vertex);
                }
            }

            return vertices;
        }

        public M2Array<M2Material> GetMaterials()
        {
            M2Array<M2Material> materials = new M2Array<M2Material>();

            if (!_model.Has<MTLS>())
                return materials;

            var layers = _model.Get<MTLS>().SelectMany(x => x.Layers);
            foreach (var layer in layers)
            {
                M2Material material = new M2Material();

                if (layer.Flags.HasFlag(MDLGEO.MODEL_GEO_UNSHADED))
                    material.Flags |= M2Material.RenderFlags.Unlit;
                if (layer.Flags.HasFlag(MDLGEO.MODEL_GEO_TWOSIDED))
                    material.Flags |= M2Material.RenderFlags.TwoSided;
                if (layer.Flags.HasFlag(MDLGEO.MODEL_GEO_UNFOGGED))
                    material.Flags |= M2Material.RenderFlags.Unfogged;

                switch (layer.BlendMode)
                {
                    case MDLTEXOP.TEXOP_ADD:
                        material.BlendMode |= M2Material.BlendingMode.Add;
                        break;
                    case MDLTEXOP.TEXOP_TRANSPARENT: // ??
                    case MDLTEXOP.TEXOP_MODULATE:
                        material.BlendMode |= M2Material.BlendingMode.Mod;
                        break;
                    case MDLTEXOP.TEXOP_MODULATE2X:
                        material.BlendMode |= M2Material.BlendingMode.Mod2X;
                        break;
                    case MDLTEXOP.TEXOP_ADD_ALPHA:
                        material.BlendMode |= M2Material.BlendingMode.Decal;
                        break;
                }

                materials.Add(material);
            }

            return materials;
        }

        public M2Array<M2Texture> GetTextures()
        {
            M2Array<M2Texture> textures = new M2Array<M2Texture>();

            if (!_model.Has<TEXS>())
                return textures;

            foreach (var t in _model.Get<TEXS>())
            {
                M2Texture texture = new M2Texture()
                {
                    Flags = (M2Texture.TextureFlags)t.Flags,
                    Name = t.Image,
                    Type = (M2Texture.TextureType)t.ReplaceableId
                };
                textures.Add(texture);
            }

            return textures;
        }

        public M2Array<M2Camera> GetCameras()
        {
            M2Array<M2Camera> cameras = new M2Array<M2Camera>();

            if (!_model.Has<CAMS>())
                return cameras;

            foreach (var c in _model.Get<CAMS>())
            {
                M2Camera camera = new M2Camera()
                {
                    FarClip = c.FarClip,
                    FieldOfView = new M2Track<C3Vector>(),
                    NearClip = c.NearClip,
                    PositionBase = c.Pivot.ToC3Vector,
                    TargetPositionBase = c.TargetPosition.ToC3Vector
                };
                camera.FieldOfView.Timestamps.Add(new M2Array<uint>() { 0 });
                camera.FieldOfView.Values.Add(new M2Array<C3Vector>() { new C3Vector(c.FieldOfView, 0, 0) });
                
                switch (c.Name.ToUpper())
                {
                    case "PORTRAIT":
                        camera.Type = M2Camera.CameraType.Portrait;
                        break;
                    case "PAPERDOLL":
                        camera.Type = M2Camera.CameraType.CharacterInfo;
                        break;
                    default:
                        continue;
                }

                cameras.Add(camera);
            }

            //Add missing CharacterInfo camera
            if (!cameras.Any(x => x.Type == M2Camera.CameraType.CharacterInfo) && cameras.Any(x => x.Type == M2Camera.CameraType.Portrait))
            {
                var c = cameras.First(x => x.Type == M2Camera.CameraType.Portrait);
                M2Camera camera = new M2Camera()
                {
                    FarClip = c.FarClip,
                    FieldOfView = c.FieldOfView,
                    NearClip = c.NearClip,
                    PositionBase = c.PositionBase,
                    TargetPositionBase = c.TargetPositionBase,
                    Type = M2Camera.CameraType.CharacterInfo
                };

                cameras.Add(camera);
            }

            return cameras;
        }

        public M2Array<short> GetTextureLookup()
        {
            return _model.Get<MTLS>().SelectMany(x => x.Layers).Select(x => (short)x.TextureId).Distinct().ToM2Array();
        }

        public M2Array<M2TextureTransform> GetTextureTransform()
        {
            M2Array<M2TextureTransform> transforms = new M2Array<M2TextureTransform>();
            if (!_model.Has<TXAN>())
                return transforms;

            foreach (var txan in _model.Get<TXAN>())
            {
                var anim = new M2TextureTransform();
                txan.RotationKeys?.PopulateM2Track(anim.Rotation, _model.Get<SEQS>());
                txan.ScaleKeys?.PopulateM2Track(anim.Scale, _model.Get<SEQS>());
                txan.TranslationKeys?.PopulateM2Track(anim.Translation, _model.Get<SEQS>());
                transforms.Add(anim);
            }

            return transforms;
        }

        public M2Array<M2Color> GetColors()
        {
            M2Array<M2Color> colors = new M2Array<M2Color>();
            if (!_model.Has<GEOA>())
                return colors;

            foreach (var geoa in _model.Get<GEOA>())
            {
                var color = new M2Color();

                //geoa.AlphaTrack?.PopulateM2Track(color.Alpha, _model.Get<SEQS>());

                if (geoa.ColorKeys != null)
                {
                    geoa.ColorKeys.PopulateM2Track(color.Color, _model.Get<SEQS>());
                }
                else
                {
                    color.Color.Timestamps.Add(new M2Array<uint>() { 0 });
                    color.Color.Values.Add(new M2Array<C3Vector>() { geoa.Color.ToC3Vector });
                }

                colors.Add(color);
            }
            return colors;
        }

        public M2Array<M2TextureWeight> GetTextureWeights()
        {
            M2Array<M2TextureWeight> textureweights = new M2Array<M2TextureWeight>();
            if (!_model.Has<MTLS>())
                return textureweights;

            foreach (var lay in _model.Get<MTLS>().SelectMany(x => x.Layers))
            {
                M2TextureWeight weight = new M2TextureWeight();
                if (lay.AlphaKeys != null)
                {
                    lay.AlphaKeys.PopulateM2Track(weight.Weight, _model.Get<SEQS>());
                }
                else
                {
                    weight.Weight.Timestamps.Add(new M2Array<uint>() { 0 });
                    weight.Weight.Values.Add(new M2Array<FixedPoint_0_15>() { new FixedPoint_0_15(lay.Alpha.ToShort()) });
                }

                textureweights.Add(weight);
            }

            return textureweights;
        }

        public M2Array<short> GetTexUnitLookup()
        {
            var mats = _model.Get<GEOS>().Select(x => (short)x.MaterialId).ToList();
            while (mats.Count < Model.Textures.Count)
                mats.Add(-1);

            return mats.ToM2Array();
        }


        public M2Array<short> GetBoneLookup()
        {
            List<short> Bones = new List<short>();

            foreach (var geo in _model.Get<GEOS>())
            {
                foreach (var triangle in geo.FaceVertices)
                {
                    var tri = triangle.ToArray();

                    short[] triBones = new short[12];
                    for (int i = 0; i < 3; i++) //Iterate triangle
                    {
                        byte[] vertexproperty = new byte[4];
                        var vertex = Model.GlobalVertexList[tri[i]];

                        for (int j = 0; j < 4; j++) //Iterate vertices
                        {
                            if (vertex.BoneWeights[j] != 0)
                            {
                                if (!Bones.Contains(vertex.BoneIndices[j]))
                                    Bones.Add(vertex.BoneIndices[j]); //Store Bone if new

                                vertexproperty[j] = (byte)Bones.IndexOf(vertex.BoneIndices[j]); //Build Vertex Property
                            }
                        }

                        Model.Views[0].Properties.Add(new VertexProperty(vertexproperty));
                    }
                }
            }

            var result = new M2Array<short>();
            for (int i = 0; i < 4; i++)
                result.AddRange(Bones);

            return result;
        }

    }
}
