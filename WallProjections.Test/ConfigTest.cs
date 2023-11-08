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
    /// Tests that loaded config is identical to the original config.
    /// </summary>
    [Test]
    public void TestSaveAndLoad()
    {
        var hotspots = new List<Hotspot> { new(
                id: 0,
                textFile: "example.txt",
                imgFile: "example.jpg",
                x: 1,
                y: 2,
                r: 3
            ) };
        var original = new Config(hotspots, ConfigLocation);

        original.SaveConfig();


        // Check read file is same as saved config.
        var recreated = Config.LoadConfig(ConfigLocation);


        Assert.That(original.Hotspots, Has.Count.EqualTo(recreated.Hotspots.Count));

        Assert.Multiple(() =>
        {
            Assert.That(recreated.ConfigLocation, Is.EqualTo("config_test.json"));
            Assert.That(original.Hotspots[0].X, Is.EqualTo(recreated.Hotspots[0].X));
            Assert.That(original.Hotspots[0].Y, Is.EqualTo(recreated.Hotspots[0].Y));
            Assert.That(original.Hotspots[0].R, Is.EqualTo(recreated.Hotspots[0].R));
            Assert.That(original.Hotspots[0].Id, Is.EqualTo(recreated.Hotspots[0].Id));
            Assert.That(original.Hotspots[0].TextFile, Is.EqualTo(recreated.Hotspots[0].TextFile));
            Assert.That(original.Hotspots[0].ImgFile, Is.EqualTo(recreated.Hotspots[0].ImgFile));
            Assert.That(original.Hotspots[0].VideoFile, Is.EqualTo(recreated.Hotspots[0].VideoFile));
        });
    }

    /// <summary>
    /// Checks that GetHotspot returns the correct hotspot.
    /// </summary>
    [Test]
    public void TestGetHotspot()
    {
        var config = new Config(
            hotspots: new List<Hotspot>
            {
                new(id: 1, textFile: "test.txt"),
                new(id: 2, textFile: "test2.txt", imgFile: "test.jpg")
            }
            );

        var hotspot1 = config.GetHotspot(1);
        Assert.That(hotspot1, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(hotspot1!.Id, Is.EqualTo(1));
            Assert.That(hotspot1.TextFile, Is.EqualTo("test.txt"));
            Assert.That(config.GetHotspot(3), Is.Null);
        });
    }
}
