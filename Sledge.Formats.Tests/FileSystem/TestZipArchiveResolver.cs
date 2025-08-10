using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.Tests.FileSystem;

[TestClass]
public class TestZipArchiveResolver
{
    internal static readonly Dictionary<string, string> Files = new()
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
        { "folder3/folder3.1/data3.log", "data goes here" },
    };

    private static ZipArchive _instance;

    internal static ZipArchive CreateZipArchive()
    {
        var ms = new MemoryStream();
        var zip = new ZipArchive(ms, ZipArchiveMode.Update, false);
        foreach (var (k, v) in Files)
        {
            var e = zip.CreateEntry(k);
            using var s = e.Open();
            var bytes = Encoding.ASCII.GetBytes(v);
            s.Write(bytes, 0, bytes.Length);
        }
        return zip;
    }

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _instance = CreateZipArchive();
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        _instance?.Dispose();
    }

    [TestMethod]
    public void TestFileExists()
    {
        var zfs = new ZipArchiveResolver(_instance);
        foreach (var kv in Files)
        {
            Assert.IsTrue(zfs.FileExists(kv.Key));
        }
    }

    [TestMethod]
    public void TestOpenFile()
    {
        var zfs = new ZipArchiveResolver(_instance);
        foreach (var kv in Files)
        {
            using var s = zfs.OpenFile(kv.Key);
            using var tr = new StreamReader(s, Encoding.ASCII);
            Assert.AreEqual(kv.Value, tr.ReadToEnd());
        }
    }

    [TestMethod]
    public void TestGetFiles()
    {
        var zfs = new ZipArchiveResolver(_instance);
        var files = zfs.GetFiles("/").ToList();
        Assert.AreEqual(5, files.Count);
        CollectionAssert.AreEquivalent(new[] { "test1.txt", "test2.txt", "test3.txt", "test4.txt", "test5.txt" }, files);
    }

    [TestMethod]
    public void TestGetFolders()
    {
        var zfs = new ZipArchiveResolver(_instance);

        var folders = zfs.GetFolders("/").ToList();
        Assert.AreEqual(3, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "folder", "folder2", "folder3" }, folders);

        var folders2 = zfs.GetFolders("folder3").ToList();
        Assert.AreEqual(1, folders2.Count);
        CollectionAssert.AreEquivalent(new[] { "folder3/folder3.1" }, folders2);
    }

    [TestMethod]
    public void TestFileNotFound()
    {
        var zfs = new ZipArchiveResolver(_instance);
        Assert.ThrowsExactly<FileNotFoundException>(() =>
        {
            zfs.OpenFile("not_found.txt");
        });
        Assert.ThrowsExactly<FileNotFoundException>(() =>
        {
            zfs.OpenFile("not/found");
        });
        Assert.ThrowsExactly<DirectoryNotFoundException>(() =>
        {
            zfs.GetFiles("not/found");
        });
        Assert.ThrowsExactly<DirectoryNotFoundException>(() =>
        {
            zfs.GetFolders("not/found");
        });
    }
}