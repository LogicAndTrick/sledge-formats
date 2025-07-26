using System.IO;

namespace Sledge.Formats.Model.Goldsource
{
    public static class UnitTestingShim
    {
        public static AnimationFrame[] ReadAnimationFrames(BinaryReader br, Sequence sequence, int numBones, long startPosition, ushort[] boneOffsets)
            => MdlFile.ReadAnimationFrames(br, sequence, numBones, startPosition, boneOffsets);

        public static ushort[] WriteAnimationFrames(BinaryWriter bw, Sequence seq, Blend blend, int numBones)
            => MdlFile.WriteAnimationFrames(bw, seq, blend, numBones);

        public static void WriteAnimationFrameValues(BinaryWriter bw, short[] values)
            => MdlFile.WriteAnimationFrameValues(bw, values);
    }
}
