using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Threading;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views.Display;
using WallProjections.Views.Editor;
using WallProjections.Views.SecondaryScreens;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class NavigatorTest
{
    [AvaloniaTest]
    [NonParallelizable]
    public async Task ConstructorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);

        await FlushUIThread(100);
        lifetime.AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>();
        Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task ConstructorNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);

        await FlushUIThread(100);
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>();
        Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task OpenEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());

        navigator.OpenEditor();

        await FlushUIThread();
        await Task.Delay(400);
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>();
        Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task CloseEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        navigator.OpenEditor();
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();

        await FlushUIThread();
        lifetime.AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>();
        Assert.Multiple(() =>
        {
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
            Assert.That(lifetime.Shutdowns, Is.Empty);
            Assert.That(vmProvider.HasBeenDisposed, Is.False);
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task CloseEditorNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();

        await FlushUIThread();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { 0 }));
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Null);
            Assert.That(pythonHandler.IsDisposed, Is.False);
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task ShowHideCalibrationMarkersTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>();

        navigator.ShowCalibrationMarkers();
        await FlushUIThread();
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsArUcoGridViewModel>();

        navigator.HideCalibrationMarkers();
        await FlushUIThread();
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task GetArUcoPositionsTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        navigator.ShowCalibrationMarkers();
        await FlushUIThread();
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsArUcoGridViewModel>();

        var positions = navigator.GetArUcoPositions();
        Assert.That(positions, Is.Not.Null.And.Not.Empty);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task ShutdownTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.Shutdown();
        await FlushUIThread();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { 0 }));
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Null);
            Assert.That(pythonHandler.IsDisposed, Is.False);
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoOptimization)] // Prevents the `navigator` from being optimized out
    public async Task ShutdownFromAppLifetimeTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        lifetime.Shutdown();
        await FlushUIThread();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { 0 }));
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Null);
            Assert.That(pythonHandler.IsDisposed, Is.False);
        });

        // Statement to prevent Garbage Collection of the navigator before the end of the test
        _ = navigator;
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task DisposeTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        await FlushUIThread(100);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());

        navigator.Dispose();
        await FlushUIThread();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Null);
            Assert.That(pythonHandler.IsDisposed, Is.False);
        });
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Runs jobs on the UI thread and waits for them to finish.
    /// </summary>
    /// <param name="delay">The delay in milliseconds.</param>
    private static async Task FlushUIThread(int delay = 1000)
    {
        Dispatcher.UIThread.RunJobs();
        await Task.Delay(delay);
        Dispatcher.UIThread.RunJobs();
    }

    [TestFixture]
    private class WindowExtensionsTest
    {
        [AvaloniaTest]
        public async Task CloseAndDisposeTest()
        {
            var dataContext = new MockDataContext();
            var closed = false;
            var window = new Window
            {
                DataContext = dataContext
            };
            window.Closed += (_, _) => closed = true;
            Assert.That(window.ShowInTaskbar, Is.True);

            window.CloseAndDispose();

            await Task.Delay(10);
            Assert.That(window.ShowInTaskbar, Is.False);

            await Task.Delay(300);
            Assert.Multiple(() =>
            {
                Assert.That(dataContext.HasBeenDisposed, Is.True);
                Assert.That(closed, Is.True);
            });
        }

        /// <summary>
        /// A mock <see cref="IDisposable" /> object.
        /// </summary>
        private class MockDataContext : IDisposable
        {
            public bool HasBeenDisposed { get; private set; }

            public void Dispose()
            {
                HasBeenDisposed = true;
            }
        }
    }
}

/// <summary>
/// Extension methods for <see cref="NavigatorTest" />s.
/// </summary>
internal static class NavigatorAssertions
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Asserts that <paramref name="lifetime" />'s <see cref="AppLifetime.MainWindow" />
    /// has the correct type and data context, and that the secondary window's data context is of correct type.
    /// </summary>
    /// <param name="lifetime">The <see cref="AppLifetime" /> to check.</param>
    /// <typeparam name="TMain">The expected type of the main window.</typeparam>
    /// <typeparam name="TMainVM">The expected type of the main window's data context.</typeparam>
    /// <typeparam name="TSecondaryVM">The expected type of the secondary window's data context.</typeparam>
    public static void AssertOpenedWindows<TMain, TMainVM, TSecondaryVM>(this AppLifetime lifetime)
    {
        var mainWindow = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(mainWindow, Is.InstanceOf<TMain>());
            Assert.That(mainWindow?.DataContext, Is.InstanceOf<TMainVM>());
        });

        Assert.That(lifetime.Windows, Has.Count.EqualTo(2));
        Assert.That(lifetime.Windows, Has.One.Items.SameAs(mainWindow));

        var window = lifetime.Windows.OfType<SecondaryWindow>().FirstOrDefault();
        Assert.That(window, Is.Not.SameAs(mainWindow));

        Assert.That(window, Is.InstanceOf<SecondaryWindow>());
        Assert.That(window?.DataContext, Is.InstanceOf<ISecondaryWindowViewModel>());

        var secondaryVM = window?.DataContext as ISecondaryWindowViewModel;
        Assert.That(secondaryVM!.Content, Is.InstanceOf<TSecondaryVM>());
    }

    // ReSharper restore InconsistentNaming
}
