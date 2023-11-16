using WallProjections.Models.Configuration;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="ContentImporter"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ContentImporterTest
{
    /// <summary>
    /// Location of the zip file for testing.
    /// </summary>
    private static string TestZip => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets/test.zip");

    /// <summary>
    /// Test that the <see cref="ContentImporter.Load"/> method loads the config and the file correctly.
    /// </summary>
    [Test]
    public void TestContentImporterLoad()
    {
        IContentImporter contentImporter = new ContentImporter();
        IConfig config = contentImporter.Load(TestZip);
        IConfig config2 = new Config(new List<Hotspot>{ new(0, 1, 2, 3) });

        ConfigTests.CheckConfigsEqual(config, config2);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")), Is.EqualTo("Hello World\n"));
        });

        contentImporter.Cleanup();
    }

    /// <summary>
    /// Test that ensures the temp folder is removed after running <see cref="ContentImporter.Cleanup"/>
    /// </summary>
    [Test]
    public void TestContentImporterCleanup()
    {
        var contentImporter = new ContentImporter();
        var config = contentImporter.Load(TestZip);

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.True);

        contentImporter.Cleanup();

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.False);
    }
}

