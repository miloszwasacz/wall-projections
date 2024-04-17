using System.Collections.Immutable;
using Avalonia;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using static WallProjections.Test.TestExtensions;
using HotspotSelectedArgs = WallProjections.Helper.Interfaces.IHotspotHandler.HotspotArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class PythonHandlerTest
{
    private const int CameraIndex = 700;

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
    public void ConstructorTest()
    {
        var (handler, _) = CreateInstance();
        Assert.That(handler.CameraIndex, Is.EqualTo(CameraIndex));
        handler.Dispose();
    }

    [Test]
    [Timeout(2000)]
    public async Task RunHotspotDetectionTest()
    {
        var (handler, python) = CreateInstance();
        await handler.RunHotspotDetection(TestConfig);
        Assert.That(python.IsHotspotDetectionRunning, Is.True);
        handler.Dispose();
    }

    [AvaloniaTest]
    [Timeout(2000)]
    public async Task RunCalibrationTest()
    {
        var (handler, python) = CreateInstance();
        await handler.RunCalibration(ImmutableDictionary.Create<int, Point>());
        Assert.That(python.IsCameraCalibrated, Is.True);
        handler.Dispose();
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
        handler.Dispose();
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
        handler.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(AsyncTaskTestCases))]
    [Timeout(2000)]
    public void PythonActionExceptionTest(Func<IPythonHandler, Task> asyncAction)
    {
        var (handler, python) = CreateInstance();
        python.Exception = new Exception("Test exception");
        Assert.ThrowsAsync<Exception>(() => asyncAction(handler));
        handler.Dispose();
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
        handler.Dispose();
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
        handler.Dispose();
    }

    /// <summary>
    /// Creates a new instance of <see cref="PythonHandler" /> and a mock python proxy
    /// </summary>
    private static (PythonHandler handler, MockPythonProxy python) CreateInstance()
    {
        var python = new MockPythonProxy();
        var handler = new PythonHandler(CameraIndex, python, new MockLoggerFactory());
        return (handler, python);
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
