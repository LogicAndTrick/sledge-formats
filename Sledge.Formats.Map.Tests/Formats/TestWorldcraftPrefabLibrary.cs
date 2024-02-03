using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;

namespace Sledge.Formats.Map.Tests.Formats;

[TestClass]
public class TestWorldcraftPrefabLibrary
{
    [TestMethod]
    public void TestSimplePrefab()
    {
        using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.prefab.boxes.ol");
        var library = new WorldcraftPrefabLibrary(file);

        Assert.AreEqual(2, library.Prefabs.Count);
        Assert.AreEqual("some boxes", library.Description);

        Assert.AreEqual("box1", library.Prefabs[0].Name);
        Assert.AreEqual("box number one", library.Prefabs[0].Description);

        Assert.AreEqual("box2", library.Prefabs[1].Name);
        Assert.AreEqual("second box", library.Prefabs[1].Description);
    }

    [TestMethod]
    public void TestMultiVersionPrefab()
    {
        using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.prefab.multi_version.ol");
        var library = new WorldcraftPrefabLibrary(file);

        Assert.AreEqual(3, library.Prefabs.Count);
        Assert.AreEqual("1.5b", library.Prefabs[0].Name);
        Assert.AreEqual("2.1", library.Prefabs[1].Name);
        Assert.AreEqual("3.3", library.Prefabs[2].Name);
    }

    [TestMethod]
    public void TestWritingPrefab()
    {
        using var file = typeof(TestJackhammerFormat).Assembly.GetManifestResourceStream("Sledge.Formats.Map.Tests.Resources.prefab.boxes.ol");
        var library = new WorldcraftPrefabLibrary(file);

        var newLib = new WorldcraftPrefabLibrary
        {
            Description = "write test"
        };

        var testMap = new MapFile();
        testMap.Worldspawn.Children.Add(new Entity
        {
            ClassName = "this_is_a_test"
        });

        newLib.Prefabs.Add(new Prefab("test1", "testing", library.Prefabs[0].Map));
        newLib.Prefabs.Add(new Prefab("test2", "more testing", library.Prefabs[1].Map));
        newLib.Prefabs.Add(new Prefab("test3", "even more testing", library.Prefabs[1].Map));
        newLib.Prefabs.Add(new Prefab("test4", "final test", testMap));

        var ms = new MemoryStream();
        newLib.Write(ms);
        ms.Position = 0;

        var openLib = new WorldcraftPrefabLibrary(ms);

        Assert.AreEqual(4, openLib.Prefabs.Count);
        Assert.AreEqual("write test", openLib.Description);

        Assert.AreEqual("test1", openLib.Prefabs[0].Name);
        Assert.AreEqual("testing", openLib.Prefabs[0].Description);

        Assert.AreEqual("test2", openLib.Prefabs[1].Name);
        Assert.AreEqual("more testing", openLib.Prefabs[1].Description);

        Assert.AreEqual("test3", openLib.Prefabs[2].Name);
        Assert.AreEqual("even more testing", openLib.Prefabs[2].Description);

        Assert.AreEqual("test4", openLib.Prefabs[3].Name);
        Assert.AreEqual("final test", openLib.Prefabs[3].Description);
        Assert.AreEqual(1, openLib.Prefabs[3].Map.Worldspawn.Children.Count);
        Assert.AreEqual("this_is_a_test", openLib.Prefabs[3].Map.Worldspawn.Children.OfType<Entity>().First().ClassName);
    }
}