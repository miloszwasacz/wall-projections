using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IDisplayViewModel" />
public sealed class DisplayViewModel : ViewModelBase, IDisplayViewModel
{
    //TODO Localized strings?
    internal const string PromptMessage = @"Please select a hotspot to start";

    internal const string GenericError = @"An error occurred while loading this content.
Please report this to the museum staff.";

    internal const string NotFound = @"Hmm...
Looks like this hotspot has missing content.
Please report this to the museum staff.";

    /// <summary>
    /// The <see cref="INavigator" /> used for opening the Editor and closing the Display.
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// The <see cref="IContentProvider" /> used to fetch Hotspot's content files
    /// </summary>
    private readonly IContentProvider _contentProvider;

    private readonly IViewModelProvider _vmProvider;

    private readonly ILayoutProvider _layoutProvider;

    private ILayout _contentViewModel;

    /// <summary>
    /// The <see cref="IPythonHandler" /> used to listen for Python events
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> with <see cref="ImageViewModel" />
    /// and <see cref="VideoViewModel" /> fetched by <paramref name="vmProvider" />,
    /// and starts listening for <see cref="IPythonHandler.HotspotSelected">Python events</see>
    /// </summary>
    /// <param name="navigator">The <see cref="INavigator" /> used for opening the Editor and closing the Display</param>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> used to fetch internal viewmodels</param>
    /// <param name="contentProvider">A <see cref="IContentProvider"/> for fetching data about hotspots.</param>
    /// <param name="pythonHandler">The <see cref="IPythonHandler" /> used to listen for Python events</param>
    public DisplayViewModel(
        INavigator navigator,
        IViewModelProvider vmProvider,
        IContentProvider contentProvider,
        ILayoutProvider layoutProvider,
        IPythonHandler pythonHandler
    )
    {
        _navigator = navigator;
        _contentProvider = contentProvider;
        _vmProvider = vmProvider;
        _layoutProvider = layoutProvider;
        _pythonHandler = pythonHandler;
        _pythonHandler.HotspotSelected += OnHotspotSelected;

        ContentViewModel = new DescriptionViewModel("Select hotspot", PromptMessage);
    }

    public ILayout ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    /// <inheritdoc />
    public void OnHotspotSelected(object? sender, IPythonHandler.HotspotSelectedArgs e)
    {
        ShowHotspot(e.Id);
    }

    /// <summary>
    /// Loads the content of the hotspot with the given ID if <see cref="_contentProvider" /> has been set
    /// </summary>
    /// <param name="hotspotId">The ID of a hotspot to show</param>
    private void ShowHotspot(int hotspotId)
    {
        try
        {

            var media = _contentProvider.GetMedia(hotspotId);
            var oldViewModel = ContentViewModel;

            ContentViewModel = _layoutProvider.GetLayout(_vmProvider, media);
            oldViewModel.Dispose();
        }
        catch (Exception e) when (e is IConfig.HotspotNotFoundException or FileNotFoundException)
        {
            //TODO Write to Log instead of Console
            Console.Error.WriteLine(e);
            ContentViewModel = new DescriptionViewModel("Error", NotFound);
        }
        catch (Exception e)
        {
            //TODO Write to Log instead of Console
            Console.Error.WriteLine(e);
            ContentViewModel = new DescriptionViewModel("Error", GenericError);
        }
    }

    /// <inheritdoc />
    public void OpenEditor()
    {
        _navigator.OpenEditor();
    }

    /// <inheritdoc />
    public void CloseDisplay()
    {
        _navigator.Shutdown();
    }

    /// <summary>
    /// Unsubscribes from <see cref="IPythonHandler.HotspotSelected" /> and disposes of <see cref="VideoViewModel" />
    /// </summary>
    public void Dispose()
    {
        _pythonHandler.HotspotSelected -= OnHotspotSelected;
        ContentViewModel.Dispose();
    }
}
