using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;

namespace WallProjections.Test.Models;

/// <summary>
/// Tests for the <see cref="Config"/> class.
/// </summary>
[TestFixture]
[Author(name: "Thomas Parr")]
public class ConfigTest
{
    private static readonly double[,] TestMatrix = MockPythonProxy.CalibrationResult;

    /// <summary>
    /// Test to ensure the correct count is returned from <see cref="Config.Hotspots"/>.Count
    /// </summary>
    [Test]
    public void HotspotCountTest()
    {
        var hotspots = new List<Hotspot>();

        for (var i = 0; i < 5; i++)
        {
            var config = new Config(TestMatrix, hotspots);
            Assert.Multiple(() =>
            {
                Assert.That(config.HomographyMatrix, Is.EquivalentTo(TestMatrix));
                Assert.That(config.Hotspots, Has.Count.EqualTo(i));
            });
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
        var hotspots = new List<Hotspot>
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
        };
        var config = new Config(TestMatrix, hotspots);

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
