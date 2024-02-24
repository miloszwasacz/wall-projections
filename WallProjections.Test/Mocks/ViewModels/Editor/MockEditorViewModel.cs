using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockEditorViewModel : IEditorViewModel
{
    public ObservableHotspotCollection<IEditorHotspotViewModel> Hotspots { get; set; }
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }
    public IDescriptionEditorViewModel DescriptionEditor { get; }
    public IMediaEditorViewModel ImageEditor { get; }
    public IMediaEditorViewModel VideoEditor { get; }
    public bool IsSaved { get; set; } = false;
    public bool CanExport { get; set; } = false;

    // ReSharper disable once UnusedParameter.Local
    public MockEditorViewModel(IConfig config, IViewModelProvider vmProvider, IFileHandler fileHandler) :
        this(vmProvider, fileHandler)
    {
        //TODO Do something with the config
    }

    // ReSharper disable once UnusedParameter.Local
    public MockEditorViewModel(IViewModelProvider vmProvider, IFileHandler fileHandler)
    {
        //TODO Do something with the fileHandler

        Hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>();
        DescriptionEditor = vmProvider.GetDescriptionEditorViewModel();
        ImageEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        VideoEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);
    }

    public void AddHotspot()
    {
        throw new NotImplementedException();
    }

    public void DeleteHotspot(IEditorHotspotViewModel hotspot)
    {
        throw new NotImplementedException();
    }

    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        throw new NotImplementedException();
    }

    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        throw new NotImplementedException();
    }

    public bool SaveConfig()
    {
        throw new NotImplementedException();
    }

    public bool ImportConfig(string filePath)
    {
        throw new NotImplementedException();
    }

    public bool ExportConfig(string exportPath)
    {
        throw new NotImplementedException();
    }

    public void CloseEditor()
    {
        throw new NotImplementedException();
    }
}
