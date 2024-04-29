using System;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="IViewModelProvider" />
public sealed class ViewModelProvider : IViewModelProvider
{
    /// <summary>
    /// A factory for creating loggers, passed to the viewmodels that need it
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// The backing field for the <see cref="LibVlc" /> property
    /// </summary>
    /// <remarks>Reset when <see cref="Dispose" /> is called so that a new instance can be created if needed</remarks>
    private LibVLC? _libVlc;

    /// <summary>
    /// The app-wide <see cref="INavigator" /> used for navigation between views
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// The app-wide <see cref="IPythonHandler" /> used for Python interop
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// The backing field for the <see cref="HotspotHandler" /> property
    /// </summary>
    /// <remarks>Reset when <see cref="Dispose" /> is called so that a new instance can be created if needed</remarks>
    private IHotspotHandler? _hotspotHandler;

    /// <summary>
    /// The app-wide <see cref="IProcessProxy" /> used for starting up external processes
    /// </summary>
    private readonly IProcessProxy _processProxy;

    /// <summary>
    /// A factory for creating a <see cref="IHotspotHandler" /> linked to the given <see cref="_pythonHandler" />
    /// </summary>
    private readonly Func<IPythonHandler, IHotspotHandler> _hotspotHandlerFactory;

    /// <summary>
    /// A factory for creating <see cref="IContentProvider" />s based on the given <see cref="IConfig" />
    /// </summary>
    private readonly Func<IConfig, IContentProvider> _contentProviderFactory;

    /// <summary>
    /// A factory for creating <see cref="ILayoutProvider" />s
    /// </summary>
    private readonly Func<ILayoutProvider> _layoutProviderFactory;

    /// <summary>
    /// Creates a new <see cref="ViewModelProvider" /> with the given <see cref="INavigator" />
    /// </summary>
    /// <param name="navigator">The app-wide <see cref="INavigator" /> used for navigation between views</param>
    /// <param name="pythonHandler">The app-wide <see cref="IPythonHandler" /> used for Python interop</param>
    /// <param name="processProxy">
    /// The app-wide <see cref="IProcessProxy" /> used for starting up external processes
    /// </param>
    /// <param name="hotspotHandlerFactory">
    /// A factory for creating a <see cref="IHotspotHandler" /> based on <paramref name="pythonHandler" />
    /// </param>
    /// <param name="contentProviderFactory">
    /// A factory for creating <see cref="IContentProvider" />s based on the given <see cref="IConfig" />
    /// </param>
    /// <param name="layoutProviderFactory">A factory for creating <see cref="ILayoutProvider" />s</param>
    /// <param name="loggerFactory">A factory for creating loggers</param>
    public ViewModelProvider(
        INavigator navigator,
        IPythonHandler pythonHandler,
        IProcessProxy processProxy,
        Func<IPythonHandler, IHotspotHandler> hotspotHandlerFactory,
        Func<IConfig, IContentProvider> contentProviderFactory,
        Func<ILayoutProvider> layoutProviderFactory,
        ILoggerFactory loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _navigator = navigator;
        _pythonHandler = pythonHandler;
        _processProxy = processProxy;
        _hotspotHandlerFactory = hotspotHandlerFactory;
        _contentProviderFactory = contentProviderFactory;
        _layoutProviderFactory = layoutProviderFactory;
    }

    /// <summary>
    /// A global instance of <see cref="LibVLC" /> to use for <see cref="LibVLCSharp" /> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    /// <summary>
    /// The app-wide <see cref="IHotspotHandler" /> used for handling hotspot events
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private IHotspotHandler HotspotHandler => _hotspotHandler ??= _hotspotHandlerFactory(_pythonHandler);

