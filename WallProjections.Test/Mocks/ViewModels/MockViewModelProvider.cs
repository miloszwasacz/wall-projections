using System.Collections.ObjectModel;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.ViewModels.Display;
using WallProjections.Test.Mocks.ViewModels.Editor;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockViewModelProvider : IViewModelProvider
{
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

    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler)
    {
        throw new NotImplementedException();
    }

    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler)
    {
        throw new NotImplementedException();
    }

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
}
