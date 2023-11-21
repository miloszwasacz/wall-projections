using WallProjections.Helper;
using HotspotSelectedArgs = WallProjections.Helper.Interfaces.IPythonEventHandler.HotspotSelectedArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class PythonEventHandlerTest
{
    private static readonly int[] Ids = { 0, 1, 2, 1000, -100 };

    [Test]
    public void SingletonPatternTest()
    {
        var instance1 = PythonEventHandler.Instance;
        var instance2 = PythonEventHandler.Instance;

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
    /// Uses reflection to get the private constructor of <see cref="PythonEventHandler" />,
    /// so that the global instance is not used
    /// </summary>
    /// <returns>A new instance of <see cref="PythonEventHandler" /></returns>
    /// <exception cref="InvalidCastException">When the object cannot be instantiated as <see cref="PythonEventHandler" /></exception>
    private static PythonEventHandler CreateInstance()
    {
        var type = typeof(PythonEventHandler);
        var res = Activator.CreateInstance(type, true) as PythonEventHandler;
        return res ?? throw new InvalidCastException("Could construct PythonEventHandler");
    }
}
