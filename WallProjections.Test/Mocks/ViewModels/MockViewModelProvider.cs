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

    public IEditorViewModel GetEditorViewModel(IConfig config, IFileHandler fileHandler)
    {
        throw new NotImplementedException();
    }

    public IEditorViewModel GetEditorViewModel(IFileHandler fileHandler)
    {
        throw new NotImplementedException();
    }

    public IEditorHotspotViewModel GetEditorHotspotViewModel(int id)
    {
        throw new NotImplementedException();
    }

    public IEditorHotspotViewModel GetEditorHotspotViewModel(Hotspot hotspot)
    {
        throw new NotImplementedException();
    }

    public IDescriptionEditorViewModel GetDescriptionEditorViewModel()
    {
        throw new NotImplementedException();
    }

    public IMediaEditorViewModel GetMediaEditorViewModel(MediaEditorType type)
    {
        throw new NotImplementedException();
    }

    public IThumbnailViewModel GetThumbnailViewModel(MediaEditorType type, string filePath, int gridRow, int gridColumn)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new <see cref="MockImportViewModel"/> linked to the given <see cref="IDescriptionEditorViewModel" />
    /// </summary>
    /// <param name="descVm">The parent <see cref="IDescriptionEditorViewModel" /></param>
    /// <returns>A new <see cref="MockImportViewModel" /></returns>
    public IImportViewModel GetImportViewModel(IDescriptionEditorViewModel descVm) => new MockImportViewModel(descVm);
}
