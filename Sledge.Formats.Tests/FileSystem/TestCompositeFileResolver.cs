using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.Tests.FileSystem;

[TestClass]
public class TestCompositeFileResolver
{
    private static readonly Dictionary<string, string> Files = new()
    {
        { "file1.txt", "11111" },
        { "file2.txt", "22222" },
        { "folder/file.txt", "22222" },
        { "folder1/file1.txt", "11111" },
        { "folder2/file2.txt", "22222" },
    };

    private static string _baseDir1;
    private static string _baseDir2;

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _baseDir1 = Path.Combine(Path.GetTempPath(), "sledge.formats.tests-filesystem-" + Guid.NewGuid());
        _baseDir2 = Path.Combine(Path.GetTempPath(), "sledge.formats.tests-filesystem-" + Guid.NewGuid());
        if (Directory.Exists(_baseDir1)) Directory.Delete(_baseDir1, true);
        if (Directory.Exists(_baseDir2)) Directory.Delete(_baseDir2, true);
        Directory.CreateDirectory(_baseDir1);
        Directory.CreateDirectory(_baseDir2);

        Directory.CreateDirectory(Path.Combine(_baseDir1, "folder")); // duplicate
        Directory.CreateDirectory(Path.Combine(_baseDir1, "folder1")); // unique

        Directory.CreateDirectory(Path.Combine(_baseDir2, "folder")); // duplicate
        Directory.CreateDirectory(Path.Combine(_baseDir2, "folder2")); // unique

        File.WriteAllText(Path.Combine(_baseDir1, "file1.txt"), "11111");
        File.WriteAllText(Path.Combine(_baseDir2, "file2.txt"), "22222");

        File.WriteAllText(Path.Combine(_baseDir1, "folder/file.txt"), "11111"); // duplicate
        File.WriteAllText(Path.Combine(_baseDir1, "folder1/file1.txt"), "11111"); // unique

        File.WriteAllText(Path.Combine(_baseDir2, "folder/file.txt"), "22222"); // duplicate
        File.WriteAllText(Path.Combine(_baseDir2, "folder2/file2.txt"), "22222"); // unique
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        if (Directory.Exists(_baseDir1)) Directory.Delete(_baseDir1, true);
        if (Directory.Exists(_baseDir2)) Directory.Delete(_baseDir2, true);
    }

    [TestMethod]
    public void TestFileExists()
    {
        var cfs = new CompositeFileResolver(new DiskFileResolver(_baseDir2), new DiskFileResolver(_baseDir1));
        foreach (var kv in Files)
        {
            Assert.IsTrue(cfs.FileExists(kv.Key));
        }
    }

    [TestMethod]
    public void TestOpenFile()
    {
        var cfs = new CompositeFileResolver(new DiskFileResolver(_baseDir2), new DiskFileResolver(_baseDir1));
        foreach (var kv in Files)
        {
            using var s = cfs.OpenFile(kv.Key);
            using var tr = new StreamReader(s, Encoding.ASCII);
            Assert.AreEqual(kv.Value, tr.ReadToEnd());
        }
    }

    [TestMethod]
    public void TestGetFiles()
    {
        var cfs = new CompositeFileResolver(new DiskFileResolver(_baseDir2), new DiskFileResolver(_baseDir1));
        var files = cfs.GetFiles("/").ToList();
        Assert.AreEqual(2, files.Count);
        CollectionAssert.AreEquivalent(new[] { "file1.txt", "file2.txt" }, files);
    }

    [TestMethod]
    public void TestGetFolders()
    {
        var cfs = new CompositeFileResolver(new DiskFileResolver(_baseDir2), new DiskFileResolver(_baseDir1));
        var folders = cfs.GetFolders("/").ToList();
        Assert.AreEqual(3, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "folder", "folder1", "folder2" }, folders);
    }

    [TestMethod]
    public void TestFileNotFound()
    {
        var cfs = new CompositeFileResolver(new DiskFileResolver(_baseDir2), new DiskFileResolver(_baseDir1));
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            cfs.OpenFile("not_found.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            cfs.OpenFile("not/found");
        });
    }
}