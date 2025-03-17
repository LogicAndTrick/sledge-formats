using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftGameConfigurationFile
    {
        private const int StringLength = 128;
        private static readonly string ConfigFileHeader = "Game Configurations File\r\n" + (char)0x1A + '\0';

        /// <summary>
        /// Version 1.3 used for Worldcraft 1.6a-2.1
        /// </summary>
        public const float MinVersion = 1.3f;

        /// <summary>
        /// Version 1.4 used for Worldcraft 3.3 and VHE 3.5
        /// </summary>
        public const float MaxVersion = 1.4f;

        public List<WorldcraftGameConfiguration> Configurations { get; set; }

        public WorldcraftGameConfigurationFile()
        {
            Configurations = new List<WorldcraftGameConfiguration>();
        }

        public WorldcraftGameConfigurationFile(Stream stream)
        {
            Configurations = new List<WorldcraftGameConfiguration>();
            ReadFromStream(stream);
        }

        public static WorldcraftGameConfigurationFile FromFile(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return new WorldcraftGameConfigurationFile(stream);
            }
        }

        private void ReadFromStream(Stream stream)
        {
            using (var br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var header = br.ReadFixedLengthString(Encoding.ASCII, ConfigFileHeader.Length);
                if (header != ConfigFileHeader.TrimEnd('\0')) throw new NotSupportedException($"Incorrect configuration file header. Expected '{ConfigFileHeader}', got '{header}'.");

                var version = br.ReadSingle();
                if (version < MinVersion || version > MaxVersion) throw new NotSupportedException($"Unsupported configuration file version. Expected {MinVersion} or {MaxVersion}, got {version}.");

                var numGames = br.ReadInt32();
                for (var i = 0; i < numGames; i++)
                {
                    var config = new WorldcraftGameConfiguration
                    {
                        Name = br.ReadFixedLengthString(Encoding.ASCII, StringLength)
                    };

                    var numFgds = br.ReadInt32();
                    config.TextureFormat = (TextureFormat) br.ReadInt32();
                    config.MapType = (MapType) br.ReadInt32();

                    if (Math.Abs(version - 1.3f) < float.Epsilon)
                    {
#pragma warning disable CS0612 // Type or member is obsolete
                        config.PaletteFile = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
#pragma warning restore CS0612
                    }

                    config.BuildPrograms.GameExecutable = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.DefaultSolidEntityClass = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.DefaultPointEntityClass = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.BuildPrograms.BspExecutable = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.BuildPrograms.RadExecutable = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.BuildPrograms.VisExecutable = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.GameExecutableDirectory = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.RmfDirectory = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    config.BuildPrograms.BspDirectory = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    if (Math.Abs(version - 1.4f) < float.Epsilon)
                    {
                        config.BuildPrograms.CsgExecutable = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                        config.ModDirectory = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                        config.GameDirectory = br.ReadFixedLengthString(Encoding.ASCII, StringLength);
                    }
                    for (var j = 0; j < numFgds; j++)
                    {
                        config.GameDataFiles.Add(br.ReadFixedLengthString(Encoding.ASCII, StringLength));
                    }
                    Configurations.Add(config);
                }
            }
        }

        public void Write(Stream stream, float version = MaxVersion)
        {
            if (version < MinVersion || version > MaxVersion) throw new NotSupportedException($"Unsupported configuration file version. Expected {MinVersion} or {MaxVersion}, got {version}.");

            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.WriteFixedLengthString(Encoding.ASCII, ConfigFileHeader.Length, ConfigFileHeader);
                bw.Write(version);
                bw.Write(Configurations.Count);
                foreach (var config in Configurations)
                {
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.Name);
                    bw.Write(config.GameDataFiles.Count);
                    bw.Write((int)config.TextureFormat);
                    bw.Write((int)config.MapType);
                    if (Math.Abs(version - 1.3f) < float.Epsilon)
                    {
#pragma warning disable CS0612 // Type or member is obsolete
                        bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.PaletteFile);
#pragma warning restore CS0612
                    }
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.GameExecutable);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.DefaultSolidEntityClass);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.DefaultPointEntityClass);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.BspExecutable);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.RadExecutable);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.VisExecutable);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.GameExecutableDirectory);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.RmfDirectory);
                    bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.BspDirectory);
                    if (Math.Abs(version - 1.4f) < float.Epsilon)
                    {
                        bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.BuildPrograms.CsgExecutable);
                        bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.ModDirectory);
                        bw.WriteFixedLengthString(Encoding.ASCII, StringLength, config.GameDirectory);
                    }
                    foreach (var fgd in config.GameDataFiles)
                    {
                        bw.WriteFixedLengthString(Encoding.ASCII, StringLength, fgd);
                    }
                }
            }
        }
    }
}
