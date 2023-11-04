using WallProjections.Configuration;

namespace WallProjections.Test;

[TestFixture]
[Author("Thomas Parr")]
public class ConfigTests
{
    [Test]
    public void Test1()
    {
        string configLocation = "config_test.json";
        List<Hotspot> hotspots = new List<Hotspot> { new Hotspot(1, 0, 2) };
        Config original = new Config(hotspots, configLocation);

        int success = original.SaveConfig();
        // Check that file saves correctly.
        Assert.That(success == 0);


        // Check read file is same as saved config.
        Config recreated = Config.LoadConfig(configLocation);

        Assert.That(recreated.ConfigLocation, Is.EqualTo("config_test.json"));
        Assert.That(original.Hotspots, Has.Count.EqualTo(recreated.Hotspots.Count));
        Assert.Multiple(() =>
        {
            Assert.That(original.Hotspots[0].X, Is.EqualTo(recreated.Hotspots[0].X));
            Assert.That(original.Hotspots[0].Y, Is.EqualTo(recreated.Hotspots[0].Y));
            Assert.That(original.Hotspots[0].R, Is.EqualTo(recreated.Hotspots[0].R));
        });
    }
}
