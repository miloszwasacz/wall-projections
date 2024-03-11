using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class SecondaryWindowViewModelTest
{
    private static IConfig CreateConfig() => new Config(new double[0, 0], Enumerable.Empty<Hotspot>());

    [AvaloniaTest]
    public void ConstructorTest()
    {
        var vmProvider = new MockViewModelProvider();
        var viewModel = new SecondaryWindowViewModel(vmProvider);
        Assert.That(viewModel.Content, Is.Null);
    }

    [AvaloniaTest]
    public void ShowHotspotDisplayTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider);
        viewModel.ShowHotspotDisplay(CreateConfig());
        Assert.That(viewModel.Content, Is.InstanceOf<IHotspotDisplayViewModel>());
    }

    [AvaloniaTest]
    public void ShowPositionEditorTest()
    {
        var vmProvider = new MockViewModelProvider();
        var fileHandler = new MockFileHandler(new List<Hotspot.Media>());
        using var viewModel = new SecondaryWindowViewModel(vmProvider);
        var editorViewModel = new MockEditorViewModel(vmProvider, fileHandler);
        viewModel.ShowPositionEditor(editorViewModel);
        Assert.That(viewModel.Content, Is.InstanceOf<IPositionEditorViewModel>());
        Assert.That(viewModel.Content, Is.SameAs(editorViewModel.PositionEditor));
    }

    [AvaloniaTest]
    public void ShowArUcoGridTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider);
        viewModel.ShowArUcoGrid();
        Assert.That(viewModel.Content, Is.InstanceOf<IArUcoGridViewModel>());
    }

    [AvaloniaTest]
    public void ChangeContentTest()
    {
        var vmProvider = new MockViewModelProvider();
        using var viewModel = new SecondaryWindowViewModel(vmProvider);

        viewModel.ShowHotspotDisplay(CreateConfig());
        var hotspotDisplay = viewModel.Content as MockHotspotDisplayViewModel;
        Assert.That(viewModel.Content, Is.InstanceOf<IHotspotDisplayViewModel>());

        viewModel.ShowArUcoGrid();
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Content, Is.InstanceOf<IArUcoGridViewModel>());
            Assert.That(hotspotDisplay!.IsDisposed, Is.True);
        });
    }

    [AvaloniaTest]
    public void DisposeTest()
    {
        var vmProvider = new MockViewModelProvider();
        var viewModel = new SecondaryWindowViewModel(vmProvider);

        viewModel.ShowHotspotDisplay(CreateConfig());
        var hotspotDisplay = viewModel.Content as MockHotspotDisplayViewModel;
        Assert.That(viewModel.Content, Is.InstanceOf<IHotspotDisplayViewModel>());

        viewModel.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Content, Is.Null);
            Assert.That(hotspotDisplay!.IsDisposed, Is.True);
        });
    }
}
