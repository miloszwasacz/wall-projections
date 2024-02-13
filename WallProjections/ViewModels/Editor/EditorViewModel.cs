using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Platform.Storage;
using ReactiveUI;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IEditorViewModel{T}" />
public class EditorViewModel : ViewModelBase, IEditorViewModel
{
    /// <summary>
    /// A <see cref="IViewModelProvider" /> used for creating child viewmodels.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.
    /// </summary>
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// The backing field for <see cref="Hotspots" />.
    /// </summary>
    private ObservableHotspotCollection<IEditorHotspotViewModel> _hotspots;

    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private IEditorHotspotViewModel? _selectedHotspot;

    /// <summary>
    /// The backing field for <see cref="IsSaved" />.
    /// </summary>
    private bool _isSaved;

    /// <inheritdoc />
    public ObservableHotspotCollection<IEditorHotspotViewModel> Hotspots
    {
        get => _hotspots;
        set
        {
            _hotspots.CollectionChanged -= SetUnsaved;

            this.RaiseAndSetIfChanged(ref _hotspots, value);
            SelectedHotspot = null;

            _hotspots.CollectionChanged += SetUnsaved;
            IsSaved = false;
        }
    }

    /// <inheritdoc />
    public IEditorHotspotViewModel? SelectedHotspot
    {
        get => _selectedHotspot;
        set
        {
            // Skip changes to SelectedItem if the collection is only updating because of PropertyChanged on an item
            if (Hotspots.IsItemUpdating) return;

            this.RaiseAndSetIfChanged(ref _selectedHotspot, value);
            DescriptionEditor.Hotspot = value;

            ImageEditor.Media.CollectionChanged -= SetUnsaved;
            VideoEditor.Media.CollectionChanged -= SetUnsaved;

            if (value is not null)
            {
                ImageEditor.Media = value.Images;
                VideoEditor.Media = value.Videos;
            }
            else
            {
                ImageEditor.Media = new ObservableCollection<IThumbnailViewModel>();
                VideoEditor.Media = new ObservableCollection<IThumbnailViewModel>();
            }

            ImageEditor.Media.CollectionChanged += SetUnsaved;
            VideoEditor.Media.CollectionChanged += SetUnsaved;
        }
    }

    /// <inheritdoc />
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    /// <inheritdoc />
    public IMediaEditorViewModel ImageEditor { get; }

    /// <inheritdoc />
    public IMediaEditorViewModel VideoEditor { get; }

    /// <inheritdoc />
    public bool IsSaved
    {
        get => _isSaved;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isSaved, value);
            this.RaisePropertyChanged(nameof(IEditorViewModel.CloseButtonText));
        }
    }

    /// <inheritdoc />
    public void AddHotspot()
    {
        IsSaved = false;

        // Find the smallest available ID
        var newId = 0;
        foreach (var id in Hotspots.Select(h => h.Id).OrderBy(id => id))
        {
            if (newId == id)
                newId++;
        }

        var newHotspot = _vmProvider.GetEditorHotspotViewModel(newId);
        Hotspots.Add(newHotspot);
        SelectedHotspot = newHotspot;
    }

    /// <inheritdoc />
    public void DeleteHotspot(IEditorHotspotViewModel hotspot)
    {
        IsSaved = false;

        Hotspots.Remove(hotspot);
        SelectedHotspot = Hotspots.FirstOrDefault();
    }

    /// <inheritdoc />
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        if (SelectedHotspot is null) return;
        IsSaved = false;

        SelectedHotspot?.AddMedia(type, files);
    }

    /// <inheritdoc />
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        if (SelectedHotspot is null) return;
        IsSaved = false;

        var selectedMedia = type switch
        {
            MediaEditorType.Images => ImageEditor.SelectedMedia,
            MediaEditorType.Videos => VideoEditor.SelectedMedia,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        SelectedHotspot?.RemoveMedia(type, media);
        selectedMedia.Clear();
    }

    /// <inheritdoc />
    public bool SaveConfig()
    {
        try
        {
            var hotspots = Hotspots.Select(hotspot => hotspot.ToHotspot());
            var config = new Config(hotspots);

            // Set IsSaved to the result of SaveConfig and return the same value
            return IsSaved = _fileHandler.SaveConfig(config);
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
    }

    /// <inheritdoc />
    public bool ImportConfig(string filePath)
    {
        try
        {
            var config = _fileHandler.ImportConfig(filePath);
            if (config is null) return false;

            Hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>(
                config.Hotspots.Select(hotspot => _vmProvider.GetEditorHotspotViewModel(hotspot))
            );
            SelectedHotspot = Hotspots.FirstOrDefault();

            IsSaved = true;
            return true;
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
    }

    /// <inheritdoc />
    public bool ExportConfig(string exportPath)
    {
        //TODO Implement export functionality (FileHandler does not have export functionality yet)
        return false;
    }

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" />, not linked to any existing <see cref="IConfig" />.
    /// </summary>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    public EditorViewModel(IFileHandler fileHandler, IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
        _fileHandler = fileHandler;
        _hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>();
        DescriptionEditor = vmProvider.GetDescriptionEditorViewModel();
        ImageEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        VideoEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);

        InitEventHandlers();
    }

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> with initial data loaded from the provided <paramref name="config" />.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> providing initial state of the editor.</param>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    public EditorViewModel(IConfig config, IFileHandler fileHandler, IViewModelProvider vmProvider)
    {
        _vmProvider = vmProvider;
        _fileHandler = fileHandler;
        _hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>(
            config.Hotspots.Select(hotspot => _vmProvider.GetEditorHotspotViewModel(hotspot))
        );
        DescriptionEditor = vmProvider.GetDescriptionEditorViewModel();
        ImageEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        VideoEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);
        SelectedHotspot = Hotspots.FirstOrDefault();
        IsSaved = true;

        InitEventHandlers();
    }

    /// <summary>
    /// Initializes event handlers for the editor (to keep track of unsaved changes).
    /// </summary>
    private void InitEventHandlers()
    {
        _hotspots.CollectionChanged += SetUnsaved;
        DescriptionEditor.ContentChanged += SetUnsaved;
        ImageEditor.Media.CollectionChanged += SetUnsaved;
        VideoEditor.Media.CollectionChanged += SetUnsaved;
    }

    /// <summary>
    /// A callback that sets <see cref="IsSaved" /> to <i>false</i>.
    /// </summary>
    private void SetUnsaved(object? sender, EventArgs e)
    {
        IsSaved = false;
    }
}
