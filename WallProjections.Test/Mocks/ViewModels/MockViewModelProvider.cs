using System.Collections.ObjectModel;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockViewModelProvider : IViewModelProvider
{
    /// <summary>
    /// Whether <see cref="Dispose" /> has been called
    /// </summary>
    public bool HasBeenDisposed { get; private set; }

    #region Display

    /// <summary>
    /// Creates a new <see cref="MockDisplayViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockDisplayViewModel"/></returns>
    public IDisplayViewModel GetDisplayViewModel(IConfig config) => new MockDisplayViewModel();

    /// <summary>
    /// Creates a new <see cref="MockImageViewModel" /> instance
    /// </summary>
    /// <returns>A new <see cref="MockImageViewModel" /> instance</returns>
    public IImageViewModel GetImageViewModel() => new MockImageViewModel();

    /// <summary>
    /// Creates a new <see cref="MockVideoViewModel"/>
    /// </summary>
    /// <returns>A new <see cref="MockVideoViewModel"/></returns>
    public IVideoViewModel GetVideoViewModel() => new MockVideoViewModel();

    #endregion

    #region Editor

    /// <summary>
    /// Creates a new <see cref="MockEditorViewModel" /> with the given <see cref="IConfig" />
    /// </summary>
    /// <param name="config">The config supplied to the constructor</param>
    /// <param name="fileHandler">The file handler supplied to the constructor</param>
    /// <returns>A new <see cref="MockEditorViewModel" /></returns>
    /// <seealso cref="MockEditorViewModel(IConfig, IViewModelProvider, IFileHandler)" />
    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler) =>
        new MockEditorViewModel(config, this, fileHandler);

    /// <summary>
    /// Creates a new empty <see cref="MockEditorViewModel" />
    /// </summary>
    /// <param name="fileHandler">The file handler supplied to the constructor</param>
    /// <returns>A new <see cref="MockEditorViewModel" /></returns>
    /// <seealso cref="MockEditorViewModel(IViewModelProvider, IFileHandler)" />
    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler) => new MockEditorViewModel(this, fileHandler);

    /// <summary>
    /// Creates a new <see cref="MockEditorHotspotViewModel" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MockEditorHotspotViewModel" /></returns>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(int id) =>
        new MockEditorHotspotViewModel(id, new Coord(0, 0, 0), "", "");

    /// <summary>
    /// Creates a new <see cref="MockEditorHotspotViewModel" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MockEditorHotspotViewModel" /></returns>
    /// <exception cref="Exception">
    /// Can throw all the exceptions that <see cref="File.ReadAllText(string)" />can throw
    /// </exception>
    public IEditorHotspotViewModel GetEditorHotspotViewModel(Hotspot hotspot) => new MockEditorHotspotViewModel(
        hotspot.Id,
        hotspot.Position,
        hotspot.Title,
        File.ReadAllText(hotspot.DescriptionPath),
        new ObservableCollection<IThumbnailViewModel>(
            hotspot.ImagePaths.Select(path => new MockThumbnailViewModel(path, Path.GetFileName(path)))
        ),
        new ObservableCollection<IThumbnailViewModel>(
            hotspot.VideoPaths.Select(path => new MockThumbnailViewModel(path, Path.GetFileName(path)))
        )
    );

    /// <summary>
    /// Creates a new <see cref="MockPositionEditorViewModel" />
    /// </summary>
    /// <returns>A new <see cref="MockPositionEditorViewModel" /></returns>
    public AbsPositionEditorViewModel GetPositionEditorViewModel() => new MockPositionEditorViewModel();

    /// <summary>
    /// Creates a new <see cref="MockDescriptionEditorViewModel" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MockDescriptionEditorViewModel" /></returns>
    public IDescriptionEditorViewModel GetDescriptionEditorViewModel() => new MockDescriptionEditorViewModel();

    /// <summary>
    /// Creates a new <see cref="MockMediaEditorViewModel" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MockMediaEditorViewModel" /></returns>
    public IMediaEditorViewModel GetMediaEditorViewModel(MediaEditorType type) => new MockMediaEditorViewModel(type);

    /// <summary>
    /// Creates a new <see cref="MockImportViewModel" />
    /// </summary>
    /// <inheritdoc />
    /// <returns>A new <see cref="MockImportViewModel" /></returns>
    public IThumbnailViewModel GetThumbnailViewModel(MediaEditorType type, string filePath) =>
        new MockThumbnailViewModel(filePath, Path.GetFileName(filePath));

    /// <summary>
    /// Creates a new <see cref="MockImportViewModel" /> linked to the given <see cref="IDescriptionEditorViewModel" />
    /// </summary>
    /// <param name="descVm">The parent <see cref="IDescriptionEditorViewModel" /></param>
    /// <returns>A new <see cref="MockImportViewModel" /></returns>
    public IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm) => new MockImportViewModel(descVm);

    #endregion

    #region Secondary

    /// <summary>
    /// Creates a new <see cref="MockSecondaryWindowViewModel" />
    /// </summary>
    /// <returns>A new <see cref="MockSecondaryWindowViewModel" /></returns>
    public ISecondaryWindowViewModel GetSecondaryWindowViewModel() => new MockSecondaryWindowViewModel(this);

    /// <summary>
    /// Creates a new <see cref="MockHotspotDisplayViewModel" />
    /// </summary>
    /// <returns>A new <see cref="MockHotspotDisplayViewModel"/></returns>
    public AbsHotspotDisplayViewModel GetHotspotDisplayViewModel(IConfig config) => new MockHotspotDisplayViewModel();

    /// <summary>
    /// Creates a new <see cref="MockHotspotProjectionViewModel" />
    /// </summary>
    /// <param name="hotspot">The hotspot to be projected</param>
    /// <returns>A new <see cref="MockHotspotProjectionViewModel" /></returns>
    public IHotspotProjectionViewModel GetHotspotProjectionViewModel(Hotspot hotspot) =>
        new MockHotspotProjectionViewModel(hotspot);

    /// <summary>
    /// Creates a new <see cref="ArUcoGridViewModel" />
    /// </summary>
    /// <returns>A new <see cref="ArUcoGridViewModel" /></returns>
    /// <remarks>Note that this is not a mock, as the viewmodel is so simple that there is no need for a mock</remarks>
    public AbsArUcoGridViewModel GetArUcoGridViewModel() => new ArUcoGridViewModel();

    #endregion

    public void Dispose()
    {
        HasBeenDisposed = true;
        GC.SuppressFinalize(this);
    }
}
