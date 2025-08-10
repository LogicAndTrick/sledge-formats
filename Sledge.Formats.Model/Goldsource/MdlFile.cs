using Sledge.Formats.FileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Sledge.Formats.Model.Goldsource
{
    public class MdlFile
    {
        public Header Header { get; set; }
        public List<Bone> Bones { get; set; }
        public List<BoneController> BoneControllers { get; set; }
        public List<Hitbox> Hitboxes { get; set; }
        public List<Sequence> Sequences { get; set; }
        public List<SequenceGroup> SequenceGroups { get; set; }
        public List<Texture> Textures { get; set; }
        public List<SkinFamily> Skins { get; set; }
        public List<BodyPart> BodyParts { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<Transition> Transitions { get; set; }

        public MdlFile(IEnumerable<Stream> streams, bool leaveOpen = false)
        {
            Bones = new List<Bone>();
            BoneControllers = new List<BoneController>();
            Hitboxes = new List<Hitbox>();
            Sequences = new List<Sequence>();
            SequenceGroups = new List<SequenceGroup>();
            Textures = new List<Texture>();
            Skins = new List<SkinFamily>();
            BodyParts = new List<BodyPart>();
            Attachments = new List<Attachment>();
            Transitions = new List<Transition>();

            var readers = streams.Select(x => new BinaryReader(x, Encoding.ASCII, leaveOpen)).ToList();
            try
            {
                Read(readers);
            }
            finally
            {
                readers.ForEach(x => x.Dispose());
            }
        }

        public static MdlFile FromFile(string path) 
        {
            var dir = Path.GetDirectoryName(path);
            var fname = Path.GetFileName(path);

            var resolver = new DiskFileResolver(dir);
            return FromFile(resolver, fname);
        }

        public static MdlFile FromFile(IFileResolver resolver, string path)
        {
            var basedir = (Path.GetDirectoryName(path) ?? "").Replace('\\', '/');
            if (basedir.Length > 0 && !basedir.EndsWith("/")) basedir += "/";
            var basepath = basedir + Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            var streams = new List<Stream>();
            try
            {
                streams.Add(resolver.OpenFile(path));
                
                var tfile = basepath + "t" + ext;
                if (resolver.FileExists(tfile)) streams.Add(resolver.OpenFile(tfile));

                for (var i = 1; i < 32; i++)
                {
                    var sfile = basepath + i.ToString("00") + ext;
                    if (resolver.FileExists(sfile)) streams.Add(resolver.OpenFile(sfile));
                    else break;
                }

                return new MdlFile(streams);
            }
            finally
            {
                foreach (var s in streams) s.Dispose();
            }
        }

        private static readonly HashSet<Version> KnownVersions = new HashSet<Version>(Enum.GetValues(typeof(Version)).OfType<Version>());

        /// <summary>
        /// Check if the given stream is likely to be valid for this format. Advances the stream, does not dispose it.
        /// </summary>
        /// <param name="stream">The stream to check. Will not be disposed, and will be advanced.</param>
        /// <returns>True if this stream is likely to be valid for this format.</returns>
        public static bool CanRead(Stream stream)
        {
            try
            {
                using (var br = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var id = (ID)br.ReadInt32();
                    var version = (Version)br.ReadInt32();
                    return id == ID.Idst && KnownVersions.Contains(version);
                }
            }
            catch
            {
                return false;
            }
        }

        private void Read(IEnumerable<BinaryReader> readers)
        {
            var main = new List<BinaryReader>();
            var sequenceGroups = new Dictionary<string, BinaryReader>();

            foreach (var br in readers)
            {
                var id = (ID)br.ReadInt32();
                var version = (Version)br.ReadInt32();

                if (version != Version.Goldsource)
                {
                    throw new NotSupportedException("Only Goldsource (v10) MDL files are supported.");
                }

                if (id != ID.Idsq && id != ID.Idst)
                {
                    throw new NotSupportedException("Only Goldsource (v10) MDL files are supported.");
                }

                if (id == ID.Idst)
                {
                    main.Add(br);
                }
                else
                {
                    var name = br.ReadFixedLengthString(Encoding.ASCII, 64);
                    sequenceGroups[name] = br;
                }
            }

            foreach (var br in main)
            {
                Read(br, sequenceGroups);
            }
        }

        #region Reading
        
        private void Read(BinaryReader br, Dictionary<string, BinaryReader> sequenceGroups)
        {
            var header = new Header
            {
                ID = ID.Idst,
                Version = Version.Goldsource,
                Name = br.ReadFixedLengthString(Encoding.ASCII, 64),
                Size = br.ReadInt32(),
                EyePosition = br.ReadVector3(),
                HullMin = br.ReadVector3(),
                HullMax = br.ReadVector3(),
                BoundingBoxMin = br.ReadVector3(),
                BoundingBoxMax = br.ReadVector3(),
                Flags = br.ReadInt32()
            };

            // Read all the nums/offsets from the header
            var sections = new int[(int)Section.NumSections][];
            for (var i = 0; i < (int) Section.NumSections; i++)
            {
                var sec = (Section) i;

                int indexNum;
                if (sec == Section.Texture || sec == Section.Skin) indexNum = 3;
                else indexNum = 2;

                sections[i] = new int[indexNum];
                for (var j = 0; j < indexNum; j++)
                {
                    sections[i][j] = br.ReadInt32();
                }
            }

            // Bones
            var num = SeekToSection(br, Section.Bone, sections);
            var numBones = num;
            for (var i = 0; i < num; i++)
            {
                var bone = new Bone
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 32),
                    Parent = br.ReadInt32(),
                    Flags = br.ReadInt32(),
                    Controllers = br.ReadIntArray(6),
                    Position = br.ReadVector3(),
                    Rotation = br.ReadVector3(),
                    PositionScale = br.ReadVector3(),
                    RotationScale = br.ReadVector3()
                };
                Bones.Add(bone);
            }

            // Assume this is a texture file if there's no bones, only overwrite the header when this isn't the case
            if (numBones > 0 || Header == null)
            {
                Header = header;
            }

            // Bone controllers
            num = SeekToSection(br, Section.BoneController, sections);
            for (var i = 0; i < num; i++)
            {
                var boneController = new BoneController
                {
                    Bone = br.ReadInt32(),
                    Type = br.ReadInt32(),
                    Start = br.ReadSingle(),
                    End = br.ReadSingle(),
                    Rest = br.ReadInt32(),
                    Index = br.ReadInt32()
                };
                BoneControllers.Add(boneController);
            }

            // Hitboxes
            num = SeekToSection(br, Section.Hitbox, sections);
            for (var i = 0; i < num; i++)
            {
                var hitbox = new Hitbox
                {
                    Bone = br.ReadInt32(),
                    Group = br.ReadInt32(),
                    Min = br.ReadVector3(),
                    Max = br.ReadVector3()
                };
                Hitboxes.Add(hitbox);
            }

            // Sequence groups
            num = SeekToSection(br, Section.SequenceGroup, sections);
            for (var i = 0; i < num; i++)
            {
                var group = new SequenceGroup
                {
                    Label = br.ReadFixedLengthString(Encoding.ASCII, 32),
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 64)
                };
                br.ReadBytes(8); // unused
                SequenceGroups.Add(group);
            }

            // Sequences
            num = SeekToSection(br, Section.Sequence, sections);
            for (var i = 0; i < num; i++)
            {
                var sequence = new Sequence
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 32),
                    Framerate = br.ReadSingle(),
                    Flags = br.ReadInt32(),
                    Activity = br.ReadInt32(),
                    ActivityWeight = br.ReadInt32(),
                    NumEvents = br.ReadInt32(),
                    EventIndex = br.ReadInt32(),
                    NumFrames = br.ReadInt32(),
                    NumPivots = br.ReadInt32(),
                    PivotIndex = br.ReadInt32(),
                    MotionType = br.ReadInt32(),
                    MotionBone = br.ReadInt32(),
                    LinearMovement = br.ReadVector3(),
                    AutoMovePositionIndex = br.ReadInt32(),
                    AutoMoveAngleIndex = br.ReadInt32(),
                    Min = br.ReadVector3(),
                    Max = br.ReadVector3(),
                    NumBlends = br.ReadInt32(),
                    AnimationIndex = br.ReadInt32(),
                    BlendType = br.ReadIntArray(2),
                    BlendStart = br.ReadSingleArray(2),
                    BlendEnd = br.ReadSingleArray(2),
                    BlendParent = br.ReadInt32(),
                    SequenceGroup = br.ReadInt32(),
                    EntryNode = br.ReadInt32(),
                    ExitNode = br.ReadInt32(),
                    NodeFlags = br.ReadInt32(),
                    NextSequence = br.ReadInt32()
                };

                var seqGroup = SequenceGroups[sequence.SequenceGroup];

                // Load sequence group 0 from the main file, the other sequence groups are in sub files
                if (sequence.SequenceGroup == 0)
                {
                    var pos = br.BaseStream.Position;
                    sequence.Blends = ReadAnimationBlends(br, sequence, numBones);
                    br.BaseStream.Position = pos;
                }
                else if (sequenceGroups.TryGetValue(seqGroup.Name, out var group))
                {
                    sequence.Blends = ReadAnimationBlends(group, sequence, numBones);
                }

                // read events and pivot data
                {
                    var pos = br.BaseStream.Position;

                    br.BaseStream.Seek(sequence.EventIndex, SeekOrigin.Begin);
                    sequence.Events = new AnimationEvent[sequence.NumEvents];
                    for (var ei = 0; ei < sequence.NumEvents; ei++)
                    {
                        sequence.Events[ei] = new AnimationEvent
                        {
                            Frame = br.ReadInt32(),
                            Event = br.ReadInt32(),
                            Type = br.ReadInt32(),
                            Options = br.ReadFixedLengthString(Encoding.ASCII, 64)
                        };
                    }

                    br.BaseStream.Seek(sequence.PivotIndex, SeekOrigin.Begin);
                    sequence.Pivots = new Pivot[sequence.NumPivots];
                    for (var pi = 0; pi < sequence.NumPivots; pi++)
                    {
                        sequence.Pivots[pi] = new Pivot
                        {
                            Origin = br.ReadVector3(),
                            Start = br.ReadInt32(),
                            End = br.ReadInt32()
                        };
                    }

                    br.BaseStream.Position = pos;
                }

                Sequences.Add(sequence);
            }

            // Textures
            num = SeekToSection(br, Section.Texture, sections);
            var firstTextureIndex = Textures.Count;
            for (var i = 0; i < num; i++)
            {
                var texture = new Texture
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 64),
                    Flags = (TextureFlags) br.ReadInt32(),
                    Width = br.ReadInt32(),
                    Height = br.ReadInt32(),
                    Index = br.ReadInt32()
                };
                Textures.Add(texture);
            }

            // Texture data
            for (var i = firstTextureIndex; i < firstTextureIndex + num; i++)
            {
                var t = Textures[i];
                br.BaseStream.Position = t.Index;
                t.Data = br.ReadBytes(t.Width * t.Height);
                t.Palette = br.ReadBytes(256 * 3);
                Textures[i] = t;
            }

            // Skins
            var skinSection = sections[(int)Section.Skin];
            var numSkinRefs = skinSection[0];
            var numSkinFamilies = skinSection[1];
            br.BaseStream.Seek(skinSection[2], SeekOrigin.Begin);
            for (var i = 0; i < numSkinFamilies; i++)
            {
                var skin = new SkinFamily
                {
                    Textures = br.ReadShortArray(numSkinRefs)
                };
                Skins.Add(skin);
            }
            
            // Body parts
            num = SeekToSection(br, Section.BodyPart, sections);
            for (var i = 0; i < num; i++)
            {
                var part = new BodyPart
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 64),
                    NumModels = br.ReadInt32(),
                    Base = br.ReadInt32(),
                    ModelIndex = br.ReadInt32()
                };
                var pos = br.BaseStream.Position;
                part.Models = LoadModels(br, part);
                br.BaseStream.Position = pos;
                BodyParts.Add(part);
            }

            // Attachments
            num = SeekToSection(br, Section.Attachment, sections);
            for (var i = 0; i < num; i++)
            {
                var attachment = new Attachment
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 32),
                    Type = br.ReadInt32(),
                    Bone = br.ReadInt32(),
                    Origin = br.ReadVector3(),
                    Vectors = br.ReadVector3Array(3)
                };
                Attachments.Add(attachment);
            }

            // Transitions
            num = SeekToSection(br, Section.Transition, sections);
            var transitions = br.ReadBytes(num * num);
            for (var i = 0; i < num; i++)
            {
                for (var j = 0; j < num; j++)
                {
                    Transitions.Add(new Transition
                    {
                        FromNode = i + 1,
                        ToNode = j + 1,
                        ViaNode = transitions[i * num + j]
                    });
                }
            }

            // Sounds & Sound groups aren't used
        }

        private static int SeekToSection(BinaryReader br, Section section, int[][] sections)
        {
            var s = sections[(int)section];
            br.BaseStream.Seek(s[1], SeekOrigin.Begin);
            return s[0];
        }

        #endregion

        #region Reading animations

        private static Blend[] ReadAnimationBlends(BinaryReader br, Sequence sequence, int numBones)
        {
            var blends = new Blend[sequence.NumBlends];
            var blendLength = 6 * numBones;

            br.BaseStream.Seek(sequence.AnimationIndex, SeekOrigin.Begin);
            
            var animPosition = br.BaseStream.Position;
            var offsets = br.ReadUshortArray(blendLength * sequence.NumBlends);
            for (var i = 0; i < sequence.NumBlends; i++)
            {
                var blendOffsets = new ushort[blendLength];
                Array.Copy(offsets, blendLength * i, blendOffsets, 0, blendLength);

                var startPosition = animPosition + i * blendLength * 2;
                blends[i] = new Blend
                {
                    Frames = ReadAnimationFrames(br, sequence, numBones, startPosition, blendOffsets)
                };
            }

            return blends;
        }

        internal static AnimationFrame[] ReadAnimationFrames(BinaryReader br, Sequence sequence, int numBones, long startPosition, ushort[] boneOffsets)
        {
            var frames = new AnimationFrame[sequence.NumFrames];
            for (var i = 0; i < frames.Length; i++)
            {
                frames[i] = new AnimationFrame
                {
                    Positions = new Vector3[numBones],
                    Rotations = new Vector3[numBones]
                };
            }
            
            for (var i = 0; i < numBones; i++)
            {
                var boneValues = new short[6][];
                for (var j = 0; j < 6; j++)
                {
                    var offset = boneOffsets[i * 6 + j];
                    if (offset <= 0)
                    {
                        boneValues[j] = new short[sequence.NumFrames];
                        continue;
                    }

                    br.BaseStream.Seek(startPosition + i * 6 * 2 + offset, SeekOrigin.Begin);
                    boneValues[j] = ReadAnimationFrameValues(br, sequence.NumFrames);
                }

                for (var j = 0; j < sequence.NumFrames; j++)
                {
                    frames[j].Positions[i] = new Vector3(boneValues[0][j], boneValues[1][j], boneValues[2][j]);
                    frames[j].Rotations[i] = new Vector3(boneValues[3][j], boneValues[4][j], boneValues[5][j]);
                }
            }

            return frames;
        }

        internal static short[] ReadAnimationFrameValues(BinaryReader br, int count)
        {
            /*
             * RLE data:
             * byte compressed_length - compressed number of values in the data
             * byte uncompressed_length - uncompressed number of values in run
             * short values[compressed_length] - values in the run, the last value is repeated to reach the uncompressed length
             */
            var values = new short[count];

            for (var i = 0; i < count; /* i = i */)
            {
                var run = br.ReadBytes(2); // read the compressed and uncompressed lengths
                var vals = br.ReadShortArray(run[0]); // read the compressed data
                for (var j = 0; j < run[1] && i < count; i++, j++)
                {
                    var idx = Math.Min(run[0] - 1, j); // value in the data or the last value if we're past the end
                    values[i] = vals[idx];
                }
            }

            return values;
        }

        #endregion

        #region Reading models

        private static Model[] LoadModels(BinaryReader br, BodyPart part)
        {
            br.BaseStream.Seek(part.ModelIndex, SeekOrigin.Begin);

            var models = new Model[part.NumModels];
            for (var i = 0; i < part.NumModels; i++)
            {
                var model = new Model
                {
                    Name = br.ReadFixedLengthString(Encoding.ASCII, 64),
                    Type = br.ReadInt32(),
                    Radius = br.ReadSingle(),
                    NumMesh = br.ReadInt32(),
                    MeshIndex = br.ReadInt32(),
                    NumVerts = br.ReadInt32(),
                    VertInfoIndex = br.ReadInt32(),
                    VertIndex = br.ReadInt32(),
                    NumNormals = br.ReadInt32(),
                    NormalInfoIndex = br.ReadInt32(),
                    NormalIndex = br.ReadInt32(),
                    NumGroups = br.ReadInt32(),
                    GroupIndex = br.ReadInt32()
                };

                var pos = br.BaseStream.Position;
                model.Meshes = ReadMeshes(br, model);
                br.BaseStream.Position = pos;

                models[i] = model;
            }

            return models;
        }

        private static Mesh[] ReadMeshes(BinaryReader br, Model model)
        {
            var meshes = new Mesh[model.NumMesh];

            // Read all the vertex data
            br.BaseStream.Position = model.VertInfoIndex;
            var vertexBones = br.ReadBytes(model.NumVerts);

            br.BaseStream.Position = model.NormalInfoIndex;
            var normalBones = br.ReadBytes(model.NumNormals);

            br.BaseStream.Position = model.VertIndex;
            var vertices = br.ReadVector3Array(model.NumVerts);

            br.BaseStream.Position = model.NormalIndex;
            var normals = br.ReadVector3Array(model.NumNormals);

            // Read the meshes
            br.BaseStream.Position = model.MeshIndex;
            for (var i = 0; i < model.NumMesh; i++)
            {
                var mesh = new Mesh
                {
                    NumTriangles = br.ReadInt32(),
                    TriangleIndex = br.ReadInt32(),
                    SkinRef = br.ReadInt32(),
                    NumNormals = br.ReadInt32(),
                    NormalIndex = br.ReadInt32()
                };
                meshes[i] = mesh;
            }

            // Read the triangle data
            for (var i = 0; i < model.NumMesh; i++)
            {
                meshes[i].Vertices = ReadTriangles(br, meshes[i], vertices, vertexBones, normals, normalBones);
            }

            return meshes;
        }

        private static MeshVertex[] ReadTriangles(BinaryReader br, Mesh mesh, Vector3[] vertices, byte[] vertexBones, Vector3[] normals, byte[] normalBones)
        {
            /*
             * Mesh data
             * short type - abs(type) is the length of the run
             *   - < 0 = triangle fan,
             *   - > 0 = triangle strip
             *   - 0 = end of list
             * short vertex - vertex index
             * short normal - normal index
             * short u, short v - texture coordinates
             */

            var meshVerts = new MeshVertex[mesh.NumTriangles * 3];
            var vi = 0;

            br.BaseStream.Position = mesh.TriangleIndex;

            short type;
            while ((type = br.ReadInt16()) != 0)
            {
                var fan = type < 0;
                var length = Math.Abs(type);
                var pointData = br.ReadShortArray(4 * length);
                for (var i = 0; i < length - 2; i++)
                {
                    //                    | TRIANGLE FAN    |                       | TRIANGLE STRIP (ODD) |         | TRIANGLE STRIP (EVEN) |
                    var add = fan ? new[] { 0, i + 1, i + 2 } : (i % 2 == 1 ? new[] { i + 1, i, i + 2      } : new[] { i, i + 1, i + 2       });
                    foreach (var idx in add)
                    {
                        var vert = pointData[idx * 4 + 0];
                        var norm = pointData[idx * 4 + 1];
                        var s = pointData[idx * 4 + 2];
                        var t = pointData[idx * 4 + 3];
                        
                        meshVerts[vi++] = new MeshVertex
                        {
                            VertexBone = vertexBones[vert],
                            NormalBone = normalBones[norm],
                            Vertex = vertices[vert],
                            Normal = normals[norm],
                            Texture = new Vector2(s, t)
                        };
                    }
                }
            }

            return meshVerts;
        }

        #endregion

        /// <summary>
        /// Write the model to data streams.
        /// </summary>
        /// <param name="name">The name of the model. This is NOT the path on disk to the file, but rather the name of the model without the file extension. For example, for 'cube.mdl', this should be 'cube'. The header will be updated to match this name.</param>
        /// <param name="options">Options for writing the file</param>
        /// <returns>The output with information about the files. This must be disposed by the caller.</returns>
        public MdlWriteOutput Write(string name, MdlFileWriteOptions options = null)
        {
            options = options ?? new MdlFileWriteOptions();

            var str = new MemoryStream();
            var bw = new BinaryWriter(str, Encoding.ASCII, true);

            var result = new MdlWriteOutput();
            var baseFile = new MdlWriteOutputFile
            {
                Type = MdlWriteOutputType.Base,
                Suffix = "",
                FileNumber = 0,
                Stream = str
            };
            result.Files.Add(baseFile);

            Header.Name = name;

            // all sequence groups aside from 0 have their animation frames written to external files
            for (var i = 1; i < SequenceGroups.Count; i++)
            {
                var sg = SequenceGroups[i];
                var filename = $"{name}{i:00}";
                sg.Name = $"models\\{filename}.mdl";

                var seqStr = new MemoryStream();
                var seqFile = new MdlWriteOutputFile
                {
                    Type = MdlWriteOutputType.Sequence,
                    Suffix = i.ToString("00"),
                    FileNumber = i,
                    Stream = seqStr
                };
                result.Files.Add(seqFile);

                using (var seqBw = new BinaryWriter(seqStr, Encoding.ASCII, true))
                {
                    WriteSequenceFile(seqBw, filename, i);
                }
            }

            var splitTextures = options.ShouldSplitTextures(this);
            if (splitTextures)
            {
                WriteFile(bw, Header, WriteType.DataOnly);

                var texHeader = new Header
                {
                    ID = ID.Idst,
                    Version = Version.Goldsource
                };
                var texStr = new MemoryStream();
                var texFile = new MdlWriteOutputFile
                {
                    Type = MdlWriteOutputType.Texture,
                    Suffix = "t",
                    FileNumber = -1,
                    Stream = texStr
                };
                result.Files.Add(texFile);

                using (var texBw = new BinaryWriter(texStr, Encoding.ASCII, true))
                {
                    WriteFile(texBw, texHeader, WriteType.TexturesOnly);
                }
            }
            else
            {
                WriteFile(bw, Header, WriteType.DataAndTextures);
            }

            bw.Dispose();

            return result;
        }

        #region Writing

        private enum WriteType
        {
            DataAndTextures,
            DataOnly,
            TexturesOnly
        }

        private void WriteFile(BinaryWriter bw, Header hdr, WriteType writeType)
        {
            WriteHeader(bw, hdr);

            // an array for section offsets
            var sections = new int[(int)Section.NumSections][];
            var numSectionInts = sections.Length * 2 + 2; // textures and skins have 3 ints, all others have 2
            var sectionsPos = bw.BaseStream.Position;
            bw.Write(new byte[numSectionInts * sizeof(int)]); // this will write all zeros, we'll rewrite it again later

            // setup all sections with zeroes
            var pos = (int)bw.BaseStream.Position;
            for (var i = 0; i < sections.Length; i++)
            {
                var section = (Section)i;
                if (section == Section.Texture) sections[i] = new[] { 0, pos, pos };
                else if (section == Section.Skin) sections[i] = new[] { 0, 0, pos };
                else sections[i] = new[] { 0, pos };
            }

            // write each section in the same order as studiomdl
            if (writeType == WriteType.DataAndTextures || writeType == WriteType.DataOnly)
            {
                sections[(int)Section.Bone] = WriteBones(bw);
                sections[(int)Section.BoneController] = WriteBoneControllers(bw);
                sections[(int)Section.Attachment] = WriteAttachments(bw);
                sections[(int)Section.Hitbox] = WriteHitboxes(bw);

                // write the animation frames for sequence group 0
                foreach (var seq in Sequences.Where(x => x.SequenceGroup == 0))
                {
                    seq.AnimationIndex = (int)bw.BaseStream.Position;
                    WriteAnimationBlends(bw, seq, Bones.Count);
                }

                sections[(int)Section.Sequence] = WriteSequences(bw);
                sections[(int)Section.SequenceGroup] = WriteSequenceGroups(bw);
                sections[(int)Section.Transition] = WriteTransitions(bw);
                sections[(int)Section.BodyPart] = WriteBodyParts(bw);
            }

            if (writeType == WriteType.DataAndTextures || writeType == WriteType.TexturesOnly)
            {
                sections[(int)Section.Skin] = WriteSkins(bw);
                sections[(int)Section.Texture] = WriteTextures(bw);
            }

            // section values are known now, go back and write them
            bw.BaseStream.Seek(sectionsPos, SeekOrigin.Begin);
            foreach (var s in sections)
            {
                foreach (var i in s)
                {
                    bw.Write(i);
                }
            }
            bw.BaseStream.Seek(0, SeekOrigin.End);

            // final size is known now, write it back into the header
            SetSize(bw);
        }

        private void WriteSequenceFile(BinaryWriter bw, string filename, int sequenceGroup)
        {
            const int nameLength = 64;
            const int sizeOffset = sizeof(int) + sizeof(int) + nameLength;

            bw.Write((int) ID.Idsq);
            bw.Write((int) Version.Goldsource);
            bw.WriteFixedLengthString(Encoding.ASCII, nameLength, filename);
            bw.Write(0); // length, written after we've finished writing

            foreach (var seq in Sequences.Where(x => x.SequenceGroup == sequenceGroup))
            {
                seq.AnimationIndex = (int)bw.BaseStream.Position;
                WriteAnimationBlends(bw, seq, Bones.Count);
            }

            // go back and write the size
            var size = (int) bw.BaseStream.Position;
            bw.Seek(sizeOffset, SeekOrigin.Begin);
            bw.Write(size);

            bw.Seek(0, SeekOrigin.End);
        }

        private static void WriteHeader(BinaryWriter bw, Header hdr)
        {
            bw.Write((int) hdr.ID);
            bw.Write((int) hdr.Version);
            bw.WriteFixedLengthString(Encoding.ASCII, 64, hdr.Name);
            bw.Write((int) 0); // size - computed at the end
            bw.WriteVector3(hdr.EyePosition);
            bw.WriteVector3(hdr.HullMin);
            bw.WriteVector3(hdr.HullMax);
            bw.WriteVector3(hdr.BoundingBoxMin);
            bw.WriteVector3(hdr.BoundingBoxMax);
            bw.Write(hdr.Flags);
        }

        /// <summary>
        /// Update the size field in the header to match the size of the stream
        /// </summary>
        private static void SetSize(BinaryWriter bw)
        {
            var pos = bw.BaseStream.Position;
            bw.BaseStream.Seek(0, SeekOrigin.End);
            var size = bw.BaseStream.Position;
            bw.BaseStream.Seek(4 + 4 + 64, SeekOrigin.Begin);
            bw.Write((int)size);
            bw.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        private int[] WriteBones(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            foreach (var bone in Bones)
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 32, bone.Name);
                bw.Write(bone.Parent);
                bw.Write(bone.Flags);
                bw.WriteIntArray(6, bone.Controllers);
                bw.WriteVector3(bone.Position);
                bw.WriteVector3(bone.Rotation);
                bw.WriteVector3(bone.PositionScale);
                bw.WriteVector3(bone.RotationScale);
            }
            return new [] { Bones.Count, startPos };
        }

        private int[] WriteBoneControllers(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            foreach (var bc in BoneControllers)
            {
                bw.Write(bc.Bone);
                bw.Write(bc.Type);
                bw.Write(bc.Start);
                bw.Write(bc.End);
                bw.Write(bc.Rest);
                bw.Write(bc.Index);
            }
            return new [] { BoneControllers.Count, startPos };
        }

        private int[] WriteHitboxes(BinaryWriter bw)
        {
            var startPos = (int) bw.BaseStream.Position;
            foreach (var hb in Hitboxes)
            {
                bw.Write(hb.Bone);
                bw.Write(hb.Group);
                bw.WriteVector3(hb.Min);
                bw.WriteVector3(hb.Max);
            }
            return new[] { Hitboxes.Count, startPos };
        }

        private int[] WriteSequences(BinaryWriter bw)
        {
            var startPos = (int) bw.BaseStream.Position;

            const int sequenceSize = 176;

            // write a blank sequence, we'll fill it in after writing events/piots/animations
            var buffer = new byte[sequenceSize];
            foreach (var seq in Sequences)
            {
                bw.Write(buffer);
            }

            // now write all the binary data for each sequence, storing the updated data indices
            foreach (var seq in Sequences.Where(x => x.SequenceGroup == 0))
            {
                seq.EventIndex = (int)bw.BaseStream.Position;
                WriteEvents(bw, seq);

                seq.PivotIndex = (int)bw.BaseStream.Position;
                WritePivots(bw, seq);

                if (seq.SequenceGroup == 0)
                {
                    // only do animations for sequence group 0, the others are already written to external files and the index updated
                    seq.AnimationIndex = (int)bw.BaseStream.Position;
                    WriteAnimationBlends(bw, seq, Bones.Count);
                }
            }

            var endPos = bw.BaseStream.Position;

            // go back and write the actual sequence info
            bw.BaseStream.Seek(startPos, SeekOrigin.Begin);
            foreach (var seq in Sequences)
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 32, seq.Name);
                bw.Write(seq.Framerate);
                bw.Write(seq.Flags);
                bw.Write(seq.Activity);
                bw.Write(seq.ActivityWeight);
                bw.Write(seq.NumEvents);
                bw.Write(seq.EventIndex);
                bw.Write(seq.NumFrames);
                bw.Write(seq.NumPivots);
                bw.Write(seq.PivotIndex);
                bw.Write(seq.MotionType);
                bw.Write(seq.MotionBone);
                bw.WriteVector3(seq.LinearMovement);
                bw.Write(seq.AutoMovePositionIndex);
                bw.Write(seq.AutoMoveAngleIndex);
                bw.WriteVector3(seq.Min);
                bw.WriteVector3(seq.Max);
                bw.Write(seq.NumBlends);
                bw.Write(seq.AnimationIndex);
                bw.WriteIntArray(2, seq.BlendType);
                bw.WriteSingleArray(2, seq.BlendStart);
                bw.WriteSingleArray(2, seq.BlendEnd);
                bw.Write(seq.BlendParent);
                bw.Write(seq.SequenceGroup);
                bw.Write(seq.EntryNode);
                bw.Write(seq.ExitNode);
                bw.Write(seq.NodeFlags);
                bw.Write(seq.NextSequence);
            }

            bw.BaseStream.Seek(endPos, SeekOrigin.Begin);
            return new[] { Sequences.Count, startPos };
        }

        private void WriteEvents(BinaryWriter bw, Sequence seq)
        {
            foreach (var ev in seq.Events)
            {
                bw.Write(ev.Frame);
                bw.Write(ev.Event);
                bw.Write(ev.Type);
                bw.WriteFixedLengthString(Encoding.ASCII, 64, ev.Options);
            }
        }

        private void WritePivots(BinaryWriter bw, Sequence seq)
        {
            foreach (var pivot in seq.Pivots)
            {
                bw.WriteVector3(pivot.Origin);
                bw.Write(pivot.Start);
                bw.Write(pivot.End);
            }
        }

        private int[] WriteSequenceGroups(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            foreach (var group in SequenceGroups)
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 32, group.Label);
                bw.WriteFixedLengthString(Encoding.ASCII, 64, group.Name);
                bw.Write(new byte[8]); // unused
            }
            return new[] { SequenceGroups.Count, startPos };
        }

        private int[] WriteTextures(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;

            // write textures
            foreach (var tex in Textures)
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 64, tex.Name);
                bw.Write((int) tex.Flags);
                bw.Write(tex.Width);
                bw.Write(tex.Height);
                bw.Write(tex.Index);
            }

            var dataPos = (int)bw.BaseStream.Position;

            // write texture data
            foreach (var tex in Textures)
            {
                tex.Index = (int)bw.BaseStream.Position;
                bw.Write(tex.Data);
                bw.Write(tex.Palette);
            }

            var endPos = (int)bw.BaseStream.Position;

            // go back and write the texture indices
            bw.BaseStream.Seek(startPos, SeekOrigin.Begin);
            foreach (var tex in Textures)
            {
                bw.Seek(64 + sizeof(int) + 3, SeekOrigin.Current);
                bw.Write(tex.Index);
            }

            bw.Seek(endPos, SeekOrigin.Begin);
            return new[] { Textures.Count, startPos, dataPos };
        }

        private int[] WriteSkins(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            var numSkinFamilies = Skins.Count;
            var numSkinRefs = numSkinFamilies == 0 ? 0 : Skins[0].Textures.Length;
            foreach (var skin in Skins)
            {
                bw.WriteShortArray(numSkinRefs, skin.Textures);
            }
            return new[] { numSkinRefs, numSkinFamilies, startPos };
        }

        private int[] WriteAttachments(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            foreach (var att in Attachments)
            {
                bw.WriteFixedLengthString(Encoding.ASCII, 32, att.Name);
                bw.Write(att.Type);
                bw.Write(att.Bone);
                bw.WriteVector3(att.Origin);
                bw.WriteVector3Array(3, att.Vectors);
            }
            return new[] { Attachments.Count, startPos };
        }

        private int[] WriteTransitions(BinaryWriter bw)
        {
            var startPos = (int)bw.BaseStream.Position;
            var numNodes = 0;
            if (Sequences.Any() && Transitions.Any())
            {
                numNodes = Sequences.Select(x => x.ExitNode)
                    .Union(Sequences.Select(x => x.EntryNode))
                    .Union(Transitions.Select(x => x.FromNode))
                    .Union(Transitions.Select(x => x.ToNode))
                    .Union(Transitions.Select(x => (int) x.ViaNode))
                    .Max();
                var groupedTransitions = Transitions.GroupBy(x => x.FromNode).ToDictionary(x => x.Key, x => x.ToList());
                for (var i = 0; i < numNodes; i++)
                {
                    var group = groupedTransitions.TryGetValue(i + 1, out var g) ? g : new List<Transition>();
                    for (var j = 0; j < numNodes; j++)
                    {
                        var v = group.FirstOrDefault(x => x.ToNode == j + 1);
                        byte val = v?.ViaNode ?? 0;
                        bw.Write(val);
                    }
                }
            }
            return new[] { numNodes, startPos };
        }

        #endregion

        #region Writing animations

        private static void WriteAnimationBlends(BinaryWriter bw, Sequence seq, int numBones)
        {
            var pos = bw.BaseStream.Position;

            var blendLength = 6 * numBones;
            var offsets = new ushort[blendLength * seq.NumBlends];

            // skip the offsets for now, we'll come back to write them
            bw.BaseStream.Seek(sizeof(ushort) * offsets.Length, SeekOrigin.Current);

            for (var i = 0; i < seq.Blends.Length; i++)
            {
                var blend = seq.Blends[i];
                var blendOffsets = WriteAnimationFrames(bw, seq, blend, numBones);
                Array.Copy(blendOffsets, 0, offsets, blendLength * i, blendLength);
            }

            // go back and write the offsets
            var endPos = bw.BaseStream.Position;
            bw.BaseStream.Seek(pos, SeekOrigin.Begin);
            bw.WriteUShortArray(offsets.Length, offsets);
            bw.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }

        internal static ushort[] WriteAnimationFrames(BinaryWriter bw, Sequence seq, Blend blend, int numBones)
        {
            var ret = new ushort[6 * numBones];

            ushort currentOffset = 0;
            var idx = 0;

            for (var i = 0; i < numBones; i++)
            {
                currentOffset += sizeof(ushort) * 6;
                var b = i;
                ret[idx++] = WriteVals(x => x.Positions[b].X);
                ret[idx++] = WriteVals(x => x.Positions[b].Y);
                ret[idx++] = WriteVals(x => x.Positions[b].Z);
                ret[idx++] = WriteVals(x => x.Rotations[b].X);
                ret[idx++] = WriteVals(x => x.Rotations[b].Y);
                ret[idx++] = WriteVals(x => x.Rotations[b].Z);
            }

            return ret;

            ushort WriteVals(Func<AnimationFrame, float> map)
            {
                var offs = currentOffset;
                var vals = blend.Frames.Select(x => (short) map(x)).ToArray();
                var len = (ushort) WriteAnimationFrameValues(bw, vals);
                currentOffset += len;
                return len == 0 ? (ushort) 0 : offs;
            }
        }

        internal static long WriteAnimationFrameValues(BinaryWriter bw, short[] values)
        {
            if (values.Length == 0 || values.All(x => x == 0)) return 0; // no values, or every value is 0

            var startPos = bw.BaseStream.Position;
            foreach (var run in AnimationRle.Compress(values))
            {
                run.WriteTo(bw);
            }
            return bw.BaseStream.Position - startPos;
        }

        #endregion

        #region Writing models

        private int[] WriteBodyParts(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }

        private void WriteModels(BinaryWriter bw, BodyPart part)
        {

        }

        #endregion

        /// <summary>
        /// Get the transforms for the bones of this model
        /// </summary>
        /// <param name="sequence">The sequence id to use</param>
        /// <param name="frame">The frame number to use</param>
        /// <param name="subframe">The subframe between the given frame and the next frame, as a percentage between 0 and 1</param>
        /// <param name="transforms">The array of transforms to set values into. Must be at least the size of the <see cref="Bones"/> array.</param>
        public void GetTransforms(int sequence, int frame, float subframe, ref Matrix4x4[] transforms)
        {
            var seq = Sequences[sequence];
            var blend = seq.Blends[0];
            var cFrame = blend.Frames[frame % seq.NumFrames];
            var nFrame = blend.Frames[(frame + 1) % seq.NumFrames];

            var indivTransforms = new Matrix4x4[128];
            for (var i = 0; i < Bones.Count; i++)
            {
                var bone = Bones[i];
                var cPos = bone.Position + cFrame.Positions[i] * bone.PositionScale;
                var nPos = bone.Position + nFrame.Positions[i] * bone.PositionScale;
                var cRot = bone.Rotation + cFrame.Rotations[i] * bone.RotationScale;
                var nRot = bone.Rotation + nFrame.Rotations[i] * bone.RotationScale;

                var cQtn = Quaternion.CreateFromYawPitchRoll(cRot.X, cRot.Y, cRot.Z);
                var nQtn = Quaternion.CreateFromYawPitchRoll(nRot.X, nRot.Y, nRot.Z);

                // MDL angles have Y as the up direction
                cQtn = new Quaternion(cQtn.Y, cQtn.X, cQtn.Z, cQtn.W);
                nQtn = new Quaternion(nQtn.Y, nQtn.X, nQtn.Z, nQtn.W);

                var mat = Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(cQtn, nQtn, subframe));
                mat.Translation = cPos * (1 - subframe) + nPos * subframe;

                indivTransforms[i] = mat;
            }

            for (var i = 0; i < Bones.Count; i++)
            {
                var mat = indivTransforms[i];
                var parent = Bones[i].Parent;
                while (parent >= 0)
                {
                    var parMat = indivTransforms[parent];
                    mat = mat * parMat;
                    parent = Bones[parent].Parent;
                }
                transforms[i] = mat;
            }
        }

        /// <summary>
        /// Pre-calculate some bogus chrome values that look "ok" for a cheap effect.
        /// This method will modify the original vertices.
        /// </summary>
        public void WriteFakePrecalculatedChromeCoordinates()
        {
            var transforms = new Matrix4x4[Bones.Count];
            GetTransforms(0, 0, 0, ref transforms);
            for (var bp = 0; bp < BodyParts.Count; bp++)
            {
                var part = BodyParts[bp];
                for (var m = 0; m < part.Models.Length; m++)
                {
                    var model = part.Models[m];
                    for (var me = 0; me < model.Meshes.Length; me++)
                    {
                        var mesh = model.Meshes[me];
                        var skin = Textures[mesh.SkinRef];
                        if (!skin.Flags.HasFlag(TextureFlags.Chrome)) continue;

                        for (var vi = 0; vi < mesh.Vertices.Length; vi++)
                        {
                            var v = mesh.Vertices[vi];
                            var transform = transforms[v.VertexBone];

                            // Borrowed from HLMV's StudioModel::Chrome function
                            var tmp = Vector3.Normalize(transform.Translation);

                            // Using unitx for the "player right" vector
                            var up = Vector3.Normalize(Vector3.Cross(tmp, Vector3.UnitX));
                            var right = Vector3.Normalize(Vector3.Cross(tmp, up));

                            // HLMV is doing an inverse rotate (no translation),
                            // so we set the shift values to zero after inverting
                            var inv = Matrix4x4.Invert(transform, out var i) ? i : transform;
                            inv.Translation = Vector3.Zero;
                            up = Vector3.Transform(up, inv);
                            right = Vector3.Transform(right, inv);

                            BodyParts[bp].Models[m].Meshes[me].Vertices[vi].Texture = new Vector2(
                                (Vector3.Dot(v.Normal, right) + 1) * 32,
                                (Vector3.Dot(v.Normal, up) + 1) * 32
                            );
                        }
                    }
                }
            }
        }

        private enum Section : int
        {
            Bone,
            BoneController,
            Hitbox,
            Sequence,
            SequenceGroup,
            Texture,
            Skin,
            BodyPart,
            Attachment,
            Sound,      // Unused
            SoundGroup, // Unused
            Transition,
            NumSections = 12
        }
    }
}
