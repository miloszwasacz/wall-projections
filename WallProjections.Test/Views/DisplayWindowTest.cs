using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using WallProjections.Helper.Interfaces;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class DisplayWindowTest
{
    private static readonly int[] Ids = { 0, 1, 2, 100, -1000 };

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = new MockDisplayViewModel();
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };

        Assert.Multiple(() =>
        {
            Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));
            Assert.That(displayWindow.Description.Text, Is.EqualTo(vm.Description));
            Assert.That(displayWindow.VideoView.DataContext, Is.Not.Null);
            Assert.That(displayWindow.VideoView.DataContext, Is.SameAs(vm.VideoViewModel));
        });
    }

    [AvaloniaTest]
    public void DescriptionChangedTest()
    {
        var vm = new MockDisplayViewModel();
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };

        Assert.That(displayWindow.Description.Text, Is.EqualTo(vm.Description));

        foreach (var id in Ids)
        {
            vm.OnHotspotSelected(null, new IPythonEventHandler.HotspotSelectedArgs(id));
            Assert.Multiple(() =>
            {
                Assert.That(vm.CurrentHotspotId, Is.EqualTo(id));
                Assert.That(displayWindow.Description.Text, Is.EqualTo(vm.Description));
            });
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
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
}
