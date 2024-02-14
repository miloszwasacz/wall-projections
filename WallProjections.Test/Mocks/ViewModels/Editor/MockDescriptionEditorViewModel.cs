using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Test.Mocks.ViewModels.Editor;

public class MockDescriptionEditorViewModel : IDescriptionEditorViewModel
{
    public event EventHandler<EventArgs>? ContentChanged;
    public IEditorHotspotViewModel? Hotspot { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; }
    public IImportViewModel Importer { get; }

    public MockDescriptionEditorViewModel(
        string title = "",
        string description = "",
        bool isEnabled = false,
        Func<IDescriptionEditorViewModel, IImportViewModel>? importerConstructor = null
    )
    {
        Title = title;
        Description = description;
        IsEnabled = isEnabled;
        Importer = importerConstructor?.Invoke(this)!;
    }
}
