using System.Reflection;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using static WallProjections.Test.TestSetup;
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

        Assert.Multiple(() =>
        {
            Assert.That(instance2, Is.SameAs(instance1));
            Assert.That(PythonRuntime.IsDisposed, Is.False);
        });
    }

    [Test]
    [TestCaseSource(nameof(Ids))]
    public void OnPressDetectedTest(int id)
    {
        var (handler, _) = CreateInstance();
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
    /// <returns>
    /// A new instance of <see cref="PythonHandler" /> and a <see cref="MockPythonProxy" /> injected into the constructor
    /// </returns>
    /// <exception cref="InvalidCastException">When the object cannot be instantiated as <see cref="PythonHandler" /></exception>
    private static (PythonHandler handler, MockPythonProxy python) CreateInstance()
    {
        var python = new MockPythonProxy();
        var ctor = typeof(PythonHandler).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(IPythonProxy) },
            null
        );
        var instance = ctor?.Invoke(new object[] { python }) as PythonHandler
                       ?? throw new MissingMethodException("Could not construct PythonEventHandler");

        return (instance, python);
    }
}
