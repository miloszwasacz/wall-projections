using System.Text.Json;
using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="FileHandler" /> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class FileHandlerTest
{
    private const string TestTxtFile = "0.txt";
    private const string TestTxtFileContents = "Hello World\n";

    /// <summary>
    /// Location of the zip file for testing
    /// </summary>
    private static string TestZip => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.zip");

    /// <summary>
    /// Location of the zip file with no config file for testing
    /// </summary>
    private static string TestZipNoConfig =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_no_config.zip");

    /// <summary>
    /// Location of the zip file with an invalid config file for testing
    /// </summary>
    private static string TestZipInvalidConfig =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_invalid_config.zip");

    /// <summary>
    /// Test if the <see cref="FileHandler" /> class is a singleton
    /// </summary>
    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = FileHandler.Instance;
        var instance2 = FileHandler.Instance;

        Assert.That(instance2, Is.SameAs(instance1));
    }

    /// <summary>
    /// Test that loaded config matches expected config information
    /// </summary>
    [Test]
    public void LoadTest()
    {
        IFileHandler fileHandler = CreateInstance();
        var config = fileHandler.Load(TestZip);
        var config2 = new Config(new List<Hotspot> { new(0, 1, 2, 3) });

        AssertConfigsEqual(config, config2);

        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> clears the temp folder before loading
    /// </summary>
    [Test]
    public void LoadExistingDirectoryTest()
    {
        var contentCache = CreateInstance();
        var oldFilePath = Path.Combine(contentCache.TempPath, Path.GetRandomFileName());

        #region Create temp folder and file for the test

        if (Directory.Exists(contentCache.TempPath))
            Directory.Delete(contentCache.TempPath, true);

        Directory.CreateDirectory(contentCache.TempPath);
        File.Create(oldFilePath).Close();
        Assert.That(File.Exists(oldFilePath), Is.True, "Could not create file in temp folder for the test");

        #endregion

        var config = contentCache.Load(TestZip);

        var mediaFolder = Path.Combine(contentCache.GetHotspotMediaFolder(config.GetHotspot(0)!), TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(mediaFolder), Is.True);
            Assert.That(File.ReadAllText(mediaFolder), Is.EqualTo(TestTxtFileContents));
            Assert.That(File.Exists(oldFilePath), Is.False);
        });

        contentCache.Dispose();
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the zip file does not exist
    /// </summary>
    [Test]
    public void LoadNoZipTest()
    {
        var contentCache = CreateInstance();
        var path = Path.GetRandomFileName() + ".zip";
        Assert.Throws<FileNotFoundException>(() => contentCache.Load(path));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the zip file does not contain a config file
    /// </summary>
    [Test]
    public void LoadNoConfigTest()
    {
        var contentCache = CreateInstance();
        Assert.Throws<FileNotFoundException>(() => contentCache.Load(TestZipNoConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the config file has invalid format
    /// </summary>
    [Test]
    public void LoadInvalidConfigTest()
    {
        var contentCache = CreateInstance();
        Assert.Throws<JsonException>(() => contentCache.Load(TestZipInvalidConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method loads media files correctly
    /// </summary>
    [Test]
    public void GetHotspotMediaFolderTest()
    {
        IFileHandler fileHandler = CreateInstance();
        var config = fileHandler.Load(TestZip);

        var mediaFolder = Path.Combine(fileHandler.GetHotspotMediaFolder(config.GetHotspot(0)!), TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(mediaFolder), Is.True);
            Assert.That(File.ReadAllText(mediaFolder), Is.EqualTo(TestTxtFileContents));
        });
        //TODO Test more media files

        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that <see cref="FileHandler.CreateContentProvider" /> returns an instance of <see cref="IContentProvider" />
    /// </summary>
    [Test]
    public void CreateContentProviderTest()
    {
        IFileHandler fileHandler = CreateInstance();
        var config = fileHandler.Load(TestZip);
        var contentProvider = fileHandler.CreateContentProvider(config);
        Assert.That(contentProvider, Is.InstanceOf<IContentProvider>());
    }

    /// <summary>
    /// Test that the temp folder is removed after running <see cref="FileHandler.Dispose" />
    /// </summary>
    [Test]
    public void DisposeTest()
    {
        var contentCache = CreateInstance();
        var config = contentCache.Load(TestZip);

        Assert.That(Directory.Exists(contentCache.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.True);

        Assert.That(() => contentCache.Dispose(), Throws.Nothing);

        Assert.That(Directory.Exists(contentCache.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.False);
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Test that no exception is thrown when <see cref="FileHandler.Dispose" /> is called
    /// while the temp folder is still in use
    /// </summary>
    [Test]
    [Platform("Win")]
    public void DisposeIOExceptionWindowsTest()
    {
        var contentCache = CreateInstance();
        var tempFilePath = Path.Combine(contentCache.TempPath, Path.GetRandomFileName());

        contentCache.Load(TestZip);

        var file = File.Create(tempFilePath);

        Assert.That(() => contentCache.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(contentCache.TempPath), Is.True);

        file.Close();

        Assert.That(() => contentCache.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(contentCache.TempPath), Is.False);
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Test that no exception is thrown when <see cref="FileHandler.Dispose" /> is called
    /// and the application has no write permissions to the temp folder
    /// </summary>
    [Test]
    [Platform("Linux,MacOsX")]
    public void DisposeIOExceptionLinuxTest()
    {
        var contentCache = CreateInstance();
        contentCache.Load(TestZip);

        System.Diagnostics.Process.Start("chmod", "000 " + contentCache.TempPath).WaitForExit();

        Assert.That(() => contentCache.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(contentCache.TempPath), Is.True);

        System.Diagnostics.Process.Start("chmod", "777 " + contentCache.TempPath).WaitForExit();

        Assert.That(() => contentCache.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(contentCache.TempPath), Is.False);
    }

    /// <summary>
    /// Uses reflection to get the private constructor of <see cref="FileHandler" />,
    /// so that the global instance is not used
    /// </summary>
    /// <returns>A new instance of <see cref="FileHandler" /></returns>
    /// <exception cref="InvalidCastException">When the object cannot be instantiated as <see cref="FileHandler" /></exception>
    private static FileHandler CreateInstance()
    {
        var type = typeof(FileHandler);
        var res = Activator.CreateInstance(type, true) as FileHandler;
        return res ?? throw new InvalidCastException("Could not construct ContentCache");
    }

    /// <summary>
    /// Asserts that two <see cref="Config"/> classes are equal
    /// </summary>
    /// <param name="config1">First <see cref="Config"/> class to compare</param>
    /// <param name="config2">Second <see cref="Config"/> class to compare</param>
    private static void AssertConfigsEqual(IConfig config1, IConfig config2)
    {
        Assert.That(config1.HotspotCount, Is.EqualTo(config2.HotspotCount));

        for (var i = 0; i < config1.HotspotCount; i++)
        {
            Assert.Multiple(() =>
            {
                var hotspot1 = config1.GetHotspot(i);
                var hotspot2 = config2.GetHotspot(i);
                Assert.That(hotspot1, Is.Not.Null, $"Hotspot {i} null on config1");
                Assert.That(hotspot2, Is.Not.Null, $"Hotspot {i} null on config2");
                Assert.That(hotspot1!.Id, Is.EqualTo(hotspot2!.Id), $"Hotspot {i} Id not equal in configs");
                Assert.That(hotspot1.Position.X, Is.EqualTo(hotspot2.Position.X),
                    $"Hotspot {i} X position not equal in configs");
                Assert.That(hotspot1.Position.Y, Is.EqualTo(hotspot2.Position.Y),
                    $"Hotspot {i} Y position not equal in configs");
                Assert.That(hotspot1.Position.R, Is.EqualTo(hotspot2.Position.R),
                    $"Hotspot {i} radius not equal in configs");
            });
        }
    }
}
