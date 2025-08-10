using Sledge.Formats.Model.Goldsource;
using Version = Sledge.Formats.Model.Goldsource.Version;

namespace Sledge.Formats.Model.Tests;

[TestClass]
public class TestGoldsource
{
    [TestMethod]
    public void TestMdl10Cube()
    {
        using var file = typeof(TestGoldsource).Assembly.GetManifestResourceStream("Sledge.Formats.Model.Tests.Resources.mdl10.cube.mdl");
        var mdl = new MdlFile(new[] { file });

        Assert.AreEqual(2, mdl.BodyParts.Count);
        Assert.AreEqual(3, mdl.Sequences.Count);
        Assert.AreEqual(3, mdl.Skins.Count);
    }

    [TestMethod]
    public void TestMdl10RoundTrip()
    {
        using var file = typeof(TestGoldsource).Assembly.GetManifestResourceStream("Sledge.Formats.Model.Tests.Resources.mdl10.cube.mdl");
        var mdl = new MdlFile(new[] { file });

        using var output = mdl.Write("cube", new MdlFileWriteOptions
        {
            SplitTextures = false,
        });

        Assert.AreEqual(1, output.Files.Count);

        var writeFile = output.Files[0];

        Assert.AreEqual(0, writeFile.FileNumber);
        Assert.AreEqual(MdlWriteOutputType.Base, writeFile.Type);
        Assert.AreEqual("", writeFile.Suffix);
    }

    [TestMethod]
    public void TestMdl10WriteAnimationFrames()
    {
        using var file = typeof(TestGoldsource).Assembly.GetManifestResourceStream("Sledge.Formats.Model.Tests.Resources.mdl10.cube.mdl");
        var mdl = new MdlFile(new[] { file });
        var numBones = mdl.Bones.Count;
        
        var seq = mdl.Sequences[0];
        var blend = seq.Blends[0];
        var expectedFrames = blend.Frames;

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var offsets = UnitTestingShim.WriteAnimationFrames(bw, seq, blend, numBones);

        ms.Seek(0, SeekOrigin.Begin);
        using var br = new BinaryReader(ms);
        var actualFrames = UnitTestingShim.ReadAnimationFrames(br, seq, numBones, -12, offsets);

        Assert.AreEqual(expectedFrames.Length, actualFrames.Length);
        for (var i = 0; i < expectedFrames.Length; i++)
        {
            var ef = expectedFrames[i];
            var af = actualFrames[i];
            Assert.AreEqual(numBones, af.Positions.Length);
            Assert.AreEqual(numBones, af.Rotations.Length);
            for (var j = 0; j < numBones; j++)
            {
                Assert.AreEqual(ef.Positions[j], af.Positions[j]);
                Assert.AreEqual(ef.Rotations[j], af.Rotations[j]);
            }
        }
    }

    [TestMethod]
    public void TestMdl10WriteAnimationFrameValues()
    {
        var values = new short[] { 1, 2, 3, 1, 1, 1, 1, 1, 8 };
        var runs = AnimationRle.Compress(values).ToList();

        Assert.AreEqual(2, runs.Count);
        Assert.AreEqual(4, runs[0].CompressedLength);
        Assert.AreEqual(8, runs[0].UncompressedLength);
        Assert.AreEqual(1, runs[0].Values[0]);
        Assert.AreEqual(2, runs[0].Values[1]);
        Assert.AreEqual(3, runs[0].Values[2]);
        Assert.AreEqual(1, runs[0].Values[3]);
        Assert.AreEqual(1, runs[1].CompressedLength);
        Assert.AreEqual(1, runs[1].UncompressedLength);
        Assert.AreEqual(8, runs[1].Values[0]);

        values = new short[] { 1, 2, 2, 3, 3, 3, 4, 4, 4, 4 };
        runs = AnimationRle.Compress(values).ToList();

        Assert.AreEqual(2, runs.Count);
        Assert.AreEqual(4, runs[0].CompressedLength);
        Assert.AreEqual(6, runs[0].UncompressedLength);
        Assert.AreEqual(1, runs[0].Values[0]);
        Assert.AreEqual(2, runs[0].Values[1]);
        Assert.AreEqual(2, runs[0].Values[2]);
        Assert.AreEqual(3, runs[0].Values[3]);
        Assert.AreEqual(1, runs[1].CompressedLength);
        Assert.AreEqual(4, runs[1].UncompressedLength);
        Assert.AreEqual(4, runs[1].Values[0]);

        values = Enumerable.Range(0, 300).Select(_ => (short)1).ToArray();
        runs = AnimationRle.Compress(values).ToList();

        Assert.AreEqual(2, runs.Count);
        Assert.AreEqual(1, runs[0].CompressedLength);
        Assert.AreEqual(255, runs[0].UncompressedLength);
        Assert.AreEqual(1, runs[0].Values[0]);
        Assert.AreEqual(1, runs[1].CompressedLength);
        Assert.AreEqual(45, runs[1].UncompressedLength);
        Assert.AreEqual(1, runs[1].Values[0]);
    }

    [TestMethod]
    [DataRow("cube.mdl")]
    [DataRow("cube-tex.mdl", "cube-texT.mdl")]
    public void TestMdl10Header(params string[] fileNames)
    {
        var a = typeof(TestGoldsource).Assembly;
        var files = fileNames.Select(x => a.GetManifestResourceStream($"Sledge.Formats.Model.Tests.Resources.mdl10.{x}")!).ToList();
        var mdl = new MdlFile(files);

        Assert.AreEqual(ID.Idst, mdl.Header.ID);
        Assert.AreEqual(Version.Goldsource, mdl.Header.Version);
        Assert.AreEqual(fileNames[0], mdl.Header.Name);

        files.ForEach(x => x.Dispose());
    }
}