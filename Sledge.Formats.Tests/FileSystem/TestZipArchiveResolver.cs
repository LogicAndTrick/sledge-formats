using System;
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

    private const string Archive = "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";

    private static ZipArchive _instance;

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        var data = Convert.FromBase64String(Archive);
        var ms = new MemoryStream(data);
        ms.Position = 0;
        _instance = new ZipArchive(ms, ZipArchiveMode.Read, false);
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
        Assert.AreEqual(2, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "folder", "folder2" }, folders);
    }

    [TestMethod]
    public void TestFileNotFound()
    {
        var zfs = new ZipArchiveResolver(_instance);
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            zfs.OpenFile("not_found.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            zfs.OpenFile("not/found");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            zfs.GetFiles("not/found");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            zfs.GetFolders("not/found");
        });
    }
}