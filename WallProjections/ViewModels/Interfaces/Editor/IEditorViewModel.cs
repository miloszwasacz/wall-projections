using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using WallProjections.Helper;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.Views.Editor;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for editing data about a <see cref="IConfig" />.
/// </summary>
public interface IEditorViewModel
{
    //TODO Change to use a custom extension
    /// <summary>
    /// The name of the file to export the config to.
    /// </summary>
    public const string ExportFileName = "WallProjectionsConfig.zip";

    /// <summary>
    /// A collection of all hotspots in the editor.
    /// </summary>
    public ObservableHotspotCollection<IEditorHotspotViewModel> Hotspots { get; set; }

    /// <summary>
    /// The currently selected hotspot (or <i>null</i> if <see cref="Hotspots" /> is empty).
    /// </summary>
    public IEditorHotspotViewModel? SelectedHotspot { get; set; }

    /// <summary>
    /// A <see cref="AbsPositionEditorViewModel" /> for editing the position of the currently selected hotspot.
    /// </summary>
    public AbsPositionEditorViewModel PositionEditor { get; }

    /// <summary>
    /// A <see cref="IDescriptionEditorViewModel" /> for editing the title and description of the currently selected hotspot.
    /// </summary>
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    /// <summary>
    /// A <see cref="IMediaEditorViewModel" /> for managing images.
    /// </summary>
    public IMediaEditorViewModel ImageEditor { get; }

    /// <summary>
    /// A <see cref="IMediaEditorViewModel" /> for managing videos.
    /// </summary>
    public IMediaEditorViewModel VideoEditor { get; }

    /// <summary>
    /// Whether the current state of the viewmodel has been saved.
    /// </summary>
    /// <seealso cref="CanExport" />
    /// <seealso cref="CloseButtonText" />
    public bool IsSaved { get; }

    /// <summary>
    /// The text of the button that closes the editor.
    /// </summary>
    /// <returns>"Close" if <see cref="IsSaved" /> is <i>true</i>; "Discard" otherwise.</returns>
    public string CloseButtonText => IsSaved ? "Close" : "Discard";

    /// <summary>
    /// Whether there is no loaded config, so it is safe to import a new one.
    /// </summary>
    public bool IsImportSafe { get; }

    /// <summary>
    /// Whether the current config can be exported (i.e. exists and has been saved).
    /// </summary>
    /// <seealso cref="IsSaved" />
    public bool CanExport { get; }

    #region Loading properties

    /// <summary>
    /// Whether the editor is currently saving the config.
    /// </summary>
    public bool IsSaving { get; }

    /// <summary>
    /// Whether the editor is currently importing a config from a file.
    /// </summary>
    public bool IsImporting { get; }

    /// <summary>
    /// Whether the editor is currently exporting the config to a file.
    /// </summary>
    public bool IsExporting { get; }

    /// <summary>
    /// Whether the editor is currently calibrating the camera.
    /// </summary>
    public bool IsCalibrating { get; }

    /// <summary>
    /// Whether the editor is currently performing any action that disables other actions
    /// (i.e. saving, importing, exporting, or calibrating).
    /// </summary>
    public bool AreActionsDisabled { get; }

    /// <summary>
    /// Tries to acquire the action lock, preventing other actions from being performed.
    /// </summary>
    /// <returns>Whether the lock was acquired.</returns>
    /// <seealso cref="ReleaseActionLock" />
    /// <seealso cref="AreActionsDisabled" />
    public bool TryAcquireActionLock();

    /// <summary>
    /// Releases the action lock, allowing other actions to be performed.
    /// </summary>
    /// <seealso cref="TryAcquireActionLock" />
    /// <seealso cref="AreActionsDisabled" />
    public void ReleaseActionLock();

    /// <summary>
    /// Performs the given <paramref name="action" /> while ignoring any attempts to perform other actions.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>Whether the lock was acquired and action was performed successfully.</returns>
    /// <seealso cref="AreActionsDisabled" />
    public Task<bool> WithActionLock(Func<Task> action);

    #endregion

    #region Hotspot management

    /// <summary>
    /// Adds a new hotspot and selects it.
    /// </summary>
    public void AddHotspot();

    /// <summary>
    /// Deletes the given hotspot.
    /// </summary>
    /// <param name="hotspot">The hotspot to delete.</param>
    public void DeleteHotspot(IEditorHotspotViewModel hotspot);

    #endregion

    #region Media management

    /// <summary>
    /// Adds media of the given <paramref name="type" /> to <see cref="SelectedHotspot" />.
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to add.</param>
    /// <param name="files">
    /// An iterator of image files that are converted to <see cref="IThumbnailViewModel" />s and added to the hotspot.
    /// </param>
    public void AddMedia(MediaEditorType type, IEnumerable<IStorageFile> files);

    /// <summary>
    /// Removes appropriate media from <see cref="SelectedHotspot" />.
    /// </summary>
    /// <param name="type">The <see cref="MediaEditorType">type</see> of media to remove.</param>
    /// <param name="media">The media to remove from the hotspot.</param>
    public void RemoveMedia(MediaEditorType type, IEnumerable<IThumbnailViewModel> media);

    #endregion

    #region Config manipulation

    /// <summary>
    /// Creates a new <see cref="IConfig" /> using the currently stored data in this <see cref="IEditorViewModel" />
    /// and saves it to the file system.
    /// </summary>
    /// <returns>Whether the file was saved successfully.</returns>
    public Task<bool> SaveConfig();

    /// <summary>
    /// Imports a configuration from a file and overwrites the currently saved configuration.
    /// </summary>
    /// <param name="filePath">The path to the configuration file to import.</param>
    /// <returns>Whether the file was imported successfully.</returns>
    public Task<bool> ImportConfig(string filePath);

    /// <summary>
    /// Exports the current configuration to a file at the specified path.
    /// </summary>
    /// <param name="exportPath">A path to a folder where the configuration will be exported.</param>
    /// <returns>Whether the file was exported successfully.</returns>
    public Task<bool> ExportConfig(string exportPath);

    #endregion

    #region Calibration

    /// <summary>
    /// Shows the calibration markers on the secondary display.
    /// </summary>
    /// <remarks>This method has to be called on the UI thread.</remarks>
    public void ShowCalibrationMarkers();

    /// <summary>
    /// Hides the calibration markers if they are currently visible.
    /// </summary>
    /// <remarks>This method has to be called on the UI thread.</remarks>
    public void HideCalibrationMarkers();

    /// <summary>
    /// Calibrates the camera, sets the current homography matrix,
    /// and <see cref="HideCalibrationMarkers">hides the calibration markers</see>.
    /// </summary>
    /// <returns>Whether the calibration has been successful.</returns>
    /// <remarks>
    /// This method has to be called on the UI thread.<br />
    /// For the calibration to be successful,
    /// <see cref="ShowCalibrationMarkers">ArUco markers must be visible on the secondary display</see>.
    /// </remarks>
    public Task<bool> CalibrateCamera();

    #endregion

    /// <summary>
    /// Closes the <see cref="EditorWindow">Editor</see> and discards any unsaved changes.
    /// </summary>
    public void CloseEditor();
}
