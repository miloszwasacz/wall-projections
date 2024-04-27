using System.Reflection;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.Views;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views.Editor;
using static WallProjections.Test.ViewModels.NavigatorAssertions;

namespace WallProjections.Test.InternalTests;

[TestFixture]
public class NavigatorInternalTest
{
    /// <summary>
    /// Tests the scenario where the Display window is opened, but no configuration file is loaded.
    /// </summary>
    [AvaloniaTest]
    [NonParallelizable]
    public void OpenDisplayNoConfigTest()
    {
        using var lifetime = new MockDesktopLifetime();
        var pythonHandler = new MockPythonHandler();
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new FileNotFoundException());

        using var navigator = new Navigator(
            lifetime,
            pythonHandler,
            (_, _) => vmProvider,
            () => fileHandler,
            _ => { },
            new MockLoggerFactory()
        );

        var openDisplayMethod =
            typeof(Navigator).GetMethod("OpenDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method not found");
        openDisplayMethod.Invoke(navigator, null);

        Assert.Multiple(() =>
        {
            AssertOpenedWindows<EditorWindow, IEditorViewModel, AbsPositionEditorViewModel>(navigator);
            Assert.That(pythonHandler.CurrentScript, Is.Not.EqualTo(MockPythonHandler.PythonScript.HotspotDetection));
            Assert.That(lifetime.Shutdowns, Is.Empty);
        });
    }
}
