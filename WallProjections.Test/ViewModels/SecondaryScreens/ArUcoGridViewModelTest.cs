using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels.SecondaryScreens;

[TestFixture]
public class ArUcoGridViewModelTest
{
    [AvaloniaTest]
    public void ConstructorTest()
    {
        var viewModel = new ArUcoGridViewModel();
        Assert.That(viewModel.ArUcoList, Is.Not.Empty);
    }
}
