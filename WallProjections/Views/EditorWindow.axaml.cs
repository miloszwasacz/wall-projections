using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.Views.EditorUserControls;
using ImportWarningDialog = WallProjections.Views.EditorUserControls.ImportWarningDialog;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

public partial class EditorWindow : Window
{
    private const string ExplorerErrorMessage = "Could not open the File Explorer";
    private const string ImportErrorMessage = "An error occured while importing the configuration file";
    private const string ExportErrorMessage = "An error occured while exporting configuration to a file";
    private const string CalibrationErrorMessage = "An error occured while calibrating the camera";
    private const string SaveErrorMessage = "An error occured while saving the configuration";

    /// <summary>
    /// The path to the warning icon.
    /// </summary>
    private static readonly Uri WarningIconPath = new("avares://WallProjections/Assets/warning-icon.ico");

    //TODO Change to an actual icon
    /// <summary>
    /// The path to the camera calibration icon.
    /// </summary>
    private static readonly Uri CalibrationIconPath = new("avares://WallProjections/Assets/warning-icon.ico");

    /// <summary>
    /// Whether any dialog is currently shown.
    /// </summary>
    private bool _isDialogShown;

    /// <summary>
    /// Whether calibration is currently running.
    /// </summary>
    private bool _isCalibrationRunning;

    public EditorWindow()
    {
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for showing a toast saying that the explorer could not be opened.
    /// </summary>
    private void MediaEditor_OnOpenExplorerFailed(object? sender, RoutedEventArgs e)
    {
        ShowToast(ExplorerErrorMessage);
    }

    /// <summary>
    /// Opens a file picker to import a description from a file.
    /// Then, if the import is <see cref="IImportViewModel.IsImportSafe">safe</see>, the file is imported;
    /// otherwise, a <see cref="ImportWarningDialog">dialog</see> is shown.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void DescriptionEditor_OnImportDescription(object? sender, RoutedEventArgs e)
    {
        // Prevent multiple dialogs from being shown at once
        if (_isDialogShown) return;

        if (DataContext is not IEditorViewModel vm) return;
        var importer = vm.DescriptionEditor.Importer;

        var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a file to import...",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain },
            SuggestedStartLocation = startFolder
        });

        // No file was selected
        if (files.Count == 0) return;

        var file = files[0].Path.LocalPath;
        var safety = importer.IsImportSafe();
        if (safety is ImportWarningLevel.None)
        {
            //TODO Use the result of ImportFromFile to show an error message
            importer.ImportFromFile(file);
            return;
        }

