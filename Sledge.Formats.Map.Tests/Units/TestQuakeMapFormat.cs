using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;

namespace Sledge.Formats.Map.Tests.Units;

[TestClass]
public class TestQuakeMapFormat
{
    private const string QuakeFormatFile = @"// Game: Half-Life
// Format: Standard
// entity 0
{
""classname"" ""worldspawn""
// brush 0
{
( 0 0 0 ) ( 0 1 0 ) ( 0 0 1 ) AAATRIGGER 0 0 0 1 1
( 0 0 0 ) ( 0 0 1 ) ( 1 0 0 ) AAATRIGGER 0 0 0 1 1
( 0 0 0 ) ( 1 0 0 ) ( 0 1 0 ) AAATRIGGER 0 0 0 1 1
( 64 64 16 ) ( 64 65 16 ) ( 65 64 16 ) AAATRIGGER 0 0 0 1 1
( 64 64 128 ) ( 65 64 128 ) ( 64 64 129 ) AAATRIGGER 0 0 0 1 1
( 64 64 128 ) ( 64 64 129 ) ( 64 65 128 ) AAATRIGGER 0 0 0 1 1
}
}
// entity 1
{
""classname"" ""trigger_once""
// brush 0
{
( 80 0 0 ) ( 80 1 0 ) ( 80 0 1 ) AAATRIGGER 0 0 0 1 1
( 80 0 0 ) ( 80 0 1 ) ( 81 0 0 ) AAATRIGGER 0 0 0 1 1
( 80 0 0 ) ( 81 0 0 ) ( 80 1 0 ) AAATRIGGER 0 0 0 1 1
( 144 64 16 ) ( 144 65 16 ) ( 145 64 16 ) AAATRIGGER 0 0 0 1 1
( 144 64 128 ) ( 145 64 128 ) ( 144 64 129 ) AAATRIGGER 0 0 0 1 1
( 144 64 128 ) ( 144 64 129 ) ( 144 65 128 ) AAATRIGGER 0 0 0 1 1
}
}
// entity 2
{
""classname"" ""func_group""
""_tb_type"" ""_tb_layer""
""_tb_name"" ""Layer2""
""_tb_id"" ""2""
""_tb_layer_sort_index"" ""0""
// brush 0
{
( 0 80 0 ) ( 0 81 0 ) ( 0 80 1 ) AAATRIGGER 0 0 0 1 1
( -16 80 0 ) ( -16 80 1 ) ( -15 80 0 ) AAATRIGGER 0 0 0 1 1
( -16 80 0 ) ( -15 80 0 ) ( -16 81 0 ) AAATRIGGER 0 0 0 1 1
( 64 224 16 ) ( 64 225 16 ) ( 65 224 16 ) AAATRIGGER 0 0 0 1 1
( 64 144 16 ) ( 65 144 16 ) ( 64 144 17 ) AAATRIGGER 0 0 0 1 1
( 64 224 16 ) ( 64 224 17 ) ( 64 225 16 ) AAATRIGGER 0 0 0 1 1
}
}
";
    private const string ValveFormatFile = @"// Game: Half-Life
// Format: Valve
// entity 0
{
""classname"" ""worldspawn""
""sounds"" ""1""
""MaxRange"" ""4096""
""mapversion"" ""220""
""wad"" ""C:/Half-Life/valve/halflife.wad;C:/Half-Life/valve/liquids.wad;C:/Half-Life/valve/water.wad;C:/Half-Life/valve/xeno.wad;C:/zhlt.wad""
""_tb_def"" ""builtin:halflife.fgd""
// brush 0
{
( -64 256 64 ) ( -64 128 64 ) ( -64 128 -64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 64 128 -64 ) ( -64 128 -64 ) ( -64 128 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( -64 128 -64 ) ( 64 128 -64 ) ( 64 256 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( -64 256 64 ) ( 64 256 64 ) ( 64 128 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( 64 256 64 ) ( -64 256 64 ) ( -64 256 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 64 256 -64 ) ( 64 128 -64 ) ( 64 128 64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
}
}
// entity 1
{
""classname"" ""func_group""
""_tb_type"" ""_tb_layer""
""_tb_name"" ""Layer2""
""_tb_id"" ""4""
// brush 0
{
( -64 64 64 ) ( -64 -64 64 ) ( -64 -64 -64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 64 -64 -64 ) ( -64 -64 -64 ) ( -64 -64 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( -64 -64 -64 ) ( 64 -64 -64 ) ( 64 64 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( -64 64 64 ) ( 64 64 64 ) ( 64 -64 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( 64 64 64 ) ( -64 64 64 ) ( -64 64 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 64 64 -64 ) ( 64 -64 -64 ) ( 64 -64 64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
}
}
// entity 2
{
""classname"" ""func_group""
""_tb_type"" ""_tb_layer""
""_tb_name"" ""Layer1""
""_tb_id"" ""5""
}
// entity 3
{
""classname"" ""trigger_once""
""_tb_layer"" ""5""
// brush 0
{
( 128 64 64 ) ( 128 -64 64 ) ( 128 -64 -64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 256 -64 -64 ) ( 128 -64 -64 ) ( 128 -64 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 128 -64 -64 ) ( 256 -64 -64 ) ( 256 64 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( 128 64 64 ) ( 256 64 64 ) ( 256 -64 64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
( 256 64 64 ) ( 128 64 64 ) ( 128 64 -64 ) AAATRIGGER [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 256 64 -64 ) ( 256 -64 -64 ) ( 256 -64 64 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
}
}
";

    [DataTestMethod]
    [DataRow(typeof(QuakeMapFormat))]
    [DataRow(typeof(QuakeMapFormat2))]
    public void TestBasic(Type type)
    {
        var format = (IMapFormat) Activator.CreateInstance(type)!;
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(QuakeFormatFile));
        var map = format.Read(stream);

        Assert.AreEqual(3, map.Worldspawn.Children.Count);
    }

    [DataTestMethod]
    [DataRow(typeof(QuakeMapFormat))]
    [DataRow(typeof(QuakeMapFormat2))]
    public void TestValve(Type type)
    {
        var format = (IMapFormat) Activator.CreateInstance(type)!;
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(ValveFormatFile));
        var map = format.Read(stream);

        Assert.AreEqual(4, map.Worldspawn.Children.Count);
    }
}