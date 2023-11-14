using Avalonia.Headless.NUnit;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;

namespace WallProjections.Test.Views;

[TestFixture]
public class DisplayViewTest
{
    private const string ArtifactId = "1";
    private static readonly string[] Files = { ArtifactId + ".txt", ArtifactId + ".mp4" };
    private static readonly MockViewModelProvider VmProvider = new();
    private static readonly MockFileProvider FileProvider = new(Files);

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = VmProvider.GetDisplayViewModel(ArtifactId, FileProvider);
        var displayView = new WallProjections.Views.DisplayView
        {
            DataContext = vm
        };

        Assert.That(displayView.Description.Text, Is.EqualTo(vm.Description));
        Assert.That(displayView.VideoView.DataContext, Is.Not.Null);
        Assert.That(displayView.VideoView.DataContext, Is.SameAs(vm.VideoViewModel));
    }
}
