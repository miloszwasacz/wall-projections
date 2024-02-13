using System;
using System.Threading;
using ReactiveUI;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IDescriptionEditorViewModel" />
public class DescriptionEditorViewModel : ViewModelBase, IDescriptionEditorViewModel
{
    /// <inheritdoc />
    public event EventHandler<EventArgs>? ContentChanged;

    /// <summary>
    /// Whether the <see cref="Hotspot" /> is being changed.
    /// </summary>
    private bool _isHotspotChanging;

    /// <summary>
    /// A mutex guarding sequential access to <see cref="_isHotspotChanging" />.
    /// </summary>
    private readonly Mutex _hotspotMutex = new();

    /// <summary>
    /// The backing field for <see cref="Hotspot"/>.
    /// </summary>
    private IEditorHotspotViewModel? _hotspot;

    /// <inheritdoc />
    public IEditorHotspotViewModel? Hotspot
    {
        set
        {
            _hotspotMutex.WaitOne();
            _isHotspotChanging = true;

            _hotspot = value;
            this.RaisePropertyChanged(nameof(Title));
            this.RaisePropertyChanged(nameof(Description));
            this.RaisePropertyChanged(nameof(IsEnabled));

            _isHotspotChanging = false;
            _hotspotMutex.ReleaseMutex();
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

            if (!_isHotspotChanging)
                ContentChanged?.Invoke(this, EventArgs.Empty);
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

            if (!_isHotspotChanging)
                ContentChanged?.Invoke(this, EventArgs.Empty);
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
    public DescriptionEditorViewModel(IViewModelProvider vmProvider)
    {
        Importer = vmProvider.GetImportViewModel(this);
    }
}
