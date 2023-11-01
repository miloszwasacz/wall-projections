using Avalonia.Controls;
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

        Assert.That(viewModel.Greeting, Is.EqualTo("Welcome to Avalonia!"));
    }

    [AvaloniaTest]
    public void Test2()
    {
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel()
        };
        mainWindow.Show();

        var textBlock = mainWindow.GetControl<TextBlock>("Greeting");
        Assert.That(textBlock.Text, Is.EqualTo("Welcome to Avalonia!"));
    }
}
