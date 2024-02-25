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
        await handler.RunHotspotDetection();
        Assert.That(python.IsHotspotDetectionRunning, Is.True);
    }

    [Test]
    [Timeout(2000)]
    public async Task RunCalibrationTest()
    {
        var (handler, python) = CreateInstance();
        await handler.RunCalibration();
        Assert.That(python.IsCameraCalibrated, Is.True);
    }

    [Test]
    [Timeout(5000)]
    public async Task CancelCurrentTaskTest()
    {
        var (handler, python) = CreateInstance();
        python.Delay = 2000;
        var task = handler.RunHotspotDetection();
        await Task.Delay(100);
        python.Delay = 0;
        handler.CancelCurrentTask();
        await Task.Delay(100);
        python.Exception = new Exception("Test exception");

        // If the task finishes execution, any exceptions are caught
        // If it doesn't, there should be no side effects
        AwaitCancelledTask(
            task,
            () => Assert.That(python.IsHotspotDetectionRunning, Is.True),
            () => Assert.That(python.IsHotspotDetectionRunning, Is.False)
        );
    }

    /// <summary>
    /// Tests whether a task is cancelled when another task is started
    /// </summary>
    [Test]
    [Timeout(5000)]
    public async Task RunNewPythonActionTest()
    {
        var (handler, python) = CreateInstance();
        python.Delay = 2000;
        var task = handler.RunHotspotDetection();
        await Task.Delay(100);
        python.Delay = 0;
        await handler.RunCalibration();
        python.Exception = new Exception("Test exception");

        // If the task finishes execution, any exceptions are caught
        // If it doesn't, there should be no side effects
        AwaitCancelledTask(
            task,
            () => Assert.Multiple(() =>
            {
                Assert.That(python.IsHotspotDetectionRunning, Is.True);
                Assert.That(python.IsCameraCalibrated, Is.True);
            }),
            () => Assert.Multiple(() =>
            {
                Assert.That(python.IsHotspotDetectionRunning, Is.False);
                Assert.That(python.IsCameraCalibrated, Is.True);
            })
        );
    }

    [Test]
    public void PythonActionExceptionTest()
    {
        var (handler, python) = CreateInstance();
        python.Exception = new Exception("Test exception");
        Assert.ThrowsAsync<Exception>(() => handler.RunHotspotDetection());
    }

    [Test]
    [TestCaseSource(nameof(Ids))]
    public void OnPressDetectedTest(int id)
    {
        var id2 = id + 1;
        var (handler, _) = CreateInstance();
        HotspotSelectedArgs? eventFiredArgs = null;
        handler.HotspotSelected += (_, a) => eventFiredArgs = a;

        handler.OnPressDetected(id);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id));
        eventFiredArgs = null;

        handler.OnPressDetected(id2);
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
    /// Waits for the <paramref name="task" /> to finish execution, catching any <see cref="TaskCanceledException" />
    /// </summary>
    /// <param name="task">The task to wait for</param>
    /// <param name="onFinished">Action to execute if the task finishes successfully</param>
    /// <param name="onCancelled">Action to execute if the task hasn't finished before being cancelled</param>
    private static void AwaitCancelledTask(Task task, Action? onFinished, Action? onCancelled)
    {
        try
        {
            task.Wait();
            onFinished?.Invoke();
        }
        catch (AggregateException e)
        {
            if (e.InnerException is not TaskCanceledException) throw;

            onCancelled?.Invoke();
        }
    }
}
