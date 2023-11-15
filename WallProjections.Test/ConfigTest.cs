using WallProjections.Configuration;

namespace WallProjections.Test;

/// <summary>
/// Tests for the Config class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ConfigTests
{
    /// <summary>
    /// Default location to save config during tests.
    /// </summary>
    private const string ConfigLocation = "config_test.json";

    /// <summary>
    /// Checks that GetHotspot returns the correct hotspot.
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
    /// Tests that loaded config is identical to the original config.
    /// </summary>
    [Test]
    public void TestSaveAndLoad()
    {
        var hotspots = new List<Hotspot> { new(
                id: 0,
                x: 1,
                y: 2,
                r: 3
            ) };
        var original = new Config(hotspots, ConfigLocation);

        original.SaveConfig(Path.GetTempPath());


        // Check read file is same as saved config.
        var recreated = ContentImporter.LoadConfig(Path.GetTempPath(), ConfigLocation);

        Assert.Multiple(() =>
        {
            Assert.That(original.HotspotCount(), Is.EqualTo(recreated.HotspotCount()));

            Assert.That(recreated.ConfigLocation, Is.EqualTo("config_test.json"));
            Assert.That(recreated.GetHotspot(0), Is.Not.Null);
        });

        var originalHotspot = original.GetHotspot(0);
        var recreatedHotspot = recreated.GetHotspot(0);

        Assert.Multiple(() =>
        {
            Assert.That(originalHotspot!.Position.X, Is.EqualTo(recreatedHotspot!.Position.X));
            Assert.That(originalHotspot.Position.Y, Is.EqualTo(recreatedHotspot.Position.Y));
            Assert.That(originalHotspot.Position.R, Is.EqualTo(recreatedHotspot.Position.R));
        });
    }
}