    #region Display

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="DisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) => new DisplayViewModel(
        _navigator,
        this,
        _contentProviderFactory(config),
        _layoutProviderFactory(),
        HotspotHandler,
        _loggerFactory
    );

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new ImageViewModel(_loggerFactory);

    /// <summary>
    /// Creates a new <see cref="VideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="VideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel() =>
        new VideoViewModel(LibVlc, new VLCMediaPlayer(LibVlc), _loggerFactory);

    #endregion

    #region Editor

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> instance based on the given <see cref="IConfig" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler) =>
        new EditorViewModel(config, _navigator, fileHandler, _pythonHandler, this, _loggerFactory);

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler) =>
        new EditorViewModel(_navigator, fileHandler, _pythonHandler, this, _loggerFactory);

    /// <summary>
    /// Creates a new <see cref="EditorHotspotViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(int id) =>
        new EditorHotspotViewModel(id, this);

    /// <summary>
    /// Creates a new <see cref="EditorHotspotViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(Hotspot hotspot) =>
        new EditorHotspotViewModel(hotspot, this);

    /// <summary>
    /// Creates a new <see cref="PositionEditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="PositionEditorViewModel" /> instance</returns>
    public AbsPositionEditorViewModel GetPositionEditorViewModel() => new PositionEditorViewModel();

    /// <summary>
    /// Creates a new <see cref="DescriptionEditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="DescriptionEditorViewModel" /> instance</returns>
    public IDescriptionEditorViewModel GetDescriptionEditorViewModel() => new DescriptionEditorViewModel(this);

    /// <summary>
    /// Creates a new <see cref="MediaEditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MediaEditorViewModel" /> instance</returns>
    public IMediaEditorViewModel GetMediaEditorViewModel(MediaEditorType type) => new MediaEditorViewModel(type switch
    {
        MediaEditorType.Images => "Images",
        MediaEditorType.Videos => "Videos",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
    });

    /// <inheritdoc />
    /// <seealso cref="ImageThumbnailViewModel" />
    /// <seealso cref="VideoThumbnailViewModel" />
    public IThumbnailViewModel GetThumbnailViewModel(MediaEditorType type, string filePath) => type switch
    {
        MediaEditorType.Images => new ImageThumbnailViewModel(filePath, _processProxy, _loggerFactory),
        MediaEditorType.Videos => new VideoThumbnailViewModel(filePath, _processProxy),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
    };

    /// <summary>
    /// Creates a new <see cref="ImportViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="ImportViewModel" /> instance</returns>
    public IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm) =>
        new ImportViewModel(descVm, _loggerFactory);

    #endregion

    #region Secondary screens

    /// <summary>
    /// Creates a new <see cref="SecondaryWindowViewModel" /> instance
    /// with <i>TH</i> being <see cref="HotspotDisplayViewModel" />
    /// </summary>
    /// <returns>A new <see cref="SecondaryWindowViewModel" /> instance</returns>
    public ISecondaryWindowViewModel GetSecondaryWindowViewModel() =>
        new SecondaryWindowViewModel(this, _loggerFactory);

    /// <summary>
    /// Creates a new <see cref="HotspotDisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="HotspotDisplayViewModel" /> instance</returns>
    public AbsHotspotDisplayViewModel GetHotspotDisplayViewModel(IConfig config) =>
        new HotspotDisplayViewModel(config, HotspotHandler, this, _loggerFactory);

    /// <summary>
    /// Creates a new <see cref="AbsHotspotProjectionViewModel" /> instance
    /// </summary>
    /// <param name="hotspot">The hotspot to be projected</param>
    /// <returns>A new <see cref="AbsHotspotProjectionViewModel" /> instance</returns>
    public AbsHotspotProjectionViewModel GetHotspotProjectionViewModel(Hotspot hotspot) =>
        new HotspotProjectionViewModel(hotspot);

    /// <summary>
    /// Creates a new <see cref="ArUcoGridViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ArUcoGridViewModel" /> instance</returns>
    public AbsArUcoGridViewModel GetArUcoGridViewModel() => new ArUcoGridViewModel();

    #endregion

    /// <summary>
    /// Disposes of the <see cref="LibVlc" /> instance on resets the backing field to <i>null</i>,
    /// so that a new instance can be created if needed
    /// </summary>
    public void Dispose()
    {
        _hotspotHandler?.Dispose();
        _hotspotHandler = null;
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
