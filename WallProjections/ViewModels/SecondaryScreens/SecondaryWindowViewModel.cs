using System;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="ISecondaryWindowViewModel" />
public class SecondaryWindowViewModel : ViewModelBase, ISecondaryWindowViewModel, IDisposable
{
    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A <see cref="IViewModelProvider" /> used for creating child viewmodels.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// The backing field for <see cref="Content" />.
    /// </summary>
    private ViewModelBase? _content;

    /// <inheritdoc />
    public ViewModelBase? Content
    {
        get => _content;
        private set
        {
            if (_content is IDisposable disposable)
                disposable.Dispose();

            this.RaiseAndSetIfChanged(ref _content, value);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="SecondaryWindowViewModel" />.
    /// </summary>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public SecondaryWindowViewModel(IViewModelProvider vmProvider, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SecondaryWindowViewModel>();
        _vmProvider = vmProvider;
    }

    /// <inheritdoc />
    public void ShowHotspotDisplay(IConfig config)
    {
        Content = _vmProvider.GetHotspotDisplayViewModel(config);
        _logger.LogTrace("Showing Hotspot Display");
    }

    /// <inheritdoc />
    public void ShowPositionEditor(IEditorViewModel editorViewModel)
    {
        Content = editorViewModel.PositionEditor;
        _logger.LogTrace("Showing Position Editor");
    }

    /// <inheritdoc />
    public void ShowArUcoGrid()
    {
        Content = _vmProvider.GetArUcoGridViewModel();
        _logger.LogTrace("Showing ArUco Grid");
    }

    public void Dispose()
    {
        if (_content is IDisposable disposable)
            disposable.Dispose();

        _content = null;
        GC.SuppressFinalize(this);
    }
}
