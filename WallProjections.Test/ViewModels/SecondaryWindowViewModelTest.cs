using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class SecondaryWindowViewModelTest
{
    private static IConfig CreateConfig() => new Config(new double[0, 0], Enumerable.Empty<Hotspot>());

    [AvaloniaTest]
    public void ConstructorTest()
    {
        var vmProvider = new MockViewModelProvider();
        var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());
        Assert.That(viewModel.Content, Is.Null);
    }

    [AvaloniaTest]
    public void ShowHotspotDisplayTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());
        viewModel.ShowHotspotDisplay(CreateConfig());
        Assert.That(viewModel.Content, Is.InstanceOf<AbsHotspotDisplayViewModel>());
    }

    [AvaloniaTest]
    public void ShowPositionEditorTest()
    {
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
        using var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());
        var editorViewModel = new MockEditorViewModel(vmProvider, fileHandler);
        viewModel.ShowPositionEditor(editorViewModel);
        Assert.That(viewModel.Content, Is.InstanceOf<AbsPositionEditorViewModel>());
        Assert.That(viewModel.Content, Is.SameAs(editorViewModel.PositionEditor));
    }

    [AvaloniaTest]
    public void ShowArUcoGridTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());
        viewModel.ShowArUcoGrid();
        Assert.That(viewModel.Content, Is.InstanceOf<AbsArUcoGridViewModel>());
    }

    [AvaloniaTest]
    public void ChangeContentTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());

        viewModel.ShowHotspotDisplay(CreateConfig());
        var hotspotDisplay = viewModel.Content as MockHotspotDisplayViewModel;
        Assert.That(viewModel.Content, Is.InstanceOf<AbsHotspotDisplayViewModel>());

        viewModel.ShowArUcoGrid();
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Content, Is.InstanceOf<AbsArUcoGridViewModel>());
            Assert.That(hotspotDisplay!.IsDisposed, Is.True);
        });
    }

    [AvaloniaTest]
    public void DisposeTest()
    {
        var vmProvider = new MockViewModelProvider();
        var viewModel = new SecondaryWindowViewModel(vmProvider, new MockLoggerFactory());

        viewModel.ShowHotspotDisplay(CreateConfig());
        var hotspotDisplay = viewModel.Content as MockHotspotDisplayViewModel;
        Assert.That(viewModel.Content, Is.InstanceOf<AbsHotspotDisplayViewModel>());

        viewModel.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Content, Is.Null);
            Assert.That(hotspotDisplay!.IsDisposed, Is.True);
        });
    }
}
