using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sledge.Formats.FileSystem;

namespace Sledge.Formats.Tests.FileSystem;

[TestClass]
public class TestVirtualSubdirectoryFileResolver
{
    private Dictionary<string, string> Files => TestZipArchiveResolver.Files;

    private static ZipArchive _instance;

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _instance = TestZipArchiveResolver.CreateZipArchive();
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        _instance?.Dispose();
    }

    [TestMethod]
    public void TestFileExists()
    {
        var vsfr = new VirtualSubdirectoryFileResolver("something/", new ZipArchiveResolver(_instance));
        foreach (var kv in Files)
        {
            Assert.IsTrue(vsfr.FileExists("something/" + kv.Key));
        }
    }

    [TestMethod]
    public void TestOpenFile()
    {
        var vsfr = new VirtualSubdirectoryFileResolver("something/", new ZipArchiveResolver(_instance));
        foreach (var kv in Files)
        {
            using var s = vsfr.OpenFile("something/" + kv.Key);
            using var tr = new StreamReader(s, Encoding.ASCII);
            Assert.AreEqual(kv.Value, tr.ReadToEnd());
        }
    }

    [TestMethod]
    public void TestGetFiles()
    {
        var vsfr = new VirtualSubdirectoryFileResolver("something/", new ZipArchiveResolver(_instance));

        var files = vsfr.GetFiles("/").ToList();
        Assert.AreEqual(0, files.Count);
        CollectionAssert.AreEquivalent(Array.Empty<string>(), files);

        files = vsfr.GetFiles("something/").ToList();
        Assert.AreEqual(5, files.Count);
        CollectionAssert.AreEquivalent(new[] { "something/test1.txt", "something/test2.txt", "something/test3.txt", "something/test4.txt", "something/test5.txt" }, files);
    }

    [TestMethod]
    public void TestGetFolders()
    {
        var vsfr = new VirtualSubdirectoryFileResolver("something/", new ZipArchiveResolver(_instance));

        var folders = vsfr.GetFolders("/").ToList();
        Assert.AreEqual(1, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "something" }, folders);

        folders = vsfr.GetFolders("something/").ToList();
        Assert.AreEqual(3, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "something/folder", "something/folder2", "something/folder3" }, folders);

        folders = vsfr.GetFolders("something/folder3").ToList();
        Assert.AreEqual(1, folders.Count);
        CollectionAssert.AreEquivalent(new[] { "something/folder3/folder3.1" }, folders);
    }

    [TestMethod]
    public void TestFileNotFound()
    {
        var vsfr = new VirtualSubdirectoryFileResolver("something/", new ZipArchiveResolver(_instance));
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            vsfr.OpenFile("test1.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            vsfr.OpenFile("not_found.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            vsfr.OpenFile("something/not_found.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            vsfr.OpenFile("something/not/found");
        });
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            vsfr.GetFiles("something/not/found");
        });
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            vsfr.GetFolders("something/not/found");
        });
    }
}