using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;

namespace Sledge.Formats.Map.Tests.Formats;

[TestClass]
public class TestQuakeFormat
{
    [TestMethod]
    public void TestMapFormatLoading()
    {
        var format = new QuakeMapFormat();
        foreach (var file in Directory.GetFiles(@"D:\Downloads\formats\map").OrderBy(x => Path.GetFileName(x).ToLower()))
        {
            using var r = File.OpenRead(file);
            try
            {
                format.Read(r);
                Console.WriteLine($"Successfully parsed {Path.GetFileName(file)}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to read file: {Path.GetFileName(file)}. {ex.Message}");
                throw;
            }
        }
    }
}