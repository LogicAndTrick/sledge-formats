using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;

namespace Sledge.Formats.Map.Tests.Formats
{
    [TestClass]
    public class TestWorldcraftPrefabLibrary
    {
        [TestMethod]
        public void TestPrefabLoading()
        {
            var file = @"D:\Downloads\worldcraft\Worldcraft 3.3\prefabs\computers.ol";
            var lib = WorldcraftPrefabLibrary.FromFile(file);
            Assert.AreEqual(14, lib.Prefabs.Count);
        }
    }
}
