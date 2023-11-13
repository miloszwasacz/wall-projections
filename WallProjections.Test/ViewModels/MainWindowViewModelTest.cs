using WallProjections.Test.Mocks;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class MainWindowViewModelTest
{
    private static readonly string[] ArtifactIds = { "1", "2" };

    [Test]
    public void CreationTest()
    {
        var viewModelProvider = new MockViewModelProvider();
        var mainWindowViewModel = new MainWindowViewModel(viewModelProvider);

        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Null);
    }

    [Test]
    public void CreateDisplayViewModelTest()
    {
        var viewModelProvider = new MockViewModelProvider();
        var mainWindowViewModel = new MainWindowViewModel(viewModelProvider);

        // Create a new DisplayViewModel
        mainWindowViewModel.CreateDisplayViewModel(ArtifactIds[0]);
        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Not.Null);
        Assert.That(mainWindowViewModel.DisplayViewModel?.Description,
            Is.EqualTo(MockDisplayViewModel.DefaultDescription + ArtifactIds[0]));

        // Change the DisplayViewModel
        mainWindowViewModel.CreateDisplayViewModel(ArtifactIds[1]);
        Assert.That(mainWindowViewModel.DisplayViewModel, Is.Not.Null);
        Assert.That(mainWindowViewModel.DisplayViewModel?.Description,
            Is.EqualTo(MockDisplayViewModel.DefaultDescription + ArtifactIds[1]));
    }
}
