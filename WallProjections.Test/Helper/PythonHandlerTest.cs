using WallProjections.Helper;
using HotspotSelectedArgs = WallProjections.Helper.Interfaces.IPythonHandler.HotspotSelectedArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class PythonHandlerTest
{
    private static readonly int[] Ids = { 0, 1, 2, 1000, -100 };

    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = PythonHandler.Instance;
        var instance2 = PythonHandler.Instance;

        Assert.That(instance2, Is.SameAs(instance1));
    }

    [Test]
    [TestCaseSource(nameof(Ids))]
    public void InstanceTest(int id)
    {
        var handler = CreateInstance();
        HotspotSelectedArgs? eventFiredArgs = null;
        handler.HotspotSelected += (_, a) => eventFiredArgs = a;


        handler.OnPressDetected(id);
        Assert.That(eventFiredArgs, Is.Not.Null);
        Assert.That(eventFiredArgs!, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id));

        var id2 = id + 1;
        handler.OnPressDetected(id2);
        Assert.That(eventFiredArgs, Is.Not.Null);
        Assert.That(eventFiredArgs!, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id2));
    }

    /// <summary>
    /// Uses reflection to get the private constructor of <see cref="PythonHandler" />,
    /// so that the global instance is not used
    /// </summary>
    /// <returns>A new instance of <see cref="PythonHandler" /></returns>
    /// <exception cref="InvalidCastException">When the object cannot be instantiated as <see cref="PythonHandler" /></exception>
    private static PythonHandler CreateInstance()
    {
        var type = typeof(PythonHandler);
        var res = Activator.CreateInstance(type, true) as PythonHandler;
        return res ?? throw new InvalidCastException("Could not construct PythonEventHandler");
    }
}
