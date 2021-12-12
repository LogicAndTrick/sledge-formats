using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sledge.Formats.GameData.Tests.Fgd
{
    [TestClass]
    public class TestFgdFiles
    {
        private static List<(string name, Stream stream)> GetFiles(string path)
        {
            var assem = Assembly.GetExecutingAssembly();
            var files = new List<(string name, Stream stream)>();

            path = assem.GetName().Name + ".Resources." + path.Replace('/', '.');

            foreach (var name in assem.GetManifestResourceNames().Where(x => x.StartsWith(path)))
            {
                files.Add((name.Substring(path.Length + 1), assem.GetManifestResourceStream(name)));
            }

            return files;
        }

        [TestMethod]
        public void TestGoldsource()
        {
            var format = new FgdFormatter();
            foreach (var (name, stream) in GetFiles("fgd/goldsource"))
            {
                try
                {
                    var def = format.Read(new StreamReader(stream));
                    Console.WriteLine($"Successfully parsed {name}.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing {name}", ex);
                }
            }
        }

        [TestMethod]
        public void TestJack()
        {
            var format = new FgdFormatter();
            foreach (var (name, stream) in GetFiles("fgd/jack"))
            {
                try
                {
                    var def = format.Read(new StreamReader(stream));
                    Console.WriteLine($"Successfully parsed {name}.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing {name}", ex);
                }
            }
        }

        [TestMethod]
        public void TestTrenchbroom()
        {
            var format = new FgdFormatter() { AllowNewlinesInStrings = true };
            foreach (var (name, stream) in GetFiles("fgd/trenchbroom"))
            {
                try
                {
                    var def = format.Read(new StreamReader(stream));
                    Console.WriteLine($"Successfully parsed {name}.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing {name}", ex);
                }
            }
        }

        [TestMethod]
        public void TestSource()
        {
            var format = new FgdFormatter();
            foreach (var (name, stream) in GetFiles("fgd/source"))
            {
                try
                {
                    var def = format.Read(new StreamReader(stream));
                    Console.WriteLine($"Successfully parsed {name}.");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error parsing {name}", ex);
                }
            }
        }
    }
}
