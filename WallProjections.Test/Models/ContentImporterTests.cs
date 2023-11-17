using WallProjections.Models.Configuration;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="ContentImporter"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ContentImporterTests
{
    /// <summary>
    /// Location of the zip file for testing.
    /// </summary>
    private static string TestZip => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets/test.zip");

    /// <summary>
    /// Test that ensures the temp folder is removed after running <see cref="ContentImporter.Cleanup"/>
    /// </summary>
    [Test]
    public void ContentImporterCleanupTest()
    {
        var contentImporter = new ContentImporter();
        var config = contentImporter.Load(TestZip);

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.True);

        contentImporter.Cleanup();

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.False);
    }

    /// <summary>
    /// Test that loaded config matches expected config information.
    /// </summary>
    [Test]
    public void ConfigLoadTest()
    {
        IContentImporter contentImporter = new ContentImporter();
        IConfig config = contentImporter.Load(TestZip);
        IConfig config2 = new Config(new List<Hotspot>{ new(0, 1, 2, 3) });

        ConfigTests.AssertConfigsEqual(config, config2);
    }

    /// <summary>
    /// Test that the <see cref="ContentImporter.Load"/> method loads media files correctly.
    /// </summary>
    [Test]
    public void MediaLoadTest()
    {
        IContentImporter contentImporter = new ContentImporter();
        IConfig config = contentImporter.Load(TestZip);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")), Is.EqualTo("Hello World\n"));
        });

        contentImporter.Cleanup();
    }
}

