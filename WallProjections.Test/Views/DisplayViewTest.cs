using Avalonia.Headless.NUnit;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Views;

namespace WallProjections.Test.Views;

[TestFixture]
public class DisplayViewTest
{
    private static readonly MockViewModelProvider VmProvider = new();

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = VmProvider.GetDisplayViewModel();
        var displayView = new DisplayWindow
        {
            DataContext = vm
        };

        Assert.Multiple(() =>
        {
            Assert.That(displayView.Description.Text, Is.EqualTo(vm.Description));
            Assert.That(displayView.VideoView.DataContext, Is.Not.Null);
            Assert.That(displayView.VideoView.DataContext, Is.SameAs(vm.VideoViewModel));
        });
    }
}
