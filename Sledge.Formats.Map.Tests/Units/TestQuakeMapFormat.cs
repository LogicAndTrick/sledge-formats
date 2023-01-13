using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
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
( 0 0 0 ) ( 0 1 0 ) ( 0 0 1 )  0 0 0 1 1 // blank texture name (apparently valid...)
( 0 0 0 ) ( 0 0 1 ) ( 1 0 0 ) {BLUE 0 0 0 1 1 // texture name starting with a token
( 0 0 0 ) ( 1 0 0 ) ( 0 1 0 ) +012TEST 0 0 0 1 1 // texture name starting with +
( 64 64 16 ) ( 64 65 16 ) ( 65 64 16 ) 12345 0 0 0 1 1 // texture name is a number
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
( -64 256 64 ) ( -64 128 64 ) ( -64 128 -64.000 )  [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
( 64 128 -64 ) ( -64 128 -64 ) ( -64 128 64 ) {BLUE [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1
( -64 128 -64 ) ( 64 128 -64 ) ( 64 256 -64 ) +012TEST [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1
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
( -64 64 64 ) ( -64 -64 64 ) ( -64 -64 -64.0e0 ) AAATRIGGER [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1
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
    private const string LotsOfWhitespace = @"
// quark format
// Entity 9
// Entities:g[1] -> trigger_once:b[9]
{
 ""classname"" ""trigger_once""
 ""target"" ""mm1""
// Brush 0
// Entities:g[1] -> trigger_once:b[9] -> poly:p[1]
 {
  ( 896 -16 32 ) ( 1024 -16 32 ) ( 896 112 32 ) AAATRIGGER [ 1.00000 0.00000 0.00000 -896.00000 ]  [ 0.00000 -1.00000 0.00000 -16.00000 ]  0  1.00000 1.00000
  ( 896 -16 160 ) ( 896 112 160 ) ( 1024 -16 160 ) AAATRIGGER [ 1.00000 0.00000 0.00000 -896.00000 ]  [ 0.00000 -1.00000 0.00000 -16.00000 ]  0  1.00000 1.00000
  ( 896 -16 32 ) ( 896 -16 160 ) ( 1024 -16 32 ) AAATRIGGER [ 1.00000 0.00000 0.00000 -896.00000 ]  [ 0.00000 0.00000 -1.00000 32.00000 ]  0  1.00000 1.00000
  ( 896 112 32 ) ( 1024 112 32 ) ( 896 112 160 ) AAATRIGGER [ 1.00000 0.00000 0.00000 -896.00000 ]  [ 0.00000 0.00000 -1.00000 32.00000 ]  0  1.00000 1.00000
  ( 896 -16 32 ) ( 896 112 32 ) ( 896 -16 160 ) AAATRIGGER [ 0.00000 1.00000 0.00000 16.00000 ]  [ 0.00000 0.00000 -1.00000 32.00000 ]  0  1.00000 1.00000
  ( 1024 -16 32 ) ( 1024 -16 160 ) ( 1024 112 32 ) AAATRIGGER [ 0.00000 1.00000 0.00000 16.00000 ]  [ 0.00000 0.00000 -1.00000 32.00000 ]  0  1.00000 1.00000
 }
}
// more whitespace, quake format
// Entity 9
// Entities:g[1] -> trigger_once:b[9]
 { 
  ""classname""  ""trigger_once""
 { 
  ( 896 -16 32 ) ( 1024 -16 32 ) ( 896 112 32 ) AAATRIGGER 0  0 0  1.00000 1.00000 
  ( 896 -16 160 ) ( 896 112 160 ) ( 1024 -16 160 ) AAATRIGGER 0  0  0  1.00000 1.00000  
  ( 896 -16 32 ) ( 896 -16 160 ) ( 1024 -16 32 ) AAATRIGGER 0 0  0  1.00000  1.00000
  ( 896 112 32 ) ( 1024 112 32 ) ( 896 112 160 ) AAATRIGGER 0 0 0    1.00000 1.00000
  ( 896 -16 32 ) ( 896 112 32 ) ( 896 -16 160 ) AAATRIGGER 0 0 0    1.00000 1.00000
  ( 1024 -16 32 ) ( 1024 -16 160 ) ( 1024 112 32 ) AAATRIGGER 0 0 0  1.00000 1.00000
 } 
 } ";

    [TestMethod]
    public void TestBasic()
    {
        var format = new QuakeMapFormat();
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(QuakeFormatFile));
        var map = format.Read(stream);

        Assert.AreEqual(3, map.Worldspawn.Children.Count);
    }

    [TestMethod]
    public void TestValve()
    {
        var format = new QuakeMapFormat();
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(ValveFormatFile));
        var map = format.Read(stream);

        Assert.AreEqual(4, map.Worldspawn.Children.Count);
    }

    [TestMethod]
    public void TestWhitespace()
    {
        var format = new QuakeMapFormat();
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(LotsOfWhitespace));
        var map = format.Read(stream);

        Assert.AreEqual(2, map.Worldspawn.Children.Count);
    }

    [DataTestMethod]
    [DataRow("en")]
    [DataRow("es")]
    [DataRow("hi")]
    public void TestLocales(string culture)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);

        var format = new QuakeMapFormat();
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(ValveFormatFile));
        var map = format.Read(stream);

        Assert.AreEqual(4, map.Worldspawn.Children.Count);
    }
}