using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    internal const string WelcomeTitle = "Select a hotspot";
    internal const string WelcomeMessage = "Please select a hotspot to start";

    internal const string GenericError = "An error occurred while loading this content.\n" +
                                         "Please report this to the museum staff.";

    internal const string NotFound = "Hmm...\n" +
                                     "Looks like this hotspot has missing content.\n" +
                                     "Please report this to the museum staff.";

    /// <summary>
    /// A logger for this class
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The <see cref="INavigator" /> used for opening the Editor and closing the Display.
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// The <see cref="IContentProvider" /> used to fetch Hotspot's content files
    /// </summary>
    private readonly IContentProvider _contentProvider;

    /// <summary>
    /// The <see cref="IViewModelProvider" /> used to fetch internal viewmodels
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// The <see cref="ILayoutProvider" /> used to fetch appropriate child layouts
    /// </summary>
    private readonly ILayoutProvider _layoutProvider;

    /// <summary>
    /// The <see cref="IHotspotHandler" /> used to listen for hotspot events
    /// </summary>
    private readonly IHotspotHandler _hotspotHandler;

    /// <summary>
    /// The backing field for <see cref="ContentViewModel" />
    /// </summary>
    private Layout _contentViewModel;

    /// <inheritdoc />
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when accessing this property
    /// </remarks>
    public Layout ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> with <see cref="ImageViewModel" />
    /// and <see cref="VideoViewModel" /> fetched by <paramref name="vmProvider" />,
    /// and starts listening for the <see cref="IHotspotHandler.HotspotActivated" /> event.
    /// </summary>
    /// <param name="navigator">The <see cref="INavigator" /> used for opening the Editor and closing the Display.</param>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> used to fetch internal viewmodels.</param>
    /// <param name="contentProvider">A <see cref="IContentProvider"/> for fetching data about hotspots.</param>
    /// <param name="layoutProvider">A <see cref="ILayoutProvider"/> for fetching appropriate child layouts.</param>
    /// <param name="hotspotHandler">A <see cref="IHotspotHandler" /> used to listen for hotspot events.</param>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public DisplayViewModel(
        INavigator navigator,
        IViewModelProvider vmProvider,
        IContentProvider contentProvider,
        ILayoutProvider layoutProvider,
        IHotspotHandler hotspotHandler,
        ILoggerFactory loggerFactory
    )
    {
        _logger = loggerFactory.CreateLogger<DisplayViewModel>();
        _navigator = navigator;
        _contentProvider = contentProvider;
        _vmProvider = vmProvider;
        _layoutProvider = layoutProvider;
        _hotspotHandler = hotspotHandler;
        _hotspotHandler.HotspotActivated += OnHotspotActivated;
        _contentViewModel = CreateWelcomeLayout(layoutProvider);
    }

    /// <summary>
    /// Returns a new <see cref="DescriptionViewModel" /> with a welcome message
    /// </summary>
    /// <param name="layoutProvider">The <see cref="ILayoutProvider" /> used to fetch the layout</param>
    private static Layout CreateWelcomeLayout(ILayoutProvider layoutProvider) =>
        layoutProvider.GetSimpleDescriptionLayout(WelcomeTitle, WelcomeMessage);

    /// <inheritdoc />
    public void OnHotspotActivated(object? sender, IHotspotHandler.HotspotArgs e)
    {
        lock (this)
        {
            if (ContentViewModel.HotspotId == e.Id) return;

            ShowHotspot(e.Id);
        }
    }

    /// <summary>
    /// Loads the content of the hotspot with the given ID if <see cref="_contentProvider" /> has been set
    /// </summary>
    /// <param name="hotspotId">The ID of a hotspot to show</param>
    private async void ShowHotspot(int hotspotId) => await Task.Run(() =>
    {
        try
        {
            var media = _contentProvider.GetMedia(hotspotId);
            if (ContentViewModel is IDisposable disposable)
                disposable.Dispose();
            ContentViewModel = _layoutProvider.GetLayout(_vmProvider, media);
        }
        catch (Exception e) when (e is IConfig.HotspotNotFoundException or FileNotFoundException)
        {
            _logger.LogError(e, "Error while loading content for hotspot {HotspotId} (Not Found)", hotspotId);
            ContentViewModel = _layoutProvider.GetErrorLayout(NotFound);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading content for hotspot {HotspotId}", hotspotId);
            ContentViewModel = _layoutProvider.GetErrorLayout(GenericError);
        }
    });

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
    /// Unsubscribes from <see cref="_hotspotHandler" />'s events and disposes of <see cref="VideoViewModel" />
    /// </summary>
    public void Dispose()
    {
        _hotspotHandler.HotspotActivated -= OnHotspotActivated;
        lock (this)
        {
            if (ContentViewModel is IDisposable disposable)
                disposable.Dispose();
            ContentViewModel = CreateWelcomeLayout(_layoutProvider);
        }
    }
}
