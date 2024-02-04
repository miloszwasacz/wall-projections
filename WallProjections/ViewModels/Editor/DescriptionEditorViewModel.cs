using ReactiveUI;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IDescriptionEditorViewModel" />
public class DescriptionEditorViewModel : ViewModelBase, IDescriptionEditorViewModel
{
    /// <summary>
    /// The backing field for <see cref="Hotspot"/>.
    /// </summary>
    private IEditorHotspotViewModel? _hotspot;

    /// <summary>
    /// The source <see cref="IEditorHotspotViewModel">hotspot</see> that is being edited.
    /// </summary>
    public IEditorHotspotViewModel? Hotspot
    {
        set
        {
            _hotspot = value;
            this.RaisePropertyChanged(nameof(Title));
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>
    /// <inheritdoc />
    /// <br /><br />
    /// The title is stored directly in the <see cref="Hotspot" />'s
    /// <see cref="IEditorHotspotViewModel.Title"/>.
    /// </summary>
    public string Title
    {
        get => _hotspot?.Title ?? "";
        set
        {
            if (_hotspot is null) return;

            _hotspot.Title = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// <inheritdoc />
    /// <br /><br />
    /// The description is stored directly in the <see cref="Hotspot" />'s
    /// <see cref="IEditorHotspotViewModel.Description"/>.
    /// </summary>
    public string Description
    {
        get => _hotspot?.Description ?? "";
        set
        {
            if (_hotspot is null) return;

            _hotspot.Description = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Whether the Description can be edited (i.e. <see cref="Hotspot" /> is not <i>null</i>).
    /// </summary>
    public bool IsEnabled => _hotspot is not null;

    /// <inheritdoc />
    public IImportViewModel Importer { get; }

    /// <summary>
    /// Creates a new <see cref="DescriptionEditorViewModel" />.
    /// </summary>
    public DescriptionEditorViewModel()
    {
        //TODO Change to use ViewModelProvider
        Importer = new ImportViewModel(this);
    }
}
