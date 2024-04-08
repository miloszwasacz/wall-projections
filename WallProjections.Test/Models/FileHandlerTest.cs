using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;

namespace WallProjections.Test.Models;

//TODO Add assertions for homography matrix in the Config
/// <summary>
/// Tests for the <see cref="FileHandler" /> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class FileHandlerTest
{
    private const string TestTxtFile = "text_0.txt";
    private const string TestTxtFileContents = "Hello World\n";
    private static string HotspotTitle(int id) => $"Hotspot {id}";
    private static readonly double[,] TestMatrix = MockPythonProxy.CalibrationResult;

    private static string TestAssets => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets");

    /// <summary>
    /// Location of the zip file for testing
    /// </summary>
    private static string TestZip => Path.Combine(TestAssets, "test.zip");

    /// <summary>
    /// Location of the zip file with no config file for testing
    /// </summary>
    private static string TestZipNoConfig => Path.Combine(TestAssets, "test_no_config.zip");

    /// <summary>
    /// Location of the zip file with an invalid config file for testing
    /// </summary>
    private static string TestZipInvalidConfigFile => Path.Combine(TestAssets, "test_invalid_config.zip");

    /// <summary>
    /// Location of a text file to check exception for non zip file
    /// </summary>
    private static string TestNonZipConfig => Path.Combine(TestAssets, "test.txt");

    /// <summary>
    /// Location of a zip file containing a file with only whitespace in the name
    /// </summary>
    private static string TestNonExistentConfig => Path.Combine(TestAssets, "does_not_exist.zip");

    /// <summary>
    /// Location to store current non test config while tests are running
    /// </summary>
    private static string CurrentConfigTempStore =>
        Path.Combine(IFileHandler.AppDataFolderPath, "TestTemp");

    /// <summary>
    /// Ensures that the current config is not lost during tests.
    /// </summary>
    [OneTimeSetUp]
    public void FileHandlerSetUp()
    {
        if (Directory.Exists(IFileHandler.CurrentConfigFolderPath))
        {
            Directory.Move(
                IFileHandler.CurrentConfigFolderPath,
                CurrentConfigTempStore
            );
        }
    }

    /// <summary>
    /// Ensures that the current config is moved back to the original position once the tests are finished.
    /// </summary>
    [OneTimeTearDown]
    public void FileHandlerTearDown()
    {
        if (Directory.Exists(IFileHandler.CurrentConfigFolderPath))
        {
            Directory.Delete(IFileHandler.CurrentConfigFolderPath, true);
        }

        Assert.That(!Directory.Exists(IFileHandler.CurrentConfigFolderPath));

        if (Directory.Exists(CurrentConfigTempStore))
        {
            Directory.Move(
                CurrentConfigTempStore,
                IFileHandler.CurrentConfigFolderPath
            );
        }
    }

    /// <summary>
    /// Ensures the imported config is deleted before any tests.
    /// </summary>
    [SetUp]
    public void DeleteImportedConfig()
    {
        if (!Directory.Exists(IFileHandler.CurrentConfigFolderPath)) return;

        Directory.Delete(IFileHandler.CurrentConfigFolderPath, true);
        Assert.That(!Directory.Exists(IFileHandler.CurrentConfigFolderPath));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.LoadConfig"/> method throws a
    /// <see cref="ConfigNotImportedException"/> if no config is imported
    /// </summary>
    [NonParallelizable]
    [Test]
    public void LoadNonImportedConfigTest()
    {
        // Check that there is no config imported
        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.False);

        Assert.Throws<ConfigNotImportedException>(() => new FileHandler().LoadConfig());
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.LoadConfig"/> method throws a
    /// <see cref="ConfigIOException"/> if the config file is already opened
    /// </summary>
    [NonParallelizable]
    [Test]
    public void LoadingOnBlockedFileTest()
    {
        Directory.CreateDirectory(IFileHandler.CurrentConfigFolderPath);

        // Creates file and keeps it open/blocked
        using var _ = File.Create(IFileHandler.CurrentConfigFilePath);

        Assert.That(File.Exists(IFileHandler.CurrentConfigFilePath));

        Assert.Throws<ConfigIOException>(() => new FileHandler().LoadConfig());
    }

    /// <summary>
    /// Test that loaded config matches expected config information
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigTest()
    {
        IFileHandler fileHandler = new FileHandler();
        var config = fileHandler.ImportConfig(TestZip);
        var config2 = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(1, 2, 3),
                HotspotTitle(0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        Assert.That(config, Is.Not.Null);
        AssertConfigsEqual(config!, config2);
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> clears the temp folder before loading
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigExistingDirectoryTest()
    {
        var fileHandler = new FileHandler();
        var oldFilePath = Path.Combine(IFileHandler.CurrentConfigFolderPath, Path.GetRandomFileName());

        #region Create folder and file for the test

        Directory.CreateDirectory(IFileHandler.CurrentConfigFolderPath);
        File.Create(oldFilePath).Close();
        Assert.That(File.Exists(oldFilePath), Is.True, "Could not create file in config folder for the test");

        #endregion

        fileHandler.ImportConfig(TestZip);

        var textFilePath = Path.Combine(IFileHandler.CurrentConfigFolderPath, TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(textFilePath), Is.True);
            Assert.That(File.ReadAllText(textFilePath), Is.EqualTo(TestTxtFileContents));
            Assert.That(File.Exists(oldFilePath), Is.False);
        });
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> method throws an exception when the zip file does not exist
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigNoZipTest()
    {
        var fileHandler = new FileHandler();
        var path = Path.GetRandomFileName() + ".zip";
        Assert.Throws<ExternalFileReadException>(() => fileHandler.ImportConfig(path));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> method throws a
    /// <see cref="ConfigInvalidException"/> when the zip file does not contain a config file
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigNoConfigTest()
    {
        var fileHandler = new FileHandler();
        Assert.Throws<ConfigInvalidException>(() => fileHandler.ImportConfig(TestZipNoConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> method throws a
    /// <see cref="ConfigInvalidException"/> when the config file has invalid format
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigInvalidConfigTest()
    {
        var fileHandler = new FileHandler();
        Assert.Throws<ConfigInvalidException>(() => fileHandler.ImportConfig(TestZipInvalidConfigFile));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig"/> method throws a
    /// <see cref="ConfigPackageFormatException"/> if a file to import is not a zip.
    /// </summary>
    [NonParallelizable]
    [Test]
    public void ImportNonZipFileTest()
    {
        var fileHandler = new FileHandler();
        // Non zip file
        Assert.Throws<ConfigPackageFormatException>(() => fileHandler.ImportConfig(TestNonZipConfig));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig"/> method throws a
    /// <see cref="ExternalFileReadException"/> if a file to import cannot be found.
    /// </summary>
    [NonParallelizable]
    [Test]
    public void ImportNonExistentFileTest()
    {
        var fileHandler = new FileHandler();

        // Non existent file
        Assert.Throws<ExternalFileReadException>(() => fileHandler.ImportConfig(TestNonExistentConfig));
    }

    [NonParallelizable]
    [Test]
    public void ImportNonAccessibleFileTest()
    {
        var fileHandler = new FileHandler();

        using var file = ZipFile.Open(TestZip, ZipArchiveMode.Update);

        Assert.Throws<ExternalFileReadException>(() => fileHandler.ImportConfig(TestZip));
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> method always deletes the
    /// <see cref="IFileHandler.TempConfigFolderPath"/>
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ImportConfigDisposeTest()
    {
        var fileHandler = new FileHandler();
        var path = TestZip;
        fileHandler.ImportConfig(path);

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.False);

        path = Path.GetRandomFileName() + ".zip";
        Assert.Throws<ExternalFileReadException>(() => fileHandler.ImportConfig(path));

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.False);
    }

    /// <summary>
    /// Test that a normal config exports correctly.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ExportConfigTest()
    {
        var tempFilePath = Path.GetTempFileName();
        File.Delete(tempFilePath);
        Assert.That(File.Exists(tempFilePath), Is.False, "Could not delete temp file from test.");

        var fileHandler = new FileHandler();
        fileHandler.ImportConfig(TestZip);
        AssertConfigImported();

        fileHandler.ExportConfig(tempFilePath);
        Assert.That(File.Exists(tempFilePath), Is.True, "Exported zip does not exist");
        {
            using var zipFile = ZipFile.OpenRead(tempFilePath);
            var configFile = zipFile.GetEntry("config.json");
            Assert.That(configFile, Is.Not.Null, "config.json not found in zip file.");

            var textFile = zipFile.GetEntry(TestTxtFile);
            Assert.That(textFile, Is.Not.Null, "text_0.txt not found in zip file.");

            using var textReader = new StreamReader(textFile!.Open());
            var textFileContent = textReader.ReadToEnd();
            Assert.That(textFileContent, Is.EqualTo(TestTxtFileContents));

            var imageFile = zipFile.GetEntry("image_1_0.png");
            Assert.That(imageFile, Is.Not.Null, "image_1_0.png not found in zip file.");
        }

        File.Delete(tempFilePath);
        Assert.That(File.Exists(tempFilePath), Is.False, "Could not delete temp file from test.");
    }

    /// <summary>
    /// Test that error is thrown if no config is imported when export is attempted.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void ExportConfigNoConfigTest()
    {
        var tempFilePath = Path.GetTempFileName();
        File.Delete(tempFilePath);
        Assert.That(File.Exists(tempFilePath), Is.False, "Could not delete temp file from test.");

        if (Directory.Exists(IFileHandler.CurrentConfigFolderPath))
            Directory.Delete(IFileHandler.CurrentConfigFolderPath);

        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.False,
            "Could not delete config folder for test.");

        var fileHandler = new FileHandler();

        Assert.That(() => fileHandler.ExportConfig(tempFilePath), Throws.InstanceOf<ConfigNotImportedException>());

        File.Delete(tempFilePath);
        Assert.That(File.Exists(tempFilePath), Is.False, "Could not delete temp file from test.");
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.ImportConfig" /> method loads media files correctly
    /// </summary>
    [Test]
    [NonParallelizable]
    public void GetHotspotMediaTest()
    {
        IFileHandler fileHandler = new FileHandler();
        fileHandler.ImportConfig(TestZip);

        var txtFilePath = Path.Combine(IFileHandler.CurrentConfigFolderPath, TestTxtFile);
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(txtFilePath), Is.True);
            Assert.That(File.ReadAllText(txtFilePath), Is.EqualTo(TestTxtFileContents));
        });
        //TODO Test more media files
    }

    /// <summary>
    /// Test that the temp folder is removed after running <see cref="IFileHandler.DeleteConfigFolder" />
    /// </summary>
    [Test]
    [NonParallelizable]
    public void DeleteConfigFolderTest()
    {
        var fileHandler = new FileHandler();
        fileHandler.ImportConfig(TestZip);

        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.True);

        Assert.That(IFileHandler.DeleteConfigFolder, Throws.Nothing);

        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.False);
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Test that no exception is thrown when <see cref="IFileHandler.DeleteConfigFolder" /> is called
    /// while the temp folder is still in use
    /// </summary>
    [Test]
    [NonParallelizable]
    [Platform("Win")]
    public void DeleteConfigFolderIOExceptionWindowsTest()
    {
        var fileHandler = new FileHandler();
        var tempFilePath = Path.Combine(IFileHandler.CurrentConfigFolderPath, Path.GetRandomFileName());

        fileHandler.ImportConfig(TestZip);

        var file = File.Create(tempFilePath);

        Assert.That(IFileHandler.DeleteConfigFolder, Throws.InstanceOf<ConfigIOException>());
        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.True);

        file.Close();

        Assert.That(IFileHandler.DeleteConfigFolder, Throws.Nothing);
        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.False);
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Test that no exception is thrown when <see cref="IFileHandler.DeleteConfigFolder" /> is called
    /// and the application has no write permissions to the temp folder
    /// </summary>
    [Test]
    [NonParallelizable]
    [Platform("Linux,MacOsX")]
    public void DeleteConfigFolderIOExceptionLinuxTest()
    {
        var fileHandler = new FileHandler();
        fileHandler.ImportConfig(TestZip);

        Process.Start("chmod", "000 " + IFileHandler.CurrentConfigFolderPath).WaitForExit();

        Assert.That(IFileHandler.DeleteConfigFolder, Throws.InstanceOf<ConfigIOException>());
        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.True);

        Process.Start("chmod", "777 " + IFileHandler.CurrentConfigFolderPath).WaitForExit();

        Assert.That(IFileHandler.DeleteConfigFolder, Throws.Nothing);
        Assert.That(Directory.Exists(IFileHandler.CurrentConfigFolderPath), Is.False);
    }

    /// <summary>
    /// Test that config is correctly saved in already existing empty config folder.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigBasicTest()
    {
        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        fileHandler.SaveConfig(config);

        Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));

        var newConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(config, newConfig);
    }

    /// <summary>
    /// Test that config is correctly saved without previously existing config folder.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigBasicNoFolderTest()
    {
        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        fileHandler.SaveConfig(config);

        Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));

        var newConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(config, newConfig);
    }

    /// <summary>
    /// Test that config is correctly saved without previously existing config folder.
    /// </summary>
    [NonParallelizable]
    [Test]
    public void SaveConfigExistingTempFolderTest()
    {
        // Creates an existing temporary config folder with a file to ensure they are deleted correctly.
        Directory.CreateDirectory(IFileHandler.TempConfigFolderPath);

        // Immediately close file to ensure no IOException
        File.Create(IFileHandler.TempConfigFilePath).Close();

        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        fileHandler.SaveConfig(config);

        Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));

        var newConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(config, newConfig);
    }

    /// <summary>
    /// Test that <see cref="FileHandler.SaveConfig"/> still saves correctly if a
    /// file is open inside the <see cref="IFileHandler.TempConfigFolderPath"/>.
    /// </summary>
    /// <remarks>
    ///     This test only passes on Linux and MacOS,
    ///     since they keep a reference to a deleted file.
    /// </remarks>
    [NonParallelizable]
    [Platform("Linux,MacOsX")]
    [Test]
    public void SaveConfigWithNonWritableTempFileUnixTest()
    {
        // Creates an existing temporary config folder with a file to ensure they are deleted correctly.
        Directory.CreateDirectory(IFileHandler.TempConfigFolderPath);

        // Keep new file open so that temp folder is not writable.
        File.Create(IFileHandler.TempConfigFilePath);

        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        Assert.That(File.Exists(IFileHandler.TempConfigFilePath));

        fileHandler.SaveConfig(config);

        var newConfig = fileHandler.LoadConfig();

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.False);
        AssertConfigsEqual(config, newConfig);
    }

    /// <summary>
    /// Test that <see cref="FileHandler.SaveConfig"/> throws a <see cref="ConfigIOException"/> if a
    /// file is open inside the <see cref="IFileHandler.TempConfigFolderPath"/> on Windows.
    /// </summary>
    /// <remarks>
    ///     This test only passes on Windows, as Windows will block deletion of a parent folder
    ///     of a file.
    /// </remarks>
    [NonParallelizable]
    [Platform("Win")]
    [Test]
    public void SaveConfigWithNonWritableTempFileWindowsTest()
    {
        // Creates an existing temporary config folder with a file.
        Directory.CreateDirectory(IFileHandler.TempConfigFolderPath);

        // Keep new file open so that temp folder is not writable.
        using var file = File.Create(IFileHandler.TempConfigFilePath);

        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        Assert.That(File.Exists(IFileHandler.TempConfigFilePath));

        Assert.Throws<ConfigIOException>(() => fileHandler.SaveConfig(config));

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.True);

        file.Close();

        Directory.Delete(IFileHandler.TempConfigFolderPath);
    }

    /// <summary>
    /// Test that <see cref="FileHandler.SaveConfig"/> throws a
    /// <see cref="ConfigIOException"/> if a file has the same path as the temp folder
    /// </summary>
    [NonParallelizable]
    [Test]
    public void SaveConfigWithFileAtTempFolderTest()
    {
        File.Create(IFileHandler.TempConfigFolderPath).Close();

        Assert.That(File.Exists(IFileHandler.TempConfigFolderPath));

        var fileHandler = new FileHandler();
        var config = new Config(TestMatrix, ImmutableList<Hotspot>.Empty);

        Assert.Throws<ConfigIOException>(() => fileHandler.SaveConfig(config));

        // Clean up file to get rid of errors
        File.Delete(IFileHandler.TempConfigFolderPath);
    }

    /// <summary>
    /// Test that config with a single hotspot with description is saved correctly with file imported.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigDescriptionTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0, new Coord(0, 0, 0), "Hotspot 0", textFilePath, ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));
        });

        var savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
    }

    /// <summary>
    /// Test that config with a single hotspot with description and image is saved correctly.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigDescriptionPlusImageTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");

        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                textFilePath,
                new List<string> { imageFilePath }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));

            // File copied into the config folder.
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));

            // Original file not deleted.
            Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets",
                "test_image.png")));
        });

        var savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                new List<string> { "image_0_0.png" }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
    }

    /// <summary>
    /// Test that config with a single hotspot with description and image is saved correctly.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigTwoImagesTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var imageFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image_2.jpg");

        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                textFilePath,
                new List<string> { imageFilePath, imageFilePath2 }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));

            // File copied into the config folder.
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_1.jpg")));

            // Original file not deleted.
            Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets",
                "test_image.png")));
            Assert.That(
                File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image_2.jpg")));
        });

        var savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                new List<string> { "image_0_0.png", "image_0_1.jpg" }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
    }

    /// <summary>
    /// Test that config with a single hotspot with description and video is saved correctly with files imported.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigDescriptionPlusVideoTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                textFilePath,
                ImmutableList<string>.Empty,
                new List<string> { videoFilePath }.ToImmutableList())
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));

            // File copied into the config folder.
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "video_0_0.mp4")));

            // Original file not deleted.
            Assert.That(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets",
                "test_video.mp4")));
        });

        var savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                new List<string> { "video_0_0.mp4" }.ToImmutableList())
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
    }

    /// <summary>
    /// Test that config with a two hotspots is saved correctly with files imported.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigTwoHotspotsTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var textFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_2.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                textFilePath,
                new List<string> { imageFilePath }.ToImmutableList(),
                new List<string> { videoFilePath }.ToImmutableList()),
            new(1,
                new Coord(0, 0, 0),
                HotspotTitle(1),
                textFilePath2,
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));

            // File copied into the config folder.
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "video_0_0.mp4")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));

            // Original files not deleted.
            Assert.That(File.Exists(textFilePath));
            Assert.That(File.Exists(textFilePath2));
            Assert.That(File.Exists(imageFilePath));
            Assert.That(File.Exists(videoFilePath));
        });

        var savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a test file.\r\n"));

        savedTextFileContents = File.ReadAllText(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_1.txt"));
        Assert.That(savedTextFileContents, Is.EqualTo("This is a second test file.\r\n"));

        // Config is transformed by saving to have relative paths.
        var newConfig = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                new List<string> { "image_0_0.png" }.ToImmutableList(),
                new List<string> { "video_0_0.mp4" }.ToImmutableList()),
            new(1,
                new Coord(0, 0, 0),
                HotspotTitle(1),
                "text_1.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(newConfig, loadedConfig);
    }

    /// <summary>
    /// Test that config with files already imported is saved correctly.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigImportedFilesTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test.txt");
        var textFilePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_2.txt");
        var imageFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");
        var videoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_video.mp4");

        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                textFilePath,
                new List<string> { imageFilePath }.ToImmutableList(),
                new List<string> { videoFilePath }.ToImmutableList()),
            new(1,
                new Coord(0, 0, 0),
                HotspotTitle(1),
                textFilePath2,
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt"),
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty),
            new(1,
                new Coord(0, 0, 0),
                HotspotTitle(1),
                Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_1.txt"),
                new List<string>
                    { Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png") }.ToImmutableList(),
                new List<string>
                    { Path.Combine(IFileHandler.CurrentConfigFolderPath, "video_0_0.mp4") }.ToImmutableList())
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "config.json")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));

            // File copied into the config folder.
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "video_1_0.mp4")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_1_0.png")));

            // Original files removed from the config folder.
            Assert.That(!File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "video_0_0.mp4")));
            Assert.That(!File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));
        });

        config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                HotspotTitle(0),
                "text_0.txt",
                ImmutableList<string>.Empty,
                ImmutableList<string>.Empty),
            new(1,
                new Coord(0, 0, 0),
                HotspotTitle(1),
                "text_1.txt",
                new List<string> { "image_1_0.png" }.ToImmutableList(),
                new List<string> { "video_1_0.mp4" }.ToImmutableList())
        });

        var loadedConfig = fileHandler.LoadConfig();

        AssertConfigsEqual(config, loadedConfig);
    }

    /// <summary>
    /// Test that config with a single hotspot with non existent description throws
    /// <see cref="ExternalFileReadException"/> and resets config back.
    /// </summary>
    [Test]
    [NonParallelizable]
    public void SaveConfigNonExistentFileTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestAssets, "test.txt");
        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "Hotspot 0",
                textFilePath,
                new List<string> { Path.Combine(TestAssets, "test_image.png") }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(IFileHandler.CurrentConfigFilePath));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));
        });

        textFilePath = TestNonExistentConfig;

        config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0, new Coord(0, 0, 0), "Hotspot 0", textFilePath, ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        Assert.Throws<ExternalFileReadException>(() => fileHandler.SaveConfig(config));

        // Image should still exist even though the second config does not have one
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(IFileHandler.CurrentConfigFilePath));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "text_0.txt")));
            Assert.That(File.Exists(Path.Combine(IFileHandler.CurrentConfigFolderPath, "image_0_0.png")));
        });
    }

    /// <summary>
    /// Test that the <see cref="FileHandler.SaveConfig"/> method clears the temp folder on exit.
    /// </summary>
    [NonParallelizable]
    [Test]
    public void SaveConfigDisposeTest()
    {
        var fileHandler = new FileHandler();
        var textFilePath = Path.Combine(TestAssets, "test.txt");
        var config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0,
                new Coord(0, 0, 0),
                "Hotspot 0",
                textFilePath,
                new List<string> { Path.Combine(TestAssets, "test_image.png") }.ToImmutableList(),
                ImmutableList<string>.Empty)
        });

        fileHandler.SaveConfig(config);

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.False);

        textFilePath = TestNonExistentConfig;

        config = new Config(TestMatrix, new List<Hotspot>
        {
            new(0, new Coord(0, 0, 0), "Hotspot 0", textFilePath, ImmutableList<string>.Empty,
                ImmutableList<string>.Empty)
        });

        Assert.Throws<ExternalFileReadException>(() => fileHandler.SaveConfig(config));

        Assert.That(Directory.Exists(IFileHandler.TempConfigFolderPath), Is.False);
    }

    /// <summary>
    /// Asserts that two <see cref="Config"/> classes are equal
    /// </summary>
    /// <param name="config1">First <see cref="Config"/> class to compare</param>
    /// <param name="config2">Second <see cref="Config"/> class to compare</param>
    private static void AssertConfigsEqual(IConfig config1, IConfig config2)
    {
        Assert.That(config1.Hotspots, Has.Count.EqualTo(config2.Hotspots.Count));

        CollectionAssert.AreEqual(config1.HomographyMatrix, config2.HomographyMatrix);

        for (var i = 0; i < config1.Hotspots.Count; i++)
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

                Assert.That(hotspot1.Title, Is.EqualTo(hotspot2.Title));

                Assert.That(hotspot1.DescriptionPath, Is.EqualTo(hotspot2.DescriptionPath));

                CollectionAssert.AreEqual(hotspot1.ImagePaths, hotspot2.ImagePaths);
                CollectionAssert.AreEqual(hotspot1.VideoPaths, hotspot2.VideoPaths);
            });
        }
    }

    /// <summary>
    /// Asserts that the config file has been correctly imported
    /// </summary>
    private static void AssertConfigImported()
    {
        Assert.That(File.Exists(IFileHandler.CurrentConfigFilePath), Is.True);
    }
}
