using System.Collections.Immutable;
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
            hotspots.Add(
                new Hotspot(
                    i,
                    new Coord(1, 2, 3),
                    "",
                    "",
                    ImmutableList<string>.Empty,
                    ImmutableList<string>.Empty
                )
            );
        }
    }

    /// <summary>
    /// Checks that <see cref="Config.GetHotspot"/> returns the correct hotspot.
    /// </summary>
    [Test]
    public void GetHotspotTest()
    {
        const string title = "Hotspot";
        var config = new Config(
            hotspots: new List<Hotspot>
            {
                new(1,
                    new Coord(0, 0, 0),
                    $"{title} 1",
                    "", new
                        List<string> { "image_1_0.jpg" }.ToImmutableList(),
                    ImmutableList<string>.Empty),
                new(2,
                    new Coord(0, 0, 0),
                    $"{title} 2",
                    "text_2.txt",
                    ImmutableList<string>.Empty,
                    ImmutableList<string>.Empty)
            }
        );

        var hotspot1 = config.GetHotspot(1);
        var hotspot2 = config.GetHotspot(2);
        var hotspot3 = config.GetHotspot(3);

        Assert.That(hotspot1, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(hotspot1!.Id, Is.EqualTo(1));
            Assert.That(hotspot1.Title, Is.EqualTo($"{title} 1"));
            Assert.That(hotspot1.ImagePaths[0], Is.EqualTo("image_1_0.jpg"));
        });

        Assert.Multiple(() =>
        {
            Assert.That(hotspot2, Is.Not.Null);
            Assert.That(hotspot2!.Id, Is.EqualTo(2));
            Assert.That(hotspot2.Title, Is.EqualTo($"{title} 2"));
            Assert.That(hotspot2.DescriptionPath, Is.EqualTo("text_2.txt"));
        });

        Assert.Multiple(() => { Assert.That(hotspot3, Is.Null); });
    }
}
