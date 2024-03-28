using System.Collections.Immutable;
using System.Reflection;
using Avalonia;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;
using static WallProjections.Test.TestSetup;
using static WallProjections.Test.TestExtensions;
using HotspotSelectedArgs = WallProjections.Helper.Interfaces.IHotspotHandler.HotspotArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class PythonHandlerTest
{
    /// <summary>
    /// An empty configuration for testing (the mocks ignore it)
    /// </summary>
    private static readonly IConfig TestConfig = new Config(new double[,] { }, Enumerable.Empty<Hotspot>());

    /// <summary>
    /// Test ids of hotspots
    /// </summary>
    private static readonly int[] Ids = { 0, 1, 2, 1000, -100 };

    /// <summary>
    /// Different types of actions that can be performed on the <see cref="IPythonHandler" />
    /// </summary>
    private static IEnumerable<TestCaseData<Func<IPythonHandler, Task>>> AsyncTaskTestCases()
    {
        yield return MakeTestData(
            (IPythonHandler h) => h.RunHotspotDetection(TestConfig),
            nameof(IPythonHandler.RunHotspotDetection)
        );
        yield return MakeTestData(
            async (IPythonHandler h) => { await h.RunCalibration(ImmutableDictionary.Create<int, Point>()); },
            nameof(IPythonHandler.RunCalibration)
        );
    }

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
    [NonParallelizable]
    public void InitializeTest()
    {
        // Clear the global instance
        var field = typeof(PythonHandler).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
                    ?? throw new MissingFieldException("Could not find the `_instance` field");
        field.SetValue(PythonHandler.Instance, null);

        // Usage of uninitialized global instance
        Assert.That(() => _ = PythonHandler.Instance, Throws.TypeOf<TypeInitializationException>());

        // Initialization of the global instance
        var initialized = PythonHandler.Initialize(PythonRuntime);
        var singleton = PythonHandler.Instance;
        Assert.Multiple(() =>
        {
            Assert.That(initialized, Is.SameAs(singleton));
            Assert.That(initialized, Is.InstanceOf<IPythonHandler>());
            Assert.That(PythonRuntime.IsDisposed, Is.False);
        });

        // Re-initialization of the global instance
        Assert.That(() => PythonHandler.Initialize(PythonRuntime), Throws.InvalidOperationException);
    }

    [Test]
    [Timeout(2000)]
    public async Task RunHotspotDetectionTest()
    {
        var (handler, python) = CreateInstance();
        await handler.RunHotspotDetection(TestConfig);
        Assert.That(python.IsHotspotDetectionRunning, Is.True);
    }

    [AvaloniaTest]
    [Timeout(2000)]
    public async Task RunCalibrationTest()
    {
        var (handler, python) = CreateInstance();
        await handler.RunCalibration(ImmutableDictionary.Create<int, Point>());
        Assert.That(python.IsCameraCalibrated, Is.True);
    }

    [Test]
    [TestCaseSource(nameof(AsyncTaskTestCases))]
    [Timeout(5000)]
    public async Task CancelCurrentTaskTest(Func<IPythonHandler, Task> asyncAction)
    {
        var (handler, python) = CreateInstance();
        python.Delay = 2000;
        var task = asyncAction(handler);
        await Task.Delay(100);
        python.Delay = 0;
        handler.CancelCurrentTask();
        await Task.Delay(100);
        python.Exception = new Exception("Test exception");
        AssertThrowsTaskCanceledException(task);
    }

    /// <summary>
    /// Tests whether a task is cancelled when another task is started
    /// </summary>
    [AvaloniaTest]
    [Timeout(5000)]
    public async Task RunNewPythonActionTest()
    {
        var (handler, python) = CreateInstance();
        python.Delay = 2000;
        var task = handler.RunHotspotDetection(TestConfig);
        await Task.Delay(100);
        python.Delay = 0;
        await handler.RunCalibration(ImmutableDictionary.Create<int, Point>());
        python.Exception = new Exception("Test exception");
        AssertThrowsTaskCanceledException(task);
    }

    [Test]
    [TestCaseSource(nameof(AsyncTaskTestCases))]
    [Timeout(2000)]
    public void PythonActionExceptionTest(Func<IPythonHandler, Task> asyncAction)
    {
        var (handler, python) = CreateInstance();
        python.Exception = new Exception("Test exception");
        Assert.ThrowsAsync<Exception>(() => asyncAction(handler));
    }

    [Test]
    [TestCaseSource(nameof(Ids))]
    public void OnHotspotPressedTest(int id)
    {
        var id2 = id + 1;
        var (handler, _) = CreateInstance();
        HotspotSelectedArgs? eventFiredArgs = null;
        handler.HotspotPressed += (_, a) => eventFiredArgs = a;

        handler.OnHotspotPressed(id);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id));
        eventFiredArgs = null;

        handler.OnHotspotPressed(id2);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id2));
    }

    [Test]
    [TestCaseSource(nameof(Ids))]
    public void OnHotspotUnpressedTest(int id)
    {
        var id2 = id + 1;
        var (handler, _) = CreateInstance();
        HotspotSelectedArgs? eventFiredArgs = null;
        handler.HotspotReleased += (_, a) => eventFiredArgs = a;

        handler.OnHotspotUnpressed(id);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id));
        eventFiredArgs = null;

        handler.OnHotspotUnpressed(id2);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
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

    /// <summary>
    /// Asserts that either a <see cref="TaskCanceledException" /> or <see cref="AggregateException" />
    /// with <see cref="TaskCanceledException" /> as inner exception has been thrown
    /// </summary>
    private static void AssertThrowsTaskCanceledException(Task task)
    {
        try
        {
            task.Wait();
            Assert.Fail("Task did not throw TaskCanceledException");
        }
        catch (TaskCanceledException)
        {
            // Expected
            Assert.Pass();
        }
        catch (AggregateException e) when (e.InnerException is TaskCanceledException)
        {
            // Expected
            Assert.Pass();
        }
    }
}
