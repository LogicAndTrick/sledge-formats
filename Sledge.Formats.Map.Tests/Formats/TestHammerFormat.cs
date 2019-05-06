using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;

namespace Sledge.Formats.Map.Tests.Formats
{
    [TestClass]
    public class TestHammerFormat
    {
        [TestMethod]
        public void TestVmfFormatLoading()
        {
            var format = new HammerVmfFormat();
            foreach (var file in Directory.GetFiles(@"D:\Downloads\formats\vmf"))
            {
                using (var r = File.OpenRead(file))
                {
                    try
                    {
                        format.Read(r);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Unable to read file: {Path.GetFileName(file)}. {ex.Message}");
                    }
                }
            }
        }
    }
}