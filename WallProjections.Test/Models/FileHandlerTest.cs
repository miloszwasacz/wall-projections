using System.Collections.Immutable;
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
    private const string TestTxtFile = "text_0.txt";
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
    /// Test that loaded config matches expected config information
    /// </summary>
    [Test]
    public void LoadTest()
    {
        IFileHandler fileHandler = new FileHandler();
        var config = fileHandler.Load(TestZip);
        var config2 = new Config(new List<Hotspot> { new(0, new Coord(1,2,3), "test_0.txt", ImmutableList<string>.Empty, ImmutableList<string>.Empty) });

        Assert.That(config, Is.Not.Null);
        AssertConfigsEqual(config!, config2);

        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> clears the temp folder before loading
    /// </summary>
    [Test]
    public void LoadExistingDirectoryTest()
    {
        var fileHandler = new FileHandler();
        var oldFilePath = Path.Combine(FileHandler.ConfigFolderPath, Path.GetRandomFileName());

        #region Create folder and file for the test

        if (Directory.Exists(FileHandler.ConfigFolderPath))
            Directory.Delete(FileHandler.ConfigFolderPath, true);

        Directory.CreateDirectory(FileHandler.ConfigFolderPath);
        File.Create(oldFilePath).Close();
        Assert.That(File.Exists(oldFilePath), Is.True, "Could not create file in config folder for the test");

        #endregion

        var config = fileHandler.Load(TestZip);

        var textFilePath = Path.Combine(FileHandler.ConfigFolderPath, TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(textFilePath), Is.True);
            Assert.That(File.ReadAllText(textFilePath), Is.EqualTo(TestTxtFileContents));
            Assert.That(File.Exists(oldFilePath), Is.False);
        });

        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the zip file does not exist
    /// </summary>
    [Test]
    public void LoadNoZipTest()
    {
        var fileHandler = new FileHandler();
        var path = Path.GetRandomFileName() + ".zip";
        Assert.Throws<FileNotFoundException>(() => fileHandler.Load(path));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the zip file does not contain a config file
    /// </summary>
    [Test]
    public void LoadNoConfigTest()
    {
        var fileHandler = new FileHandler();
        Assert.Throws<FileNotFoundException>(() => fileHandler.Load(TestZipNoConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method throws an exception when the config file has invalid format
    /// </summary>
    [Test]
    public void LoadInvalidConfigTest()
    {
        var fileHandler = new FileHandler();
        Assert.Throws<JsonException>(() => fileHandler.Load(TestZipInvalidConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.Load" /> method loads media files correctly
    /// </summary>
    [Test]
    public void GetHotspotMediaTest()
    {
        IFileHandler fileHandler = new FileHandler();
        var config = fileHandler.Load(TestZip);


        var txtFilePath = Path.Combine(FileHandler.ConfigFolderPath, TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(txtFilePath), Is.True);
            Assert.That(File.ReadAllText(txtFilePath), Is.EqualTo(TestTxtFileContents));
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
        IFileHandler fileHandler = new FileHandler();
        var config = fileHandler.Load(TestZip);
        var contentProvider = new ContentProvider(config);
        Assert.That(contentProvider, Is.InstanceOf<IContentProvider>());
    }

    /// <summary>
    /// Test that the temp folder is removed after running <see cref="FileHandler.Dispose" />
    /// </summary>
    [Test]
    public void DisposeTest()
    {
        var fileHandler = new FileHandler();
        var config = fileHandler.Load(TestZip);

        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.True);

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);

        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.False);
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
        var fileHandler = new FileHandler();
        var tempFilePath = Path.Combine(FileHandler.ConfigFolderPath, Path.GetRandomFileName());

        fileHandler.Load(TestZip);

        var file = File.Create(tempFilePath);

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.True);

        file.Close();

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.False);
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
        var fileHandler = new FileHandler();
        fileHandler.Load(TestZip);

        System.Diagnostics.Process.Start("chmod", "000 " + FileHandler.ConfigFolderPath).WaitForExit();

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.True);

        System.Diagnostics.Process.Start("chmod", "777 " + FileHandler.ConfigFolderPath).WaitForExit();

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.False);
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
                
                Assert.That(hotspot1.DescriptionPath, Is.EqualTo(hotspot2.DescriptionPath));
                
                CollectionAssert.AreEqual(hotspot1.ImagePaths, hotspot2.ImagePaths);
                CollectionAssert.AreEqual(hotspot1.VideoPaths, hotspot2.VideoPaths);
            });
        }
    }
}
