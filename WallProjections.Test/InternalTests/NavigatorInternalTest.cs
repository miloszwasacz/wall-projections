using System.Reflection;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.Views;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class NavigatorInternalTest
{
    /// <summary>
    /// Tests the scenario where the Display window is opened, but no configuration file is loaded.
    /// </summary>
    [AvaloniaTest]
    public void OpenDisplayNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(lifetime, _ => vmProvider, () => fileHandler);
        lifetime.MainWindow = null;
        Assert.That(lifetime.MainWindow, Is.Null);

        var openDisplayMethod =
            typeof(Navigator).GetMethod("OpenDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method not found");
        openDisplayMethod.Invoke(navigator, null);

        var window = lifetime.MainWindow;
        Assert.That(window, Is.InstanceOf<EditorWindow>());
        Assert.Multiple(() =>
        {
            Assert.That(window?.DataContext, Is.InstanceOf<IEditorViewModel>());
            Assert.That(lifetime.Shutdowns, Is.Empty);
        });
    }
}
