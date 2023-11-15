using Avalonia.Headless.NUnit;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;

namespace WallProjections.Test.Views;

[TestFixture]
public class MainWindowTest
{
    private const string ArtifactId = "1";
    private static readonly string[] Files = { ArtifactId + ".txt" };
    private static readonly MockViewModelProvider VmProvider = new();
    private static readonly MockFileProvider FileProvider = new(Files);

    [AvaloniaTest]
    public void ViewModelTest()
    {
        var vm = VmProvider.GetMainWindowViewModel();
        var mainWindow = new WallProjections.Views.MainWindow
        {
            DataContext = vm
        };

        Assert.Multiple(() =>
        {
            Assert.That(mainWindow.DataContext, Is.SameAs(vm));
            Assert.That(mainWindow.DisplayView.DataContext, Is.Null);
        });
    }

    //TODO Rename this test once MainWindow has been refactored
    [AvaloniaTest]
    public void CreateDisplayViewModelTest()
    {
        var vm = VmProvider.GetMainWindowViewModel();
        var mainWindow = new WallProjections.Views.MainWindow
        {
            DataContext = vm
        };
        mainWindow.Show();
        vm.CreateDisplayViewModel(ArtifactId, FileProvider);


        Assert.Multiple(() =>
        {
            Assert.That(mainWindow.DisplayView.DataContext, Is.Not.Null);
            Assert.That(mainWindow.DisplayView.DataContext, Is.SameAs(vm.DisplayViewModel));
        });
    }

    //TODO Add more proper tests once MainWindow has been refactored
}
