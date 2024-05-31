using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IDisplayViewModel" />
public sealed class DisplayViewModel : ViewModelBase, IDisplayViewModel
{
    //TODO Localized strings?
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

    /// <summary>
    /// Whether the Display is performing cleanup and closing
    /// </summary>
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when accessing this field
    /// </remarks>
    private bool _closing;

    /// <inheritdoc />
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when accessing this property
    /// <br /><br />
    /// The setter unsubscribes from the Deactivated event of the old <see cref="ContentViewModel" /> and subscribes to the new one
    /// </remarks>
    public Layout ContentViewModel
    {
        get => _contentViewModel;
        private set
        {
            _contentViewModel.Deactivated -= OnLayoutDeactivated;
            this.RaiseAndSetIfChanged(ref _contentViewModel, value);
            _contentViewModel.Deactivated += OnLayoutDeactivated;
        }
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
        _contentViewModel = layoutProvider.GetWelcomeLayout();
    }

    #region Event Handlers

    /// <inheritdoc />
    public async void OnHotspotActivated(object? sender, IHotspotHandler.HotspotArgs e) => await Task.Run(() =>
    {
        lock (this)
        {
            if (ContentViewModel.HotspotId == e.Id || _closing) return;

            _logger.LogTrace("Activating layout for hotspot with id {HotspotId}", e.Id);
            ShowHotspot(e.Id);
        }
    });

    /// <summary>
    /// Deactivates the current layout and shows the welcome screen
    /// </summary>
    /// <param name="sender">The sender of the event (unused)</param>
    /// <param name="e">The event arguments holding the deactivated layout</param>
    private void OnLayoutDeactivated(object? sender, Layout.DeactivationEventArgs e)
    {
        lock (this)
        {
            if (!ContentViewModel.IsDeactivated(e) || _closing) return;

            _logger.LogTrace("Deactivating layout for hotspot with id {HotspotId}", ContentViewModel.HotspotId);
            ShowWelcomeScreen();
        }
    }

    #endregion

    /// <summary>
    /// Loads the content of the hotspot with the given ID if <see cref="_contentProvider" /> has been set
    /// </summary>
    /// <param name="hotspotId">The ID of a hotspot to show</param>
    private void ShowHotspot(int hotspotId)
    {
        try
        {
            var media = _contentProvider.GetMedia(hotspotId);
            if (ContentViewModel is IDisposable disposable)
                disposable.Dispose();
            ContentViewModel = _layoutProvider.GetLayout(_vmProvider, media);
            _logger.LogTrace("Successfully loaded content for hotspot {HotspotId}", hotspotId);
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
    }

    /// <summary>
    /// Disposes of the current <see cref="ContentViewModel" /> (if any and is <see cref="IDisposable" />)
    /// and shows the welcome screen
    /// </summary>
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when calling this method
    /// </remarks>
    private void ShowWelcomeScreen()
    {
        if (ContentViewModel.HotspotId is not null)
            _hotspotHandler.DeactivateHotspot(ContentViewModel.HotspotId.Value);

        if (ContentViewModel is IDisposable disposable)
            disposable.Dispose();

        ContentViewModel = _layoutProvider.GetWelcomeLayout();
    }

    /// <summary>
    /// Disposes of the current <see cref="ContentViewModel" /> (if any and is <see cref="IDisposable" />)
    /// and shows message screen informing the user about Python cleanup
    /// </summary>
    /// <remarks>
    /// Remember to use <i>lock (this)</i> when calling this method
    /// </remarks>
    private void ShowCleanupMessage()
    {
        if (ContentViewModel.HotspotId is not null)
            _hotspotHandler.DeactivateHotspot(ContentViewModel.HotspotId.Value);

        if (ContentViewModel is IDisposable disposable)
            disposable.Dispose();

        var (title, description) = IDisplayViewModel.CleanupMessage;
        ContentViewModel = _layoutProvider.GetMessageLayout(title, description);
    }

    /// <inheritdoc />
    public Task OpenEditor() => Task.Run(async () =>
    {
        lock (this)
        {
            _closing = true;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            lock (this)
            {
                ShowCleanupMessage();
            }
        });

        // Wait for the cleanup message to be properly displayed
        await Task.Delay(500);

        Dispatcher.UIThread.Invoke(() => _navigator.OpenEditor());
    });

    /// <inheritdoc />
    public void CloseDisplay()
    {
        _navigator.Shutdown();
    }

    /// <summary>
    /// Unsubscribes from <see cref="_hotspotHandler" />'s events and shows the welcome screen
    /// </summary>
    public void Dispose()
    {
        _hotspotHandler.HotspotActivated -= OnHotspotActivated;
        lock (this)
        {
            ShowWelcomeScreen();
        }
    }
}
