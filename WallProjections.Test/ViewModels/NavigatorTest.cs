using Avalonia.Controls;
using WallProjections.Models;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.Views;

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

        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<DisplayWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IDisplayViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        var ownedWindows = window!.OwnedWindows;
        Assert.That(ownedWindows, Has.Count.EqualTo(1));
        Assert.That(ownedWindows[0], Is.InstanceOf<HotspotDisplayWindow>());
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

        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<EditorWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IEditorViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void OpenEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());

        navigator.OpenEditor();

        var window = lifetime.MainWindow;
        Assert.Multiple(() =>
        {
            Assert.That(window, Is.InstanceOf<EditorWindow>());
            Assert.That(window?.DataContext, Is.InstanceOf<IEditorViewModel>());
            Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void CloseEditorTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());

        using var navigator = new Navigator(lifetime, pythonHandler, (_, _) => vmProvider, () => fileHandler);
        navigator.OpenEditor();
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();
        Assert.Multiple(() =>
        {
            Assert.That(lifetime.Shutdowns, Is.Empty);
            Assert.That(lifetime.MainWindow, Is.InstanceOf<DisplayWindow>());
            Assert.That(vmProvider.HasBeenDisposed, Is.False);
            Assert.That(pythonHandler.CurrentScript, Is.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
        });
        var ownedWindows = lifetime.MainWindow!.OwnedWindows;
        Assert.That(ownedWindows, Has.Count.EqualTo(1));
        Assert.That(ownedWindows[0], Is.InstanceOf<HotspotDisplayWindow>());
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
        Assert.That(lifetime.MainWindow, Is.InstanceOf<EditorWindow>());

        navigator.CloseEditor();
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
