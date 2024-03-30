using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockEditorViewModel : IEditorViewModel
{
    public ObservableHotspotCollection<IEditorHotspotViewModel> Hotspots { get; set; }
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }
    public AbsPositionEditorViewModel PositionEditor { get; }
    public IDescriptionEditorViewModel DescriptionEditor { get; }
    public IMediaEditorViewModel ImageEditor { get; }
    public IMediaEditorViewModel VideoEditor { get; }
    public bool IsSaved { get; set; } = false;
    public bool IsImportSafe { get; set; } = false;
    public bool CanExport { get; set; } = false;
    public bool IsSaving { get; set; } = false;
    public bool IsImporting { get; set; } = false;
    public bool IsExporting { get; set; } = false;
    public bool IsCalibrating { get; set; } = false;
    public bool AreActionsDisabled { get; private set; }

    public bool TryAcquireActionLock()
    {
        lock (this)
        {
            if (AreActionsDisabled) return false;

            AreActionsDisabled = true;
            return true;
        }
    }

    public void ReleaseActionLock()
    {
        lock (this)
        {
            AreActionsDisabled = false;
        }
    }

    public async Task<bool> WithActionLock(Func<Task> action)
    {
        if (!TryAcquireActionLock()) return false;

        try
        {
            await action();
            return true;
        }
        finally
        {
            ReleaseActionLock();
        }
    }

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
        PositionEditor = vmProvider.GetPositionEditorViewModel();
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

    public Task<bool> SaveConfig()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ImportConfig(string filePath)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExportConfig(string exportPath)
    {
        throw new NotImplementedException();
    }

    public void ShowCalibrationMarkers()
    {
        throw new NotImplementedException();
    }

    public void HideCalibrationMarkers()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CalibrateCamera()
    {
        throw new NotImplementedException();
    }

    public void CloseEditor()
    {
        throw new NotImplementedException();
    }
}
