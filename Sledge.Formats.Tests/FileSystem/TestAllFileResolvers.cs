using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Sledge.Formats.FileSystem;
using System.IO;
using Sledge.Formats.Packages;
using Sledge.Formats.Packages.FileSystem;

namespace Sledge.Formats.Tests.FileSystem;

[TestClass]
public class TestAllFileResolvers
{
    private static readonly List<string> Folders =
    [
        "",
        "folder",
        "folder2",
        "folder3",
        "folder4",
        "folder5",
        "folder5/folder5.1",
    ];

    private static readonly List<string> Files =
    [
        "test1.txt",
        "test2.txt",
        "folder/data1.log",
        "folder/data2.log",
        "folder2/data1.log",
        "folder2/data2.log",
        "folder3/f3.txt",
        "folder4/f4.txt",
        "folder5/folder5.1/aaa.txt"
    ];

    private static Dictionary<string, IFileResolver> _resolvers;

    private static readonly List<Action> CleanupActions = [];

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        _resolvers = new Dictionary<string, IFileResolver>
        {
            { "Disk", CreateDiskFileResolver() },
            { "ZipArchive", CreateZipArchiveResolver() },
            { "VirtualSubdirectory", CreateVirtualSubdirectoryFileResolver() },
            { "Composite", CreateCompositeFileResolver() },
            { "Package", CreatePackageResolver() }
        };
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        CleanupActions.ForEach(x =>
        {
            try
            {
                x.Invoke();
            }
            catch
            {
                //
            }
        });
        CleanupActions.Clear();
    }

    private static IFileResolver CreateDiskFileResolver(string subdirectory = "")
    {
        var tempFolder = Path.GetTempPath();
        var baseDir = Path.Combine(tempFolder, "sledge.formats.tests-filesystem-" + Guid.NewGuid());
        if (subdirectory is { Length: > 0 }) baseDir = Path.Combine(baseDir, subdirectory);
        if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        Directory.CreateDirectory(baseDir);
        foreach (var filepath in Files)
        {
            var path = Path.Combine(baseDir, filepath);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
            File.WriteAllText(path, filepath, Encoding.ASCII);
        }
        CleanupActions.Add(() =>
        {
            Directory.Delete(tempFolder);
        });
        return new DiskFileResolver(baseDir);
    }

    private static IFileResolver CreateVirtualSubdirectoryFileResolver() => CreateDiskFileResolver("subdir1/subdir2");

    private static IFileResolver CreateZipArchiveResolver()
    {
        var ms = new MemoryStream();
        var zip = new ZipArchive(ms, ZipArchiveMode.Update, true, Encoding.ASCII);
        foreach (var filepath in Files)
        {
            var folder = Folders.Where(x => filepath.StartsWith(x + "/")).MaxBy(x => x.Length);
            if (!string.IsNullOrEmpty(folder) && zip.GetEntry(folder + "/") == null) zip.CreateEntry(folder + "/");
            var entry = zip.CreateEntry(filepath, CompressionLevel.Fastest);
            using var s = entry.Open();
            var bytes = Encoding.ASCII.GetBytes(filepath);
            s.Write(bytes, 0, bytes.Length);
        }

        // need to close and re-create the zip archive as the .Length property requires a read-only archive
        zip.Dispose();
        ms.Seek(0, SeekOrigin.Begin);
        zip = new ZipArchive(ms, ZipArchiveMode.Read, true, Encoding.ASCII);

        var resolver = new ZipArchiveResolver(zip, true);
        CleanupActions.Add(() =>
        {
            resolver.Dispose();
            zip.Dispose();
            ms.Dispose();
        });
        return resolver;
    }

    private static IFileResolver CreatePackageResolver()
    {
        // since this library can't create pak files (yet???), I made this in PakScape manually
        const string pakFile = "UEFDS5UAAABAAgAAZm9sZGVyMy9mMy50eHRmb2xkZXIyL2RhdGEyLmxvZ2ZvbGRlcjIvZGF0YTEubG9nZm9sZGVyL2RhdGEyLmxvZ2ZvbGRlci9kYXRhMS5sb2d0ZXN0Mi50eHR0ZXN0MS50eHRmb2xkZXI1L2ZvbGRlcjUuMS9hYWEudHh0Zm9sZGVyNC9mNC50eHRmb2xkZXIzL2YzLnR4dAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAwAAAAOAAAAZm9sZGVyMi9kYXRhMi5sb2cAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAaAAAAEQAAAGZvbGRlcjIvZGF0YTEubG9nAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKwAAABEAAABmb2xkZXIvZGF0YTIubG9nAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADwAAAAQAAAAZm9sZGVyL2RhdGExLmxvZwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABMAAAAEAAAAHRlc3QyLnR4dAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAXAAAAAkAAAB0ZXN0MS50eHQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGUAAAAJAAAAZm9sZGVyNS9mb2xkZXI1LjEvYWFhLnR4dAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABuAAAAGQAAAGZvbGRlcjQvZjQudHh0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAhwAAAA4AAAA=";
        var ms = new MemoryStream(Convert.FromBase64String(pakFile));
        var package = new PakPackage(ms);
        var resolver = new PackageFileResolver(package, true);
        CleanupActions.Add(() =>
        {
            resolver.Dispose();
            package.Dispose();
            ms.Dispose();
        });
        return resolver;
    }

    private static IFileResolver CreateCompositeFileResolver()
    {
        var ms = new MemoryStream();
        var zip = new ZipArchive(ms, ZipArchiveMode.Update, true, Encoding.ASCII);

        var tempFolder = Path.GetTempPath();
        var baseDir = Path.Combine(tempFolder, "sledge.formats.tests-filesystem-" + Guid.NewGuid());
        if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        Directory.CreateDirectory(baseDir);

        void AddZip(string filepath)
        {
            var folder = Folders.Where(x => filepath.StartsWith(x + "/")).MaxBy(x => x.Length);
            if (!string.IsNullOrEmpty(folder) && zip.GetEntry(folder + "/") == null) zip.CreateEntry(folder + "/");
            var entry = zip.CreateEntry(filepath, CompressionLevel.Fastest);
            using var s = entry.Open();
            var bytes = Encoding.ASCII.GetBytes(filepath);
            s.Write(bytes, 0, bytes.Length);
        }

        void AddDisk(string filepath)
        {
            var path = Path.Combine(baseDir, filepath);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
            File.WriteAllText(path, filepath, Encoding.ASCII);
        }

        var idx = 0;
        var rotation = new[] { AddZip, AddDisk };
        foreach (var filepath in Files)
        {
            rotation[idx].Invoke(filepath);
            idx = (idx + 1) % rotation.Length;
        }

        // need to close and re-create the zip archive as the .Length property requires a read-only archive
        zip.Dispose();
        ms.Seek(0, SeekOrigin.Begin);
        zip = new ZipArchive(ms, ZipArchiveMode.Read, true, Encoding.ASCII);

        var diskResolver = new DiskFileResolver(baseDir);
        var zipResolver = new ZipArchiveResolver(zip, true);

        CleanupActions.Add(() =>
        {
            Directory.Delete(tempFolder);
            zipResolver.Dispose();
            zip.Dispose();
            ms.Dispose();
        });
        return new CompositeFileResolver(
            diskResolver,
            zipResolver
        );
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestRootDirectory(string implementationName)
    {
        // test that "" and "/" both take us to the root folder
        var resolver = _resolvers[implementationName];
        CollectionAssert.AreEquivalent(Files.Where(x => !x.Contains('/')).ToList(), resolver.GetFiles("").ToList());
        CollectionAssert.AreEquivalent(Files.Where(x => !x.Contains('/')).ToList(), resolver.GetFiles("/").ToList());
        CollectionAssert.AreEquivalent(Folders.Where(x => !x.Contains('/') && x != "").ToList(), resolver.GetFolders("").ToList());
        CollectionAssert.AreEquivalent(Folders.Where(x => !x.Contains('/') && x != "").ToList(), resolver.GetFolders("/").ToList());
        CollectionAssert.AreEquivalent(resolver.GetFiles("/").ToList(), resolver.GetFiles("").ToList());
        CollectionAssert.AreEquivalent(resolver.GetFolders("/").ToList(), resolver.GetFolders("").ToList());
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestFolderExists(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        Assert.IsTrue(resolver.FolderExists(""));
        Assert.IsTrue(resolver.FolderExists("/"));
        Assert.IsFalse(resolver.FolderExists("aaaaaa"));
        Assert.IsFalse(resolver.FolderExists("aaaaaa/bbbbb"));
        foreach (var folder in Folders)
        {
            Assert.IsTrue(resolver.FolderExists(folder));
        }
        foreach (var file in Files)
        {
            Assert.IsFalse(resolver.FolderExists(file));
        }
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestFileExists(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        Assert.IsTrue(resolver.FileExists("test1.txt"));
        Assert.IsTrue(resolver.FileExists("/test1.txt"));
        Assert.IsFalse(resolver.FileExists("bbbbb.txt"));
        Assert.IsFalse(resolver.FileExists("aaaa/bbbb.log"));
        foreach (var folder in Folders)
        {
            Assert.IsFalse(resolver.FileExists(folder));
        }
        foreach (var file in Files)
        {
            Assert.IsTrue(resolver.FileExists(file));
        }
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestOpenFile(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        foreach (var file in Files)
        {
            using var stream = resolver.OpenFile(file);
            using var tr = new StreamReader(stream, Encoding.ASCII);
            Assert.AreEqual(file, tr.ReadToEnd());
        }
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestFileSize(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        foreach (var file in Files)
        {
            Assert.AreEqual(file.Length, resolver.FileSize(file));
        }
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestFileNotFound(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            resolver.OpenFile("not_found.txt");
        });
        Assert.ThrowsException<FileNotFoundException>(() =>
        {
            resolver.OpenFile("not/found");
        });
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            resolver.GetFiles("not/found");
        });
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            resolver.GetFolders("not/found");
        });
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestGetFiles(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        foreach (var folder in Folders)
        {
            var fpath = folder == "" ? "" : folder + '/';
            var expectedFiles = Files.Where(x => x.StartsWith(fpath) && !x[fpath.Length..].Contains('/')).ToList();
            CollectionAssert.AreEquivalent(expectedFiles, resolver.GetFiles(folder).ToList());
        }
    }

    [TestMethod]
    [DataRow("Disk")]
    [DataRow("ZipArchive")]
    [DataRow("VirtualSubdirectory")]
    [DataRow("Composite")]
    [DataRow("Package")]
    public void TestGetFolders(string implementationName)
    {
        var resolver = _resolvers[implementationName];
        foreach (var folder in Folders)
        {
            var fpath = folder == "" ? "" : folder + '/';
            var expectedFolders = Folders.Where(x => x != folder && x.StartsWith(fpath) && !x[fpath.Length..].Contains('/')).ToList();
            CollectionAssert.AreEquivalent(expectedFolders, resolver.GetFolders(folder).ToList());
        }
    }
}