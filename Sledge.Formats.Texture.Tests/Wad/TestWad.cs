using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Texture.Wad;
using Sledge.Formats.Valve;

namespace Sledge.Formats.Texture.Tests.Wad
{
    [TestClass]
    public class TestWad
    {
        [TestMethod]
        public void TestLoadWad2()
        {
            using (var f = File.OpenRead(@"D:\Downloads\formats\qwad\gfx.wad"))
            {
                var wad = new WadFile(f);
            }
        }

        [TestMethod]
        public void TestLoadWad3()
        {
            using (var f = File.OpenRead(@"D:\Downloads\formats\wad\halflife.wad"))
            {
                var wad = new WadFile(f);
            }
        }
    }
}