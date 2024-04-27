using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Threading;
using WallProjections.Models;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views.Display;
using WallProjections.Views.Editor;
using WallProjections.Views.SecondaryScreens;
using static WallProjections.Test.ViewModels.NavigatorAssertions;

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
        await pythonHandler.RunHotspotDetection(null!);
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );

        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);
        Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task ConstructorNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        await pythonHandler.RunHotspotDetection(null!);
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new ConfigNotImportedException(null!));

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );

        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.ConfigNotFound }));
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void ConstructorExceptionTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new IOException());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );

        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.ConfigLoadError }));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void OpenEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );
        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);

        navigator.OpenEditor();

        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);
        Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void OpenEditorPythonExceptionTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler
        {
            Exception = new Exception("Test exception")
        };
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);

        navigator.OpenEditor();

        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.PythonError }));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CloseEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );
        navigator.OpenEditor();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        navigator.CloseEditor();

        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);
        Assert.Multiple(() =>
        {
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
            Assert.That(lifetime.Shutdowns, Is.Empty);
            Assert.That(vmProvider.HasBeenDisposed, Is.False);
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CloseEditorNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler1 = new MockFileHandler(new List<Hotspot.Media>());
        var fileHandler2 = new MockFileHandler(new ConfigNotImportedException(null!));
        var fileHandlerSwitch = false;

        var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () =>
            {
                var fileHandler = !fileHandlerSwitch
                    ? fileHandler1
                    : fileHandler2;

                fileHandlerSwitch = true;
                return fileHandler;
            },
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        navigator.OpenEditor();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        navigator.CloseEditor();

        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.ConfigLoadError }));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CloseEditorExceptionTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler1 = new MockFileHandler(new List<Hotspot.Media>());
        var fileHandler2 = new MockFileHandler(new IOException());
        var fileHandlerSwitch = false;

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () =>
            {
                var fileHandler = !fileHandlerSwitch
                    ? fileHandler1
                    : fileHandler2;

                fileHandlerSwitch = true;
                return fileHandler;
            },
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        navigator.OpenEditor();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        navigator.CloseEditor();

        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.ConfigLoadError }));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CloseEditorNotInitialExceptionTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler1 = new MockFileHandler(new ConfigNotImportedException(null!));
        var fileHandler2 = new MockFileHandler(new List<Hotspot.Media>());
        var fileHandlerSwitch = false;

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () =>
            {
                var fileHandler = !fileHandlerSwitch
                    ? fileHandler1
                    : fileHandler2;

                fileHandlerSwitch = !fileHandlerSwitch;
                return fileHandler;
            },
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.ConfigNotFound }));

        navigator.OpenEditor();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        navigator.CloseEditor();

        Assert.That(
            lifetime.Shutdowns,
            Is.EquivalentTo(new[] { (int)ExitCode.ConfigNotFound, (int)ExitCode.Success })
        );
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void ShowHideCalibrationMarkersTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );
        navigator.OpenEditor();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        navigator.ShowCalibrationMarkers();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsArUcoGridViewModel>(navigator);

        navigator.HideCalibrationMarkers();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void GetArUcoPositionsTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );
        navigator.OpenEditor();
        navigator.ShowCalibrationMarkers();
        Dispatcher.UIThread.RunJobs();
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsArUcoGridViewModel>(navigator);

        var positions = navigator.GetArUcoPositions();
        Assert.That(positions, Is.Not.Null.And.Not.Empty);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void ShutdownTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);

        navigator.Shutdown();
        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.Success }));
    }

    //TODO Use this test as a basis when adding tests for GlobalWindowManager
    [AvaloniaTest]
    [NonParallelizable]
    [Ignore("The Navigator no longer subscribes to the AppLifetime.OnExit")]
    [MethodImpl(MethodImplOptions.NoOptimization)] // Prevents the `navigator` from being optimized out
    public void ShutdownFromAppLifetimeTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            exitCode =>
            {
                // ReSharper disable once AccessToDisposedClosure
                lifetime.DryShutdown(exitCode);
            },
            new MockLoggerFactory()
        );
        AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);

        lifetime.Shutdown();
        Dispatcher.UIThread.RunJobs();
        // await Task.Delay(1000);
        Dispatcher.UIThread.RunJobs();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { (int)ExitCode.Success }));
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
    public void DisposeTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );
        AssertOpenedWindows<DisplayWindow, IDisplayViewModel, AbsHotspotDisplayViewModel>(navigator);

        navigator.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Not.Null);
            Assert.That(pythonHandler.IsDisposed, Is.False);
        });
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
            window.Show();
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
    /// Asserts that <paramref name="navigator" />'s main window has the correct type and data context,
    /// and that the secondary window's data context is of correct type.
    /// </summary>
    /// <param name="navigator">The <see cref="INavigator" /> to check.</param>
    /// <typeparam name="TMain">The expected type of the main window.</typeparam>
    /// <typeparam name="TMainVM">The expected type of the main window's data context.</typeparam>
    /// <typeparam name="TSecondaryVM">The expected type of the secondary window's data context.</typeparam>
    public static void AssertOpenedWindows<TMain, TMainVM, TSecondaryVM>(INavigator navigator)
    {
        var mainWindowProperty = typeof(Navigator)
            .GetProperty("MainWindow", BindingFlags.NonPublic | BindingFlags.Instance);
        var secondaryScreenField = typeof(Navigator)
            .GetField("_secondaryScreen", BindingFlags.NonPublic | BindingFlags.Instance);

        var mainWindow = (Window?)mainWindowProperty!.GetValue(navigator);
        Assert.Multiple(() =>
        {
            Assert.That(mainWindow, Is.InstanceOf<TMain>());
            Assert.That(mainWindow?.DataContext, Is.InstanceOf<TMainVM>());
        });

        var (secondaryWindow, secondaryViewModel) =
            ((Window, ISecondaryWindowViewModel))secondaryScreenField!.GetValue(navigator)!;

        Assert.That(secondaryWindow, Is.Not.SameAs(mainWindow));
        Assert.That(secondaryWindow, Is.InstanceOf<SecondaryWindow>());
        Assert.That(secondaryWindow.DataContext, Is.InstanceOf<ISecondaryWindowViewModel>());
        Assert.That(secondaryWindow.DataContext, Is.SameAs(secondaryViewModel));

        Assert.That(secondaryViewModel.Content, Is.InstanceOf<TSecondaryVM>());
    }

    // ReSharper restore InconsistentNaming
}
