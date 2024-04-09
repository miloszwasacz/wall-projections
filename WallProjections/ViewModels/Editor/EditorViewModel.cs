using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ReactiveUI;
using WallProjections.Helper;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IEditorViewModel" />
public class EditorViewModel : ViewModelBase, IEditorViewModel
{
    /// <summary>
    /// A <see cref="INavigator" /> used for closing the Editor.
    /// </summary>
    private readonly INavigator _navigator;

    /// <summary>
    /// A <see cref="IViewModelProvider" /> used for creating child viewmodels.
    /// </summary>
    private readonly IViewModelProvider _vmProvider;

    /// <summary>
    /// A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.
    /// </summary>
    private readonly IFileHandler _fileHandler;

    /// <summary>
    /// A <see cref="IPythonHandler" /> used for calibrating the camera.
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <inheritdoc cref="IConfig.HomographyMatrix" />
    private double[,] _homographyMatrix;

    /// <summary>
    /// The backing field for <see cref="Hotspots" />.
    /// </summary>
    private ObservableHotspotCollection<IEditorHotspotViewModel> _hotspots;

    /// <summary>
    /// The backing field for <see cref="SelectedHotspot" />.
    /// </summary>
    private IEditorHotspotViewModel? _selectedHotspot;

    /// <summary>
    /// Whether the config exists, i.e. is not empty
    /// <i>(see <see cref="EditorViewModel(INavigator, IFileHandler, IPythonHandler, IViewModelProvider)" />)</i>.
    /// </summary>
    /// <seealso cref="IsImportSafe" />
    /// <seealso cref="CanExport" />
    private bool _configExists;

    /// <summary>
    /// The backing field for <see cref="IsSaved" />.
    /// </summary>
    private bool _isSaved;

    #region Loading properties' backing fields

    /// <summary>
    /// The backing field for <see cref="IsSaving" />.
    /// </summary>
    private bool _isSaving;

    /// <summary>
    /// The backing field for <see cref="IsImporting" />.
    /// </summary>
    private bool _isImporting;

    /// <summary>
    /// The backing field for <see cref="IsExporting" />.
    /// </summary>
    private bool _isExporting;

    /// <summary>
    /// The backing field for <see cref="IsCalibrating" />.
    /// </summary>
    private bool _isCalibrating;

    /// <summary>
    /// The backing field for <see cref="AreActionsDisabled" />.
    /// </summary>
    private bool _areActionsDisabled;

