using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class DisplayWindowTest
{
    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = new MockDisplayViewModel();
        vm.OnHotspotSelected(null, new IPythonHandler.HotspotSelectedArgs(1));
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();
        var content = displayWindow.FindDescendantOfType<TransitioningContentControl>();

        Assert.That(content, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));
            Assert.That(displayWindow.DataContext, Is.SameAs(vm));
            Assert.That(content!.Content, Is.SameAs(vm.ContentViewModel));
        });
    }

    [AvaloniaTest]
    public void FullscreenToggleTest()
    {
        var vm = new MockDisplayViewModel();
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));

        displayWindow.KeyPress(Key.F11, RawInputModifiers.None);
        Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.Normal));

        displayWindow.KeyPress(Key.F11, RawInputModifiers.None);
        Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));
    }

    [AvaloniaTest]
    public void QuitTest()
    {
        var navigator = new MockNavigator();
        var vm = new MockDisplayViewModel(navigator: navigator);
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        displayWindow.KeyPress(Key.Escape, RawInputModifiers.None);
        Assert.That(navigator.HasBeenShutDown, Is.True);
    }

    [AvaloniaTest]
    public void OpenEditorTest()
    {
        var navigator = new MockNavigator();
        var vm = new MockDisplayViewModel(navigator: navigator);
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        Assert.That(navigator.IsEditorOpen, Is.False);
        displayWindow.KeyPress(Key.E, RawInputModifiers.None);
        Assert.That(navigator.IsEditorOpen, Is.True);
    }

    [AvaloniaTheory]
    public void KeyPressTest(Key key)
    {
        Assume.That(key is not Key.F11);
        Assume.That(key is not Key.Escape);
        Assume.That(key is not Key.E);
        Assume.That(key is > Key.D9 or < Key.D0);

        var navigator = new MockNavigator();
        var vm = new MockDisplayViewModel(navigator: navigator);
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        displayWindow.KeyPress(key, RawInputModifiers.None);
        Assert.Multiple(() =>
        {
            Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));
            Assert.That(navigator.HasBeenShutDown, Is.False);
            Assert.That(navigator.IsEditorOpen, Is.False);
        });
    }

    [AvaloniaTest]
    [TestCase(Key.F11)]
    [TestCase(Key.Escape)]
    [TestCase(Key.E)]
    public void KeyPressInvalidViewModelTest(Key key)
    {
        var navigator = new MockNavigator();
        var displayWindow = new DisplayWindow
        {
            DataContext = navigator
        };
        displayWindow.Show();

        displayWindow.KeyPress(key, RawInputModifiers.None);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (key)
        {
            case Key.F11:
                // Toggling fullscreen should still work
                Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.Normal));
                break;
            case Key.Escape:
                Assert.That(navigator.HasBeenShutDown, Is.False);
                break;
            case Key.E:
                Assert.That(navigator.IsEditorOpen, Is.False);
                break;
            default:
                Assert.Pass();
                break;
        }
    }

    [AvaloniaTest]
    public void WindowClosedByUserTest()
    {
        var navigator = new MockNavigator();
        var vm = new MockDisplayViewModel(navigator: navigator);
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        var args = CreateWindowClosingEventArgs();
        Assert.That(args.Cancel, Is.False);

        displayWindow.Window_OnClosing(displayWindow, args);

        Assert.Multiple(() =>
        {
            Assert.That(args.Cancel, Is.True);
            Assert.That(navigator.HasBeenShutDown, Is.True);
        });
    }

    [AvaloniaTest]
    public void WindowClosedByNavigatorTest()
    {
        var navigator = new MockNavigator();
        var vm = new MockDisplayViewModel(navigator: navigator);
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        displayWindow.Close();

        Assert.That(navigator.HasBeenShutDown, Is.False);
    }

    [AvaloniaTest]
    public void WindowClosedInvalidViewModelTest()
    {
        var navigator = new MockNavigator();
        var displayWindow = new DisplayWindow
        {
            DataContext = navigator
        };
        displayWindow.Show();

        var args = CreateWindowClosingEventArgs();
        Assert.That(args.Cancel, Is.False);

        displayWindow.Window_OnClosing(displayWindow, args);

        Assert.Multiple(() =>
        {
            Assert.That(args.Cancel, Is.False);
            Assert.That(navigator.HasBeenShutDown, Is.False);
        });
    }

    /// <summary>
    /// Creates a new instance of <see cref="WindowClosingEventArgs" />
    /// using reflection to bypass the internal constructor.
    /// </summary>
    /// <seealso cref="WindowClosingEventArgs(WindowCloseReason, bool)" />
    private static WindowClosingEventArgs CreateWindowClosingEventArgs(
        WindowCloseReason reason = WindowCloseReason.WindowClosing,
        bool isProgrammatic = false
    )
    {
        var ctor = typeof(WindowClosingEventArgs).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(WindowCloseReason), typeof(bool) },
            null
        );
        var instance = ctor?.Invoke(new object[] { reason, isProgrammatic }) as WindowClosingEventArgs;

        return instance ?? throw new MissingMethodException("Could not create WindowClosingEventArgs instance");
    }
}