        // Import is not safe
        var dialog = new ImportWarningDialog(file, safety)
        {
            DataContext = importer
        };
        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm the deletion of a hotspot.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void HotspotList_OnDeleteHotspot(object? sender, HotspotList.DeleteArgs e)
    {
        if (_isDialogShown) return;

        if (DataContext is not IEditorViewModel vm) return;

        var dialog = new ConfirmationDialog(
            "Delete Hotspot",
            WarningIconPath,
            "Are you sure you want to delete this hotspot? All associated data will be lost.",
            "Delete"
        );
        dialog.Confirm += (_, _) => vm.DeleteHotspot(e.Hotspot);

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    /// <summary>
    /// Opens a file picker to import images.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void ImageEditor_OnAddMedia(object? sender, RoutedEventArgs e)
    {
        FetchMediaFiles(
            new[] { FilePickerFileTypes.ImageAll },
            WellKnownFolder.Pictures,
            (vm, files) => vm.SelectedHotspot?.AddMedia(MediaEditorType.Images, files)
        );
    }

    /// <summary>
    /// Opens a file picker to import videos.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void VideoEditor_OnAddMedia(object? sender, RoutedEventArgs e)
    {
        FetchMediaFiles(
            new[] { IContentProvider.FilePickerVideoType },
            WellKnownFolder.Videos,
            (vm, files) => vm.AddMedia(MediaEditorType.Videos, files)
        );
    }

    /// <summary>
    /// Removes media from the <see cref="IEditorHotspotViewModel.Images" /> collection.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding the media to remove.</param>
    private void ImagesEditor_OnRemoveMedia(object? sender, MediaEditor.RemoveMediaArgs e)
    {
        RemoveMedia(MediaEditorType.Images, e);
    }

    /// <summary>
    /// Removes media from the <see cref="IEditorHotspotViewModel.Videos" /> collection.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding the media to remove.</param>
    private void VideoEditor_OnRemoveMedia(object? sender, MediaEditor.RemoveMediaArgs e)
    {
        RemoveMedia(MediaEditorType.Videos, e);
    }

    /// <summary>
    /// Opens a file picker to imports a new configuration from a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void ConfigImport_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a configuration file to import...",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Configuration")
                {
                    Patterns = new[] { "*.zip" }, //TODO Change to custom file type
                    AppleUniformTypeIdentifiers = new[] { "public.zip-archive" },
                    MimeTypes = new[] { "application/zip" }
                }
            },
            SuggestedStartLocation = startFolder
        });

        if (files.Count == 0) return;

        var file = files[0].Path.LocalPath;

        var dialog = new ConfirmationDialog(
            "Import Configuration",
            WarningIconPath,
            "Are you sure you want to import a new configuration? All currently saved data will be lost.",
            "Import"
        );
        dialog.Confirm += (_, _) =>
        {
            if (vm.ImportConfig(file)) return;

            // An error occurred while importing
            ShowToast(ImportErrorMessage);
        };

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    /// <summary>
    /// Opens a folder picker to export the current configuration to a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void ConfigExport_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose export location...",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder
        });

        if (folders.Count == 0) return;

        var folder = folders[0].Path.LocalPath;
        if (vm.ExportConfig(folder)) return; //TODO Show a success message

        // An error occurred while exporting
        ShowToast(ExportErrorMessage);
    }

    /// <summary>
    /// Starts camera calibration.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Calibrate_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;
        if (_isCalibrationRunning || _isDialogShown) return;

        _isCalibrationRunning = true;
        vm.ShowCalibrationMarkers();

        var dialog = new ConfirmationDialog(
            "Camera calibration",
            CalibrationIconPath,
            "Ensure that the window with calibration patterns is on the right screen and is in fullscreen mode.",
            "Continue"
        );
        dialog.Confirm += async (_, _) =>
        {
            var success = await vm.CalibrateCamera();
            _isCalibrationRunning = false;
            if (success) return; //TODO Show a success message

            // An error occurred while calibrating
            ShowToast(CalibrationErrorMessage);
        };
        dialog.Cancel += (_, _) =>
        {
            vm.HideCalibrationMarkers();
            _isCalibrationRunning = false;
        };

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    /// <summary>
    /// Saves the current configuration to a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        if (vm.SaveConfig()) return;

        // An error occurred while saving
        ShowToast(SaveErrorMessage);
    }

    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm discarding changes,
    /// and closes the Editor if confirmed.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        if (vm.IsSaved)
        {
            vm.CloseEditor();
            return;
        }

        var dialog = CreateDiscardChangesDialog(vm);

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    private void EditPosition_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        //TODO Show some indication that the PositionEditor is in edit mode
        //TODO Add focus management to Navigator so that focus is automatically set to the PositionEditor

        vm.PositionEditor.IsInEditMode = !vm.PositionEditor.IsInEditMode;
    }

    /// <summary>
    /// Handles key presses:
    /// <ul>
    ///     <li><b>Escape</b>: Exit <see cref="IPositionEditorViewModel.IsInEditMode">edit mode</see> for <see cref="IEditorViewModel.PositionEditor" /></li>
    /// </ul>
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the key that was pressed.</param>
    private void Editor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        if (e.Key == Key.Escape)
            vm.PositionEditor.IsInEditMode = false;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm discarding changes,
    /// when the window is being closed. If Editor's state is <see cref="IEditorViewModel.IsSaved">saved</see>,
    /// then the window is closed immediately.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    /// <remarks>
    /// The window should never be closed using <see cref="Window.Close()" />; instead, the ViewModel's
    /// <see cref="IEditorViewModel.CloseEditor" /> method should be called.
    /// </remarks>
    private async void Editor_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        // The window is being closed programmatically only by ViewModel's Navigator
        if (e.IsProgrammatic) return;

        // Prevent the window from closing
        e.Cancel = true;

        if (vm.IsSaved)
        {
            vm.CloseEditor();
            return;
        }

        var dialog = CreateDiscardChangesDialog(vm);

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    //ReSharper restore UnusedParameter.Local

    /// <summary>
    /// Creates a new <see cref="ConfirmationDialog" /> to discard changes.
    /// </summary>
    private static ConfirmationDialog CreateDiscardChangesDialog(IEditorViewModel vm)
    {
        var dialog = new ConfirmationDialog(
            "Discard Changes",
            WarningIconPath,
            "Are you sure you want to discard your changes? All unsaved data will be lost.",
            "Discard"
        );
        dialog.Confirm += (_, _) => vm.CloseEditor();
        return dialog;
    }

    /// <summary>
    /// Opens a file picker to import media files. Then, performs the <paramref name="action" /> on the selected files.
    /// </summary>
    /// <param name="filter">A file type filter <i>(see <see cref="FilePickerOpenOptions.FileTypeFilter" />)</i>.</param>
    /// <param name="startLocation">
    /// A <see cref="PickerOptions.SuggestedStartLocation">suggested starting location</see> for the file picker
    /// </param>
    /// <param name="action">
    /// An action to perform on the selected files using the viewmodel from the <see cref="EditorWindow.DataContext" />.
    /// </param>
    private async void FetchMediaFiles(
        IReadOnlyList<FilePickerFileType> filter,
        WellKnownFolder startLocation,
        Action<IEditorViewModel, IReadOnlyList<IStorageFile>> action
    )
    {
        if (DataContext is not IEditorViewModel vm) return;

        var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(startLocation);
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select media files to import...",
            AllowMultiple = true,
            FileTypeFilter = filter,
            SuggestedStartLocation = startFolder
        });

        action(vm, files);
    }

    /// <summary>
    /// Removes media from the currently selected hotspot.
    /// </summary>
    /// <param name="type">The type of media to remove.</param>
    /// <param name="args">The event arguments holding the media to remove.</param>
    private async void RemoveMedia(MediaEditorType type, MediaEditor.RemoveMediaArgs args)
    {
        if (_isDialogShown) return;

        if (DataContext is not IEditorViewModel vm) return;

        var media = args.Media;
        var dialog = new ConfirmationDialog(
            $"Remove {type.Name()}",
            WarningIconPath,
            $"Are you sure you want to remove {media.Length} {type.NumberBasedLabel(media.Length)}? This action cannot be undone.",
            "Remove"
        );
        dialog.Confirm += (_, _) => vm.RemoveMedia(type, media);

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    private void ShowToast(string message, Toast.ShowDuration duration = Toast.ShowDuration.Short)
    {
        Toast.Text = message;
        Toast.Show(duration);
    }
}
