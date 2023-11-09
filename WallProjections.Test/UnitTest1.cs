using Avalonia.Headless.NUnit;
using WallProjections.ViewModels;
using WallProjections.Views;

namespace WallProjections.Test;

public class Tests
{
    [Test]
    public void Test1()
    {
        MainWindowViewModel viewModel = new();

        Assert.IsNotNull(viewModel);
    }

    [AvaloniaTest]
    public void Test2()
    {
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel()
        };
        mainWindow.Show();

        Assert.That(mainWindow.DataContext, Is.InstanceOf(typeof(MainWindowViewModel)));
    }
}