    #endregion

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
            PositionEditor.SelectHotspot(value, Hotspots.Where(h => h != value).Select(h => h.Position));
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
    public AbsPositionEditorViewModel PositionEditor { get; }

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
            _configExists = true;
            this.RaiseAndSetIfChanged(ref _isSaved, value);
            this.RaisePropertyChanged(nameof(IsImportSafe));
            this.RaisePropertyChanged(nameof(CanExport));
        }
    }

    /// <inheritdoc />
    public bool IsImportSafe => !_configExists;

    /// <inheritdoc />
    public bool CanExport => IsSaved && _configExists;

    #region Loading properties

    /// <inheritdoc />
    public bool IsSaving
    {
        get => _isSaving;
        private set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    /// <inheritdoc />
    public bool IsImporting
    {
        get => _isImporting;
        private set => this.RaiseAndSetIfChanged(ref _isImporting, value);
    }

    /// <inheritdoc />
    public bool IsExporting
    {
        get => _isExporting;
        private set => this.RaiseAndSetIfChanged(ref _isExporting, value);
    }

    /// <inheritdoc />
    public bool IsCalibrating
    {
        get => _isCalibrating;
        private set => this.RaiseAndSetIfChanged(ref _isCalibrating, value);
    }

    /// <inheritdoc />
    public bool AreActionsDisabled
    {
        get => _areActionsDisabled;
        private set => this.RaiseAndSetIfChanged(ref _areActionsDisabled, value);
    }

    /// <inheritdoc />
    public async Task<bool> WithActionLock(Func<Task> action)
    {
        lock (this)
        {
            if (AreActionsDisabled) return false;

            AreActionsDisabled = true;
        }

        try
        {
            await action();
            return true;
        }
        finally
        {
            lock (this)
            {
                AreActionsDisabled = false;
            }
        }
    }

    #endregion

    #region Hotspot management

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

    #endregion

    #region Media management

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

    #endregion

    #region Config manipulation

    /// <inheritdoc />
    public async Task<bool> SaveConfig()
    {
        try
        {
            IsSaving = true;

            var hotspots = Hotspots.Select(hotspot => hotspot.ToHotspot());
            var result = await Task.Run(() =>
            {
                var config = new Config(_homographyMatrix, hotspots);
                return _fileHandler.SaveConfig(config);
            });

            IsSaved = result;
            return result;
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ImportConfig(string filePath)
    {
        try
        {
            IsImporting = true;

            var config = await Task.Run(() => _fileHandler.ImportConfig(filePath));
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
        finally
        {
            IsImporting = false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExportConfig(string exportPath)
    {
        try
        {
            IsExporting = true;

            var path = Path.Combine(exportPath, IEditorViewModel.ExportFileName);
            return await Task.Run(() => _fileHandler.ExportConfig(path));
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            //TODO Improve error reporting (especially for "file already exists")
            return false;
        }
        finally
        {
            IsExporting = false;
        }
    }

    #endregion

    #region Calibration

    /// <inheritdoc />
    public void ShowCalibrationMarkers() => _navigator.ShowCalibrationMarkers();

    /// <inheritdoc />
    public void HideCalibrationMarkers() => _navigator.HideCalibrationMarkers();

    /// <inheritdoc />
    public async Task<bool> CalibrateCamera()
    {
        IsCalibrating = true;

        var arUcoPositions = _navigator.GetArUcoPositions();
        if (arUcoPositions is null) return false;

        var matrix = await Task.Run(() => _pythonHandler.RunCalibration(arUcoPositions));
        if (matrix is not null)
        {
            _homographyMatrix = matrix;
            _configExists = true;
            IsSaved = false;
        }

        _navigator.HideCalibrationMarkers();

        IsCalibrating = false;
        return matrix is not null;
    }

    #endregion

    /// <inheritdoc />
    public void CloseEditor()
    {
        _navigator.CloseEditor();
    }

    #region Constructors and initialization

    /// <summary>
    /// Creates a new empty <see cref="EditorViewModel" />, not linked to any existing <see cref="IConfig" />.
    /// </summary>
    /// <param name="navigator">A <see cref="INavigator" /> used for closing the Editor.</param>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <param name="pythonHandler">A <see cref="IPythonHandler" /> used for calibrating the camera.</param>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    public EditorViewModel(
        INavigator navigator,
        IFileHandler fileHandler,
        IPythonHandler pythonHandler,
        IViewModelProvider vmProvider
    )
    {
        _navigator = navigator;
        _vmProvider = vmProvider;
        _fileHandler = fileHandler;
        _pythonHandler = pythonHandler;
        _homographyMatrix = new double[,]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        };
        _hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>();
        PositionEditor = vmProvider.GetPositionEditorViewModel();
        DescriptionEditor = vmProvider.GetDescriptionEditorViewModel();
        ImageEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        VideoEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);

        _configExists = false;
        _isSaved = true; // Setting the backing field directly to avoid changing _configExists, which is set in IsSaved setter

        InitEventHandlers();
    }

    /// <summary>
    /// Creates a new <see cref="EditorViewModel" /> with initial data loaded from the provided <paramref name="config" />.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> providing initial state of the editor.</param>
    /// <param name="navigator">A <see cref="INavigator" /> used for closing the Editor.</param>
    /// <param name="fileHandler">A <see cref="IFileHandler" /> used for saving, importing, and exporting configs.</param>
    /// <param name="pythonHandler">A <see cref="IPythonHandler" /> used for calibrating the camera.</param>
    /// <param name="vmProvider">A <see cref="IViewModelProvider" /> used for creating child viewmodels.</param>
    public EditorViewModel(
        IConfig config,
        INavigator navigator,
        IFileHandler fileHandler,
        IPythonHandler pythonHandler,
        IViewModelProvider vmProvider
    )
    {
        _navigator = navigator;
        _vmProvider = vmProvider;
        _fileHandler = fileHandler;
        _pythonHandler = pythonHandler;
        _homographyMatrix = config.HomographyMatrix;
        _hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>(
            config.Hotspots.Select(hotspot => _vmProvider.GetEditorHotspotViewModel(hotspot))
        );
        PositionEditor = vmProvider.GetPositionEditorViewModel();
        DescriptionEditor = vmProvider.GetDescriptionEditorViewModel();
        ImageEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Images);
        VideoEditor = vmProvider.GetMediaEditorViewModel(MediaEditorType.Videos);
        SelectedHotspot = Hotspots.FirstOrDefault();

        _configExists = true;
        IsSaved = true;

        InitEventHandlers();
    }

    /// <summary>
    /// Initializes event handlers for the editor (to keep track of unsaved changes).
    /// </summary>
    private void InitEventHandlers()
    {
        _hotspots.CollectionChanged += SetUnsaved;
        PositionEditor.HotspotPositionChanged += SetUnsaved;
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

    #endregion
}
