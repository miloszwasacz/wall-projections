using WallProjections.Models;
using WallProjections.Models.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="ContentCache"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ContentCacheTest
{
    /// <summary>
    /// Location of the zip file for testing.
    /// </summary>
    private static string TestZip => Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets/test.zip");

    /// <summary>
    /// Test if the <see cref="ContentCache" /> class is a singleton
    /// </summary>
    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = ContentCache.Instance;
        var instance2 = ContentCache.Instance;

        Assert.That(instance2, Is.SameAs(instance1));
    }

    /// <summary>
    /// Test that ensures the temp folder is removed after running <see cref="ContentCache.Dispose"/>
    /// </summary>
    [Test]
    public void CleanupTest()
    {
        var contentImporter = ContentCache.Instance;
        var config = contentImporter.Load(TestZip);

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.True);

        contentImporter.Dispose();

        Assert.That(Directory.Exists(contentImporter.GetHotspotMediaFolder(config.GetHotspot(0)!)), Is.False);
    }

    /// <summary>
    /// Test that loaded config matches expected config information.
    /// </summary>
    [Test]
    public void LoadTest()
    {
        IContentCache contentCache = ContentCache.Instance;
        var config = contentCache.Load(TestZip);
        var config2 = new Config(new List<Hotspot> { new(0, 1, 2, 3) });

        AssertConfigsEqual(config, config2);
    }

    /// <summary>
    /// Test that the <see cref="ContentCache.Load"/> method loads media files correctly.
    /// </summary>
    [Test]
    public void MediaLoadTest()
    {
        IContentCache contentCache = ContentCache.Instance;
        var config = contentCache.Load(TestZip);

        Assert.Multiple(() =>
        {
            Assert.That(
                File.Exists(Path.Combine(contentCache.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")),
                Is.True
            );
            Assert.That(
                File.ReadAllText(Path.Combine(contentCache.GetHotspotMediaFolder(config.GetHotspot(0)!), "0.txt")),
                Is.EqualTo("Hello World\n")
            );
        });

        contentCache.Dispose();
    }

    /// <summary>
    /// Asserts that two <see cref="Config"/> classes are equal.
    /// </summary>
    /// <param name="config1">First <see cref="Config"/> class to compare.</param>
    /// <param name="config2">Second <see cref="Config"/> class to compare.</param>
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
                Assert.That(hotspot1!.Id, Is.EqualTo(hotspot2!.Id), $"Hotspot {i} Id not equal in configs.");
                Assert.That(hotspot1.Position.X, Is.EqualTo(hotspot2.Position.X),
                    $"Hotspot {i} X position not equal in configs.");
                Assert.That(hotspot1.Position.Y, Is.EqualTo(hotspot2.Position.Y),
                    $"Hotspot {i} Y position not equal in configs.");
                Assert.That(hotspot1.Position.R, Is.EqualTo(hotspot2.Position.R),
                    $"Hotspot {i} radius not equal in configs.");
            });
        }
    }

    //TODO Test Overwriting existing temp folder
    //TODO Test Config not found
    //TODO Test Invalid config format
}
