using System;
using LibVLCSharp.Shared;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Display.Layouts;
using WallProjections.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.ViewModels;

/// <inheritdoc cref="IViewModelProvider" />
public sealed class ViewModelProvider : IViewModelProvider, IDisposable
{
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
    /// Creates a new <see cref="ViewModelProvider" /> with the given <see cref="INavigator" />
    /// </summary>
    /// <param name="navigator">The app-wide <see cref="INavigator" /> used for navigation between views</param>
    /// <param name="pythonHandler">The app-wide <see cref="IPythonHandler" /> used for Python interop</param>
    public ViewModelProvider(INavigator navigator, IPythonHandler pythonHandler)
    {
        _navigator = navigator;
        _pythonHandler = pythonHandler;
    }

    /// <summary>
    /// A global instance of <see cref="LibVLC" /> to use for <see cref="LibVLCSharp" /> library
    /// </summary>
    /// <remarks>Only instantiated if needed</remarks>
    private LibVLC LibVlc => _libVlc ??= new LibVLC();

    #region Display

    //TODO Refactor this to use DI
    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="DisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) =>
        new DisplayViewModel(_navigator, this, new ContentProvider(config), new LayoutProvider(), _pythonHandler);

    /// <summary>
    /// Creates a new <see cref="ImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new ImageViewModel();

    /// <summary>
    /// Creates a new <see cref="VideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="VideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel() => new VideoViewModel(LibVlc, new VLCMediaPlayer(LibVlc));

    #endregion

    #region Editor

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> instance based on the given <see cref="IConfig" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler) =>
        new EditorViewModel(config, _navigator, fileHandler, _pythonHandler, this);

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="EditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler) =>
        new EditorViewModel(_navigator, fileHandler, _pythonHandler, this);

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
    public IPositionEditorViewModel GetPositionEditorViewModel() => new PositionEditorViewModel();

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
        MediaEditorType.Images => new ImageThumbnailViewModel(filePath, new ProcessProxy()),
        MediaEditorType.Videos => new VideoThumbnailViewModel(filePath, new ProcessProxy()),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown media type")
    };

    /// <summary>
    /// Creates a new <see cref="ImportViewModel" /> instance
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="ImportViewModel" /> instance</returns>
    public IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm) => new ImportViewModel(descVm);

    #endregion

    #region Secondary screens

    /// <summary>
    /// Creates a new <see cref="SecondaryWindowViewModel" /> instance
    /// with <i>TH</i> being <see cref="HotspotDisplayViewModel" />
    /// </summary>
    /// <returns>A new <see cref="SecondaryWindowViewModel" /> instance</returns>
    public ISecondaryWindowViewModel GetSecondaryWindowViewModel() => new SecondaryWindowViewModel(this);

    /// <summary>
    /// Creates a new <see cref="HotspotDisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="HotspotDisplayViewModel" /> instance</returns>
    public IHotspotDisplayViewModel GetHotspotDisplayViewModel(IConfig config) =>
        new HotspotDisplayViewModel(config, _pythonHandler, this);

    /// <summary>
    /// Creates a new <see cref="HotspotProjectionViewModel" /> instance
    /// </summary>
    /// <param name="hotspot">The hotspot to be projected</param>
    /// <returns>A new <see cref="HotspotProjectionViewModel" /> instance</returns>
    public IHotspotProjectionViewModel GetHotspotProjectionViewModel(Hotspot hotspot) =>
        new HotspotProjectionViewModel(hotspot);

    /// <summary>
    /// Creates a new <see cref="ArUcoGridViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ArUcoGridViewModel" /> instance</returns>
    public IArUcoGridViewModel GetArUcoGridViewModel() => new ArUcoGridViewModel();

    #endregion

    /// <summary>
    /// Disposes of the <see cref="LibVlc" /> instance on resets the backing field to <i>null</i>,
    /// so that a new instance can be created if needed
    /// </summary>
    public void Dispose()
    {
        _libVlc?.Dispose();
        _libVlc = null;
    }
}
