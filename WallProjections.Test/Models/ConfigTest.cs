using WallProjections.Models;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="Config"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ConfigTest
{
    /// <summary>
    /// Test to ensure the correct count is returned from <see cref="Config.HotspotCount"/>
    /// </summary>
    [Test]
    public void HotspotCountTest()
    {
        var hotspots = new List<Hotspot>();

        for (var i = 0; i < 5; i++)
        {
            var config = new Config(hotspots);
            Assert.That(config.HotspotCount, Is.EqualTo(i));
            // hotspots.Add(new Hotspot(i, new Coord(1, 2, 3)));
        }
    }

    /// <summary>
    /// Checks that <see cref="Config.GetHotspot"/> returns the correct hotspot.
    /// </summary>
    [Test]
    public void GetHotspotTest()
    {
        var config = new Config(
            hotspots: new List<Hotspot>
            {
                // new(id: 1),
                // new(id: 2)
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
}
