using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Texture.Vtf;

namespace Sledge.Formats.Texture.Tests.Vtf
{
    [TestClass]
    public class TestVtf
    {
        [TestMethod]
        public void TestLoadVtf()
        {
            using (var f = File.OpenRead(@"D:\sandbox\16F.vtf"))
            {
                var vtf = new VtfFile(f);
            }
        }
    }
}