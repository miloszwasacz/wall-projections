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
using WallProjections.Views;
using WallProjections.Views.EditorUserControls;

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
        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<DisplayWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IDisplayViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        Assert.That(lifetime.Windows, Has.Count.EqualTo(2));
        Assert.That(lifetime.Windows, Has.One.Items.SameAs(window));
        Assert.That(lifetime.Windows, Has.One.Items.InstanceOf<HotspotDisplayWindow>());
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
        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<EditorWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IEditorViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        Assert.That(lifetime.Windows, Has.Count.EqualTo(2));
        Assert.That(lifetime.Windows, Has.One.Items.SameAs(window));
        Assert.That(lifetime.Windows, Has.One.Items.InstanceOf<PositionEditorWindow>());
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
        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<EditorWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IEditorViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        Assert.That(lifetime.Windows, Has.Count.EqualTo(2));
        Assert.That(lifetime.Windows, Has.One.Items.SameAs(window));
        Assert.That(lifetime.Windows, Has.One.Items.InstanceOf<PositionEditorWindow>());
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
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.Empty);
            Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());
            Assert.That(vmProvider.HasBeenDisposed, Is.False);
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        Assert.That(lifetime.Windows, Has.Count.EqualTo(2));
        Assert.That(lifetime.Windows, Has.One.Items.InstanceOf<DisplayWindow>());
        Assert.That(lifetime.Windows, Has.One.Items.InstanceOf<HotspotDisplayWindow>());
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
