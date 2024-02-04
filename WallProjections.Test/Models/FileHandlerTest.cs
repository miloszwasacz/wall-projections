using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
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
        var config2 = new Config(new List<Hotspot> { new(0, new Coord(1,2,3), "text_0.txt", ImmutableList<string>.Empty, ImmutableList<string>.Empty) });

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

        fileHandler.Load(TestZip);

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
        fileHandler.Load(TestZip);


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
    /// Test that the temp folder is removed after running <see cref="FileHandler.Dispose" />
    /// </summary>
    [Test]
    public void DisposeTest()
    {
        var fileHandler = new FileHandler();
        fileHandler.Load(TestZip);

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

        Process.Start("chmod", "000 " + FileHandler.ConfigFolderPath).WaitForExit();

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.True);

        Process.Start("chmod", "777 " + FileHandler.ConfigFolderPath).WaitForExit();

        Assert.That(() => fileHandler.Dispose(), Throws.Nothing);
        Assert.That(Directory.Exists(FileHandler.ConfigFolderPath), Is.False);
    }

    /// <summary>
    /// Test that config is correctly saved in already existing empty config folder.
    /// </summary>
    [Test]
    public void SaveBasicConfig()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var config = new Config(ImmutableList<Hotspot>.Empty);

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));

        var newConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(config, newConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config is correctly saved without previously existing config folder.
    /// </summary>
    [Test]
    public void SaveBasicConfigNoFolder()
    {
        #region Ensure Config Folder Deleted
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var config = new Config(ImmutableList<Hotspot>.Empty);

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));

        var newConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(config, newConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config with a single hotspot with description is saved correctly with file imported.
    /// </summary>
    [Test]
    public void SaveConfigWithDescription()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var config = new Config(new List<Hotspot>
        {
            new(0, new Coord(0,0,0), textFilePath, ImmutableList<string>.Empty, ImmutableList<string>.Empty)
        });

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        var savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(new List<Hotspot>
        {
            new(0, new Coord(0, 0, 0), "text_0.txt", ImmutableList<string>.Empty, ImmutableList<string>.Empty)
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config with a single hotspot with description and image is saved correctly.
    /// </summary>
    [Test]
    public void SaveConfigWithDescriptionPlusImage()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");

        var config = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0,0,0),
                textFilePath,
                new List<string> { imageFilePath }.ToImmutableList(),
                ImmutableList<string>.Empty )
        });

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        // File copied into the config folder.
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_0_0.png")));

        // Original file not deleted.
        Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png")));

        var savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                new List<string>{ "image_0_0.png" }.ToImmutableList(),
                ImmutableList<string>.Empty )
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
    }

        /// <summary>
    /// Test that config with a single hotspot with description and image is saved correctly.
    /// </summary>
    [Test]
    public void SaveConfigWithTwoImages()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var imageFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image_2.jpg");

        var config = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0,0,0),
                textFilePath,
                new List<string> { imageFilePath, imageFilePath2 }.ToImmutableList(),
                ImmutableList<string>.Empty )
        });

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        // File copied into the config folder.
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_0_0.png")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_0_1.jpg")));

        // Original file not deleted.
        Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png")));
        Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image_2.jpg")));

        var savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                new List<string>{ "image_0_0.png", "image_0_1.jpg" }.ToImmutableList(),
                ImmutableList<string>.Empty )
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config with a single hotspot with description and video is saved correctly with files imported.
    /// </summary>
    [Test]
    public void SaveConfigWithDescriptionPlusVideo()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0,0,0),
                textFilePath,
                ImmutableList<string>.Empty,
                new List<string>{ videoFilePath }.ToImmutableList() )
        });

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        // File copied into the config folder.
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "video_0_0.mp4")));

        // Original file not deleted.
        Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4")));

        var savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                new List<string>{ "video_0_0.mp4" }.ToImmutableList() )
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config with a two hotspots is saved correctly with files imported.
    /// </summary>
    [Test]
    public void SaveConfigWithTwoHotspots()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var textFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_2.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0,0,0),
                textFilePath,
                new List<string>{ imageFilePath }.ToImmutableList(),
                new List<string>{ videoFilePath }.ToImmutableList() ),
            new (1,
                new Coord(0,0,0),
                textFilePath2,
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        fileHandler.Save(config);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        // File copied into the config folder.
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "video_0_0.mp4")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_0_0.png")));

        // Original files not deleted.
        Assert.That(File.Exists(textFilePath));
        Assert.That(File.Exists(textFilePath2));
        Assert.That(File.Exists(imageFilePath));
        Assert.That(File.Exists(videoFilePath));

        var savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        savedTextFileContents = File.ReadAllText(Path.Combine(FileHandler.ConfigFolderPath, "text_1.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a second test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                new List<string>{ "image_0_0.png" }.ToImmutableList(),
                new List<string>{ "video_0_0.mp4" }.ToImmutableList() ),
            new(1,
                new Coord(0, 0, 0),
                "text_1.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty )
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
    }

    /// <summary>
    /// Test that config with relative files already imported is saved correctly.
    /// </summary>
    [Test]
    public void SaveConfigWithRelativeFiles()
    {
        #region Ensure Config Folder Reset
        if (Directory.Exists(FileHandler.ConfigFolderPath))
        {
            Directory.Delete(FileHandler.ConfigFolderPath, true);
            Assert.That(!Directory.Exists(FileHandler.ConfigFolderPath));
            Directory.CreateDirectory(FileHandler.ConfigFolderPath);
            Assert.That(Directory.Exists(FileHandler.ConfigFolderPath));
        }
        #endregion

        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var textFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_2.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0,0,0),
                textFilePath,
                new List<string>{ imageFilePath }.ToImmutableList(),
                new List<string>{ videoFilePath }.ToImmutableList() ),
            new (1,
                new Coord(0,0,0),
                textFilePath2,
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        fileHandler.Save(config);

        var newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                 ImmutableList<string>.Empty),
            new(1,
                new Coord(0, 0, 0),
                "text_1.txt",
                new List<string>{ "image_0_0.png" }.ToImmutableList(),
                new List<string>{ "video_0_0.mp4" }.ToImmutableList() )
        });

        fileHandler.Save(newConfig);

        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "config.json")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "text_0.txt")));

        // File copied into the config folder.
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "video_1_0.mp4")));
        Assert.That(File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_1_0.png")));

        // Original files removed from the config folder.
        Assert.That(!File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "video_0_0.mp4")));
        Assert.That(!File.Exists(Path.Combine(FileHandler.ConfigFolderPath, "image_0_0.png")));

        newConfig = new Config(new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty),
            new(1,
                new Coord(0, 0, 0),
                "text_1.txt",
                new List<string> { "image_1_0.png" }.ToImmutableList(),
                new List<string> { "video_1_0.mp4" }.ToImmutableList())
        });

        var loadedConfig = FileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
        fileHandler.Dispose();
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
