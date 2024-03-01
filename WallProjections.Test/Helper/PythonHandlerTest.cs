﻿using System.Reflection;
using NUnit.Framework.Constraints;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.Helper;
using static WallProjections.Test.TestSetup;
using HotspotSelectedArgs = WallProjections.Helper.Interfaces.IPythonHandler.HotspotSelectedArgs;

namespace WallProjections.Test.Helper;

[TestFixture]
public class PythonHandlerTest
{
    /// <summary>
    /// A constraint for <see cref="TaskCanceledException" /> or
    /// <see cref="AggregateException" /> with <see cref="TaskCanceledException" /> as inner exception
    /// </summary>
    private static readonly InstanceOfTypeConstraint IsTaskCanceledException = Is
        .InstanceOf<TaskCanceledException>().Or
        .InstanceOf<AggregateException>().With.InnerException.InstanceOf<TaskCanceledException>();

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
        await handler.RunCalibration(new Dictionary<int, (float, float)>());
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
        Assert.ThrowsAsync(IsTaskCanceledException, async () => await task);
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
        await handler.RunCalibration(new Dictionary<int, (float, float)>());
        python.Exception = new Exception("Test exception");
        // Assert.That(async () => { await task; }, ThrowsTaskCanceledException);
        Assert.ThrowsAsync(IsTaskCanceledException, async () => await task);
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

        handler.OnHotspotPressed(id);
        Assert.That(eventFiredArgs, Is.InstanceOf<HotspotSelectedArgs>());
        Assert.That(eventFiredArgs!.Id, Is.EqualTo(id));
        eventFiredArgs = null;

        handler.OnHotspotPressed(id2);
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
}
