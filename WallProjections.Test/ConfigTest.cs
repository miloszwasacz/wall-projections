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
    /// Tests that loaded config is identical to the original config.
    /// </summary>
    [Test]
    public void TestSaveAndLoad()
    {
        const string configLocation = "config_test.json";
        var hotspots = new List<Hotspot> { new(
                id: 0,
                textFile: "example.txt",
                imgFile: "example.jpg",
                x: 1,
                y: 2,
                r: 3
            ) };
        var original = new Config(hotspots, configLocation);

        var success = original.SaveConfig();
        // Check that file saves correctly.
        Assert.That(success, Is.EqualTo(0));


        // Check read file is same as saved config.
        var recreated = Config.LoadConfig(configLocation);


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
}
