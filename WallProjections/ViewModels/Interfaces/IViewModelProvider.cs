using System;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.Interfaces;

/// <summary>
/// A provider for creating viewmodels using Dependency Injection
/// </summary>
public interface IViewModelProvider : IDisposable
{
    #region Display

    /// <summary>
    /// Creates a new <see cref="IDisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="IDisplayViewModel" /> instance</returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config);

    /// <summary>
    /// Creates a new <see cref="IImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel();

    /// <summary>
    /// Creates a new <see cref="IVideoViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IVideoViewModel" /> instance</returns>
    public IVideoViewModel GetVideoViewModel();

    #endregion

    #region Editor

    /// <summary>
    /// Creates a new <see cref="IEditorViewModel" /> instance based on the given <see cref="IConfig" />
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> providing initial state of the editor.</param>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <returns>A new <see cref="IEditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler);

    /// <summary>
    /// Creates a new empty <see cref="IEditorViewModel" />
    /// </summary>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <returns>A new <see cref="IEditorViewModel" /> instance</returns>
    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler);

    /// <summary>
    /// Creates a new <see cref="IEditorHotspotViewModel" /> instance
    /// </summary>
    /// <param name="id">The id of the hotspot.</param>
    /// <returns>A new <see cref="IEditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(int id);

    /// <summary>
    /// Creates a new <see cref="IEditorHotspotViewModel" /> instance
    /// </summary>
    /// <param name="hotspot">The hotspot to edit.</param>
    /// <returns>A new <see cref="IEditorHotspotViewModel" /> instance</returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(Hotspot hotspot);

    /// <summary>
    /// Creates a new <see cref="AbsPositionEditorViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="AbsPositionEditorViewModel" /> instance</returns>
    public AbsPositionEditorViewModel GetPositionEditorViewModel();

    /// <summary>
    /// Creates a new <see cref="IDescriptionEditorViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="IDescriptionEditorViewModel" /> instance</returns>
    public IDescriptionEditorViewModel GetDescriptionEditorViewModel();

    /// <summary>
    /// Creates a new <see cref="IMediaEditorViewModel" /> instance
    /// </summary>
    /// <param name="type">The type of media to edit.</param>
    /// <returns>A new <see cref="IMediaEditorViewModel" /> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="type" /> is not a valid <see cref="MediaEditorType" />.
    /// </exception>
    public IMediaEditorViewModel GetMediaEditorViewModel(MediaEditorType type);

    /// <summary>
    /// Creates a new <see cref="IThumbnailViewModel" /> instance
    /// </summary>
    /// <param name="type">The type of thumbnail.</param>
    /// <param name="filePath">The path to the media file.</param>
    /// <returns>A new <see cref="IThumbnailViewModel" /> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="type" /> is not a valid <see cref="MediaEditorType" />.
    /// </exception>
    public IThumbnailViewModel GetThumbnailViewModel(MediaEditorType type, string filePath);

    /// <summary>
    /// Creates a new <see cref="IImportViewModel" /> instance
    /// </summary>
    /// <param name="descVm">The parent <see cref="IDescriptionEditorViewModel"/> to which the data is imported.</param>
    /// <returns>A new <see cref="IImportViewModel" /> instance</returns>
    IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm);

    #endregion

    #region Secondary screens

    /// <summary>
    /// Creates a new <see cref="ISecondaryWindowViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="ISecondaryWindowViewModel" /> instance</returns>
    public ISecondaryWindowViewModel GetSecondaryWindowViewModel();

    /// <summary>
    /// Creates a new <see cref="AbsHotspotDisplayViewModel" /> instance
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <returns>A new <see cref="AbsHotspotDisplayViewModel" /> instance</returns>
    public AbsHotspotDisplayViewModel GetHotspotDisplayViewModel(IConfig config);

    /// <summary>
    /// Creates a new <see cref="AbsHotspotProjectionViewModel" /> instance
    /// </summary>
    /// <param name="hotspot">The hotspot to be projected</param>
    /// <returns>A new <see cref="AbsHotspotProjectionViewModel" /> instance</returns>
    public AbsHotspotProjectionViewModel GetHotspotProjectionViewModel(Hotspot hotspot);

    /// <summary>
    /// Creates a new <see cref="AbsArUcoGridViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="AbsArUcoGridViewModel" /> instance</returns>
    public AbsArUcoGridViewModel GetArUcoGridViewModel();

    #endregion
}
