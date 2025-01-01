using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class WorldcraftCommandSequenceFile
    {
        private const int NameStringLength = 128;
        private const int CommandStringLength = 260;
        private static readonly string SequenceFileHeader = "Worldcraft Command Sequences\r\n" + (char)0x1A;

        /// <summary>
        /// Version 0.1 used for Worldcraft 1.1-1.5b
        /// </summary>
        public const float MinVersion = 0.1f;

        /// <summary>
        /// Version 0.2 used for Worldcraft 1.6a and up
        /// </summary>
        public const float MaxVersion = 0.2f;

        public List<WorldcraftCommandSequence> CommandSequences { get; set; }

        public WorldcraftCommandSequenceFile()
        {
            CommandSequences = new List<WorldcraftCommandSequence>();
        }

        public WorldcraftCommandSequenceFile(Stream stream)
        {
            CommandSequences = new List<WorldcraftCommandSequence>();
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
                var header = br.ReadFixedLengthString(Encoding.ASCII, SequenceFileHeader.Length);
                if (header != SequenceFileHeader.TrimEnd('\0')) throw new NotSupportedException($"Incorrect command sequence file header. Expected '{SequenceFileHeader}', got '{header}'.");

                var version = br.ReadSingle();
                if (version < MinVersion || version > MaxVersion) throw new NotSupportedException($"Unsupported command sequence file version. Expected {MinVersion} or {MaxVersion}, got {version}.");

                var numSequences = br.ReadInt32();
                for (var i = 0; i < numSequences; i++)
                {
                    var seq = new WorldcraftCommandSequence
                    {
                        Name = br.ReadFixedLengthString(Encoding.ASCII, NameStringLength)
                    };

                    var numSteps = br.ReadInt32();
                    for (var j = 0; j < numSteps; j++)
                    {
                        var step = new WorldcraftCommandSequenceStep
                        {
                            IsEnabled = br.ReadInt32() > 0,
                            Type = (CommandType) br.ReadInt32(), // unknown 1
                            Command = br.ReadFixedLengthString(Encoding.ASCII, CommandStringLength),
                            Arguments = br.ReadFixedLengthString(Encoding.ASCII, CommandStringLength),
#pragma warning disable CS0612 // Type or member is obsolete
                            UseLongFileNames = br.ReadInt32() > 0,
#pragma warning restore CS0612
                            EnsureFileExists = br.ReadInt32() > 0,
                            FileExistsName = br.ReadFixedLengthString(Encoding.ASCII, CommandStringLength),
                            UseProcessWindow = br.ReadInt32() > 0
                        };
                        if (Math.Abs(version - 0.2f) < float.Epsilon)
                        {
                            // this looks like a bool, its only set to 1 for $game_exe commands in the default sequences file.
                            // there's no way to control it in the UI, so we'll ignore it.
                            _ = br.ReadInt32();
                        }
                        seq.Steps.Add(step);
                    }
                    CommandSequences.Add(seq);
                }
            }
        }

        public void Write(Stream stream, float version = MaxVersion)
        {
            if (version < MinVersion || version > MaxVersion) throw new NotSupportedException($"Unsupported command sequence file version. Expected {MinVersion} or {MaxVersion}, got {version}.");

            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.WriteFixedLengthString(Encoding.ASCII, SequenceFileHeader.Length, SequenceFileHeader);
                bw.Write(version);
                bw.Write(CommandSequences.Count);
                foreach (var config in CommandSequences)
                {
                    // Write sequence name, number of steps, and then each step:
                    bw.WriteFixedLengthString(Encoding.ASCII, NameStringLength, config.Name);
                    bw.Write(config.Steps.Count);
                    foreach (var step in config.Steps)
                    {
                        bw.Write(step.IsEnabled ? 1 : 0);
                        bw.Write(0); // unknown 1
                        bw.WriteFixedLengthString(Encoding.ASCII, CommandStringLength, step.Command);
                        bw.WriteFixedLengthString(Encoding.ASCII, CommandStringLength, step.Arguments);
#pragma warning disable CS0612 // Type or member is obsolete
                        bw.Write(step.UseLongFileNames ? 1 : 0);
#pragma warning restore CS0612
                        bw.Write(step.EnsureFileExists ? 1 : 0);
                        bw.WriteFixedLengthString(Encoding.ASCII, CommandStringLength, step.FileExistsName);
                        bw.Write(step.UseProcessWindow ? 1 : 0);
                        if (Math.Abs(version - 0.2f) < float.Epsilon)
                        {
                            bw.Write(0); // unknown 2
                        }
                    }
                }
            }
        }
    }
}
