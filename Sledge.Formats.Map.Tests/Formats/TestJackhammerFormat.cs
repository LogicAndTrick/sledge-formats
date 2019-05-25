using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;

namespace Sledge.Formats.Map.Tests.Formats
{
    [TestClass]
    public class TestJackhammerFormat
    {
        [TestMethod]
        public void TestJmfFormatLoading()
        {
            var format = new JackhammerJmfFormat();
            foreach (var file in Directory.GetFiles(@"D:\Downloads\formats\jmf", "1group.jmf"))
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