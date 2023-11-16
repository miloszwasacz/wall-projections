using WallProjections.Models.Configuration;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the Config class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ConfigTests
{
    /// <summary>
    /// Location of the zip file for testing.
    /// </summary>
    private static string TestZip => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets/test.zip");

    /// <summary>
    /// Test to ensure the correct count is returned from <see cref="Config.HotspotCount"/>
    /// </summary>
    [Test]
    public void TestHotspotCount()
    {
        var config = new Config(
            hotspots: new List<Hotspot>
            {
                new(id: 1),
                new(id: 2)
            }
        );

        Assert.That(config.HotspotCount, Is.EqualTo(2));
    }

    /// <summary>
    /// Checks that <see cref="Config.GetHotspot"/> returns the correct hotspot.
    /// </summary>
    [Test]
    public void TestGetHotspot()
    {
        var config = new Config(
            hotspots: new List<Hotspot>
            {
                new(id: 1),
                new(id: 2)
            }
        );

        var hotspot1 = config.GetHotspot(1);
        Assert.That(hotspot1, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(hotspot1!.Id, Is.EqualTo(1));
            Assert.That(config.GetHotspot(3), Is.Null);
        });
    }

    /// <summary>
    /// Tests that loaded config from <see cref="ContentImporter.LoadConfig"/> loads correctly.
    /// </summary>
    [Test]
    public void TestConfigLoad()
    {
        IContentImporter contentImporter = new ContentImporter();
        var hotspots = new List<Hotspot> { new(
                id: 0,
                x: 1,
                y: 2,
                r: 3
            ) };
        IConfig config = new Config(hotspots);

        // Check read file is same as saved config.
        var loadedConfig = contentImporter.LoadConfig(TestZip);

        CheckConfigsEqual(config, loadedConfig);
    }

    /// <summary>
    /// Test that the <see cref="ContentImporter.Load"/> method loads the config and the file correctly.
    /// </summary>
    [Test]
    public void TestContentImporterLoad()
    {
        IContentImporter contentImporter = new ContentImporter();
        IConfig config = contentImporter.Load(TestZip);
        IConfig config2 = new Config(new List<Hotspot>{ new(0, 1, 2, 3) });

        CheckConfigsEqual(config, config2);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(contentImporter.GetHotspotFolder(config.GetHotspot(0)!), "0.txt")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(contentImporter.GetHotspotFolder(config.GetHotspot(0)!), "0.txt")), Is.EqualTo("Hello World\n"));
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
        contentImporter.Load(TestZip);

        Assert.That(Directory.Exists(contentImporter.TempPath), Is.True);

        contentImporter.Cleanup();

        Assert.That(Directory.Exists(contentImporter.TempPath), Is.False);
    }

    /// <summary>
    /// Asserts that two <see cref="Config"/> classes are equal.
    /// </summary>
    /// <param name="config1">First <see cref="Config"/> class to compare.</param>
    /// <param name="config2">Second <see cref="Config"/> class to compare.</param>
    private static void CheckConfigsEqual(IConfig config1, IConfig config2)
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
                Assert.That(hotspot1!.Id, Is.EqualTo(hotspot2!.Id), $"Hotspot {i} Id not equal in configs.");
                Assert.That(hotspot1.Position.X, Is.EqualTo(hotspot2.Position.X), $"Hotspot {i} X position not equal in configs.");
                Assert.That(hotspot1.Position.Y, Is.EqualTo(hotspot2.Position.Y), $"Hotspot {i} Y position not equal in configs.");
                Assert.That(hotspot1.Position.R, Is.EqualTo(hotspot2.Position.R), $"Hotspot {i} radius not equal in configs.");
            });
        }
    }

}
