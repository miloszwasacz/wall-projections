using Avalonia.Controls;
using Avalonia.Threading;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views;
using AppLifetime = Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;

namespace WallProjections.Test.ViewModels;

[TestFixture]
[NonParallelizable]
public class NavigatorTest
{
    [AvaloniaTest]
    [NonParallelizable]
    public void ConstructorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);

        Dispatcher.UIThread.RunJobs();
        lifetime.AssertOpenedWindows<DisplayWindow, IDisplayViewModel, IHotspotDisplayViewModel>();
        Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void ConstructorNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);

        Dispatcher.UIThread.RunJobs();
        //TODO Change TSecondaryVM to the correct type
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, IHotspotDisplayViewModel>();
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

        Dispatcher.UIThread.RunJobs();
        Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());

        navigator.OpenEditor();

        Dispatcher.UIThread.RunJobs();
        await Task.Delay(400);
        //TODO Change TSecondaryVM to the correct type
        lifetime.AssertOpenedWindows<EditorWindow, IEditorViewModel, IHotspotDisplayViewModel>();
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
        Dispatcher.UIThread.RunJobs();
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();

        Dispatcher.UIThread.RunJobs();
        await Task.Delay(400);
        lifetime.AssertOpenedWindows<DisplayWindow, IDisplayViewModel, IHotspotDisplayViewModel>();
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
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        Dispatcher.UIThread.RunJobs();
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();
        Dispatcher.UIThread.RunJobs();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { 0 }));
            Assert.That(lifetime.MainWindow, Is.Null);
            Assert.That(vmProvider.HasBeenDisposed, Is.True);
            Assert.That(pythonHandler.CurrentScript, Is.Null);
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
            Assert.That(window.ShowInTaskbar, Is.True);

            window.CloseAndDispose();

            await Task.Delay(2);
            Assert.That(window.ShowInTaskbar, Is.False);

            await Task.Delay(250);
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

        //TODO Uncomment when SecondaryWindowViewModel is fully implemented
        // var secondaryVM = window?.DataContext as ISecondaryWindowViewModel;
        // Assert.That(secondaryVM!.Content, Is.InstanceOf<TSecondaryVM>());
    }

    // ReSharper restore InconsistentNaming
}
