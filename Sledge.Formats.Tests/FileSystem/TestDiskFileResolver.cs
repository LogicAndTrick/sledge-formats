using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.Tests.FileSystem;

[TestClass]
public class TestDiskFileResolver
{
    private static readonly Dictionary<string, string> Files = new()
    {
        { "test1.txt", "1" },
        { "test2.txt", "22" },
        { "test3.txt", "333" },
        { "test4.txt", "4444" },
        { "test5.txt", "55555" },
        { "folder/data1.log", "data goes here" },
        { "folder/data2.log", "data goes here" },
        { "folder/data3.log", "data goes here" },
        { "folder2/data1.log", "data goes here" },
        { "folder2/data2.log", "data goes here" },
        { "folder2/data3.log", "data goes here" },
    };

    private static string _baseDir;

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _baseDir = Path.Combine(Path.GetTempPath(), "sledge.formats.tests-filesystem-" + Guid.NewGuid());
        if (Directory.Exists(_baseDir)) Directory.Delete(_baseDir, true);
        Directory.CreateDirectory(_baseDir);
        foreach (var kv in Files)
        {
            var path = Path.Combine(_baseDir, kv.Key);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
            File.WriteAllText(path, kv.Value, Encoding.ASCII);
        }
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        if (Directory.Exists(_baseDir)) Directory.Delete(_baseDir, true);
    }

    [TestMethod]
    public void TestFileExists()
    {
        var dfs = new DiskFileResolver(_baseDir);
        foreach (var kv in Files)
        {
            Assert.IsTrue(dfs.FileExists(kv.Key));
        }
    }

    [TestMethod]
    public void TestOpenFile()
    {
        var dfs = new DiskFileResolver(_baseDir);
        foreach (var kv in Files)
        {
            using var s = dfs.OpenFile(kv.Key);
            using var tr = new StreamReader(s, Encoding.ASCII);
            Assert.AreEqual(kv.Value, tr.ReadToEnd());
        }
    }

    [TestMethod]
    public void TestGetFiles()
    {
        var dfs = new DiskFileResolver(_baseDir);
        var files = dfs.GetFiles("/").ToList();
        Assert.AreEqual(5, files.Count);
        CollectionAssert.AreEquivalent(new[] { "test1.txt", "test2.txt", "test3.txt", "test4.txt", "test5.txt" }, files);
    }

    [TestMethod]
    public void TestGetFolders()
    {
        var dfs = new DiskFileResolver(_baseDir);
        var folders = dfs.GetFolders("/").ToList();
        Assert.AreEqual(2, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "folder", "folder2" }, folders);
    }

    [TestMethod]
    public void TestFileNotFound()
    {
        var dfs = new DiskFileResolver(_baseDir);
        Assert.ThrowsExactly<FileNotFoundException>(() =>
        {
            dfs.OpenFile("not_found.txt");
        });
        Assert.ThrowsExactly<FileNotFoundException>(() =>
        {
            dfs.OpenFile("not/found");
        });
        Assert.ThrowsExactly<DirectoryNotFoundException>(() =>
        {
            dfs.GetFiles("not/found");
        });
        Assert.ThrowsExactly<DirectoryNotFoundException>(() =>
        {
            dfs.GetFolders("not/found");
        });
    }
}