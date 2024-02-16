using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockDescriptionEditorViewModel : IDescriptionEditorViewModel
{
    private IEditorHotspotViewModel? _hotspot;
    private string _title;
    private string _description;

    public event EventHandler<EventArgs>? ContentChanged;

    public IEditorHotspotViewModel? Hotspot
    {
        get => _hotspot;
        set
        {
            _hotspot = value;

            if (value is null) return;

            _title = _hotspot!.Title = value.Title;
            _description = _hotspot.Description = value.Description;
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            ContentChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            ContentChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public bool IsEnabled => _hotspot is not null;

    public IImportViewModel Importer { get; }

    public MockDescriptionEditorViewModel(
        string title = "",
        string description = "",
        Func<IDescriptionEditorViewModel, IImportViewModel>? importerConstructor = null
    )
    {
        _hotspot = null;
        _title = title;
        _description = description;
        Importer = importerConstructor?.Invoke(this)!;
    }
}
