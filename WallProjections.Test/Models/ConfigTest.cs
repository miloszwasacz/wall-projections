using WallProjections.Models.Configuration;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="Config"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ConfigTests
{


    /// <summary>
    /// Test to ensure the correct count is returned from <see cref="Config.HotspotCount"/>
    /// </summary>
    [Test]
    public void TestHotspotCount()
    {
        var hotspots = new List<Hotspot>();

        for (int i = 0; i < 5; i++)
        {
            var config = new Config(hotspots);
            Assert.That(config.HotspotCount, Is.EqualTo(i));
            hotspots.Add(new Hotspot(i, new Coord(1, 2, 3)));
        }
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

        Assert.Multiple(() =>
        {
            Assert.That(hotspot1, Is.Not.Null);
            Assert.That(hotspot1!.Id, Is.EqualTo(1));
            Assert.That(config.GetHotspot(3), Is.Null);
        });
    }

    /// <summary>
    /// Asserts that two <see cref="Config"/> classes are equal.
    /// </summary>
    /// <param name="config1">First <see cref="Config"/> class to compare.</param>
    /// <param name="config2">Second <see cref="Config"/> class to compare.</param>
    public static void CheckConfigsEqual(IConfig config1, IConfig config2)
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
