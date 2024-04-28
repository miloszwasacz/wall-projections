using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
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
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

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
    /// The backing field for <see cref="ConfigExists" />.
    /// </summary>
    private bool _configExists;

    /// <summary>
    /// The backing field for <see cref="IsSaved" />.
    /// </summary>
    private bool _isSaved;

    /// <summary>
    /// A reactive backing field for <see cref="IsImportSafe" />.
    /// </summary>
    private readonly ObservableAsPropertyHelper<bool> _isImportSafe;

    /// <summary>
    /// A reactive backing field for <see cref="CanExport" />.
    /// </summary>
    private readonly ObservableAsPropertyHelper<bool> _canExport;

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
            ImageEditor.IsEnabled = value is not null;
            VideoEditor.IsEnabled = value is not null;

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

    /// <summary>
    /// Whether the config exists, i.e. is not empty
    /// <i>(see <see cref="EditorViewModel(INavigator, IFileHandler, IPythonHandler, IViewModelProvider, ILoggerFactory)" />)</i>.
    /// </summary>
    /// <seealso cref="IsImportSafe" />
    /// <seealso cref="CanExport" />
    private bool ConfigExists
    {
        get => _configExists;
        set => this.RaiseAndSetIfChanged(ref _configExists, value);
    }

    /// <inheritdoc />
    public bool IsSaved
    {
        get => _isSaved;
        private set
        {
            ConfigExists = true;
            this.RaiseAndSetIfChanged(ref _isSaved, value);
        }
    }

    /// <inheritdoc />
    public bool IsImportSafe => _isImportSafe.Value;

    /// <inheritdoc />
    public bool CanExport => _canExport.Value;

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
        _logger.LogTrace("Added new hotspot with ID {Id}.", newId);
    }

    /// <inheritdoc />
    public void DeleteHotspot(IEditorHotspotViewModel hotspot)
    {
        IsSaved = false;

        Hotspots.Remove(hotspot);
        SelectedHotspot = Hotspots.FirstOrDefault();
        _logger.LogTrace("Deleted hotspot with ID {Id}.", hotspot.Id);
    }

    #endregion

    #region Media management

    /// <inheritdoc />
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files)
    {
        var hotspot = SelectedHotspot;
        if (hotspot is null)
        {
            _logger.LogWarning("No hotspot selected to add media to.");
            return;
        }

        IsSaved = false;
        hotspot.AddMedia(type, files);

        var logTypeText = type switch
        {
            MediaEditorType.Images => "images",
            MediaEditorType.Videos => "videos",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        _logger.LogTrace("Added {TypeText} to hotspot with ID {Id}.", logTypeText, hotspot.Id);
    }

    /// <inheritdoc />
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media)
    {
        var hotspot = SelectedHotspot;
        if (hotspot is null)
        {
            _logger.LogWarning("No hotspot selected to remove media from.");
            return;
        }

        IsSaved = false;
        var selectedMedia = type switch
        {
            MediaEditorType.Images => ImageEditor.SelectedMedia,
            MediaEditorType.Videos => VideoEditor.SelectedMedia,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        hotspot.RemoveMedia(type, media);
        selectedMedia.Clear();

        var logTypeText = type switch
        {
            MediaEditorType.Images => "images",
            MediaEditorType.Videos => "videos",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        _logger.LogTrace("Removed {TypeText} from hotspot with ID {Id}.", logTypeText, hotspot.Id);
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
            if (result)
                _logger.LogInformation("Config saved successfully.");
            else
                _logger.LogWarning("Failed to save config.");

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save config.");
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
            if (config is null)
            {
                _logger.LogWarning("Failed to import config from {Path}.", filePath);
                return false;
            }

            Hotspots = new ObservableHotspotCollection<IEditorHotspotViewModel>(
                config.Hotspots.Select(hotspot => _vmProvider.GetEditorHotspotViewModel(hotspot))
            );
            SelectedHotspot = Hotspots.FirstOrDefault();

            IsSaved = true;
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import config from {Path}.", filePath);
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

            var result = await Task.Run(() => _fileHandler.ExportConfig(exportPath));
            if (result)
                _logger.LogInformation("Config exported successfully at {Path}.", exportPath);
            else
                _logger.LogWarning("Failed to export config at {Path}.", exportPath);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to export config at {Path}.", exportPath);
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
        try
        {
            IsCalibrating = true;

            var arUcoPositions = _navigator.GetArUcoPositions();
            if (arUcoPositions is null)
            {
                _logger.LogWarning("Failed to calibrate camera: ArUco markers not found.");
                return false;
            }

            var matrix = await Task.Run(() => _pythonHandler.RunCalibration(arUcoPositions));
            if (matrix is not null)
            {
                _homographyMatrix = matrix;
                ConfigExists = true;
                IsSaved = false;
                _logger.LogInformation("Camera calibrated successfully.");
            }
            else
                _logger.LogWarning("Failed to calibrate camera: Python script returned null.");

            _navigator.HideCalibrationMarkers();

            return matrix is not null;
        }
        catch (Exception e)
        {
            _navigator.HideCalibrationMarkers();

            _logger.LogError(e, "Failed to calibrate camera.");
            return false;
        }
        finally
        {
            IsCalibrating = false;
        }
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
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public EditorViewModel(
        INavigator navigator,
        IFileHandler fileHandler,
        IPythonHandler pythonHandler,
        IViewModelProvider vmProvider,
        ILoggerFactory loggerFactory
    )
    {
        _logger = loggerFactory.CreateLogger<EditorViewModel>();
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

        ConfigExists = false;
        _isSaved = true; // Setting the backing field directly to avoid changing _configExists, which is set in IsSaved setter

        _isImportSafe = GetIsImportSafeProperty();
        _canExport = GetCanExportProperty();

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
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public EditorViewModel(
        IConfig config,
        INavigator navigator,
        IFileHandler fileHandler,
        IPythonHandler pythonHandler,
        IViewModelProvider vmProvider,
        ILoggerFactory loggerFactory
    )
    {
        _logger = loggerFactory.CreateLogger<EditorViewModel>();
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

        ConfigExists = true;
        IsSaved = true;

        _isImportSafe = GetIsImportSafeProperty();
        _canExport = GetCanExportProperty();

        InitEventHandlers();
    }

    /// <summary>
    /// Creates a new observable property for <see cref="_isImportSafe" />.
    /// </summary>
    private ObservableAsPropertyHelper<bool> GetIsImportSafeProperty() => this
        .WhenAnyValue(x => x.ConfigExists, exists => !exists)
        .ToProperty(this, x => x.IsImportSafe);

    /// <summary>
    /// Creates a new observable property for <see cref="_canExport" />.
    /// </summary>
    private ObservableAsPropertyHelper<bool> GetCanExportProperty() => this
        .WhenAnyValue(x => x.IsSaved, x => x.ConfigExists, (saved, exists) => saved && exists)
        .ToProperty(this, x => x.CanExport);

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
