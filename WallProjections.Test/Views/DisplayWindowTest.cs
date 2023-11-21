using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using WallProjections.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
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
            vm.OnHotspotSelected(null, new PythonEventHandler.HotspotSelectedArgs(id));
            Assert.Multiple(() =>
            {
                Assert.That(vm.CurrentHotspotId, Is.EqualTo(id));
                Assert.That(displayWindow.Description.Text, Is.EqualTo(vm.Description));
            });
        }
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

        displayWindow.OnKeyDown(null, new KeyEventArgs { Key = Key.F11 });
        Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.Normal));

        displayWindow.OnKeyDown(null, new KeyEventArgs { Key = Key.F11 });
        Assert.That(displayWindow.WindowState, Is.EqualTo(WindowState.FullScreen));

        vm.Dispose();
    }

    [AvaloniaTest]
    public void QuitTest()
    {
        var lifetime = new MockDesktopLifetime();
        Application.Current!.ApplicationLifetime = lifetime;
        var vm = new MockDisplayViewModel();
        var displayWindow = new DisplayWindow
        {
            DataContext = vm
        };
        displayWindow.Show();

        displayWindow.OnKeyDown(null, new KeyEventArgs { Key = Key.Escape });
        Assert.That(lifetime.Shutdowns, Is.EquivalentTo(new[] { 0 }));
    }
}
