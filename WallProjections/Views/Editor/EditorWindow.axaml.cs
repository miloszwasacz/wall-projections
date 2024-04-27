using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using static WallProjections.Views.ConfirmationDialog;

namespace WallProjections.Views.Editor;

[PseudoClasses(":hasoverlay")]
public partial class EditorWindow : Window
{
    private const string ImportInProgressMessage = "Importing configuration...";
    private const string ExportInProgressMessage = "Exporting configuration...";
    private const string CalibrationInProgressMessage = "Calibrating camera...";
    private const string SaveInProgressMessage = "Saving configuration...";

    private const string ImportSuccessMessage = "Configuration imported successfully";
    private const string ExportSuccessMessage = "Configuration exported successfully";
    private const string CalibrationSuccessMessage = "Camera calibration successful";

    private const string ExplorerErrorMessage = "Could not open the File Explorer";
    private const string ImportErrorMessage = "An error occured while importing the configuration file";
    private const string ExportErrorMessage = "An error occured while exporting configuration to a file";
    private const string CalibrationErrorMessage = "An error occured while calibrating the camera";
    private const string SaveErrorMessage = "An error occured while saving the configuration";

    private const string ImportGenericWarningMessage =
        "Importing this file will overwrite the current data.\n\nAre you sure you want to continue?";

    /// <summary>
    /// The path to the warning icon.
    /// </summary>
    private static readonly Uri WarningIconPath = new("avares://WallProjections/Assets/warning-icon.ico");

    /// <summary>
    /// The path to the camera calibration icon.
    /// </summary>
    private static readonly Uri CalibrationIconPath = new("avares://WallProjections/Assets/webcam-icon.ico");

    /// <summary>
    /// The backing field for the <see cref="IsInPositionEditMode" /> property.
    /// </summary>
    private bool _isInPositionEditMode;

    // /// <summary>
    // /// Whether a dialog is currently open.
    // /// </summary>
    // private bool _hasOpenDialog;

    /// <summary>
    /// A <see cref="DirectProperty{TOwner,TValue}">DirectProperty</see> that defines the <see cref="IsInPositionEditMode" /> property.
    /// </summary>
    protected static readonly DirectProperty<EditorWindow, bool> IsInPositionEditModeProperty =
        AvaloniaProperty.RegisterDirect<EditorWindow, bool>(
            nameof(IsInPositionEditMode),
            o => o.IsInPositionEditMode,
            (o, v) => o.IsInPositionEditMode = v
        );

    /// <summary>
    /// Whether the PositionEditor is in edit mode.
    /// </summary>
    public bool IsInPositionEditMode
    {
        get => _isInPositionEditMode;
        set
        {
            SetAndRaise(IsInPositionEditModeProperty, ref _isInPositionEditMode, value);

            if (PositionEditorToggle is null) return;
            if (value)
                FlyoutBase.ShowAttachedFlyout(PositionEditorToggle);
            else
                FlyoutBase.GetAttachedFlyout(PositionEditorToggle)?.Hide();
        }
    }

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
        ShowToast(ExplorerErrorMessage, NotificationType.Error);
    }

    /// <summary>
    /// Opens a file picker to import a description from a file.
    /// Then, if the import is <see cref="IImportViewModel.IsImportSafe">safe</see>, the file is imported;
    /// otherwise, a warning dialog is shown.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void DescriptionEditor_OnImportDescription(object? sender, RoutedEventArgs e) =>
        await WithActionLock(async vm =>
        {
            var importer = vm.DescriptionEditor.Importer;

            var (result, file) = await WithOverlay(async () =>
            {
                var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select a text file to import...",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { FilePickerFileTypes.TextPlain },
                    SuggestedStartLocation = startFolder
                });

                // No file was selected
                if (files.Count == 0) return (Result.Cancelled, "");

                var file = files[0].Path.LocalPath;
                var safety = importer.IsImportSafe();
                if (safety is ImportWarningLevel.None)
                    return (Result.Confirmed, file);

                // Import is not safe
                var result = await new ConfirmationDialog(
                    "Data overwrite warning",
                    WarningIconPath,
                    $"{safety.ToWarningText()} {ImportGenericWarningMessage}",
                    "Import anyway"
                ).ShowDialog(this);

                return (result, file);
            });

            //TODO Use the result of ImportFromFile to show an error message
            if (result == Result.Confirmed)
                importer.ImportFromFile(file);
        });

    /// <summary>
    /// Adds a new hotspot.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void HotspotList_OnAddHotspot(object? sender, EventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        vm.AddHotspot();
    }

    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm the deletion of a hotspot.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void HotspotList_OnDeleteHotspot(object? sender, HotspotList.DeleteArgs e) =>
        await WithActionLock(async vm =>
        {
            var result = await WithOverlay(() =>
                new ConfirmationDialog(
                    "Delete Hotspot",
                    WarningIconPath,
                    "Are you sure you want to delete this hotspot? All associated data will be lost.",
                    "Delete"
                ).ShowDialog(this)
            );

            if (result == Result.Confirmed)
                vm.DeleteHotspot(e.Hotspot);
        });

    /// <summary>
    /// Opens a file picker to import images.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void ImageEditor_OnAddMedia(object? sender, RoutedEventArgs e) => FetchMediaFiles(
        new[] { FilePickerFileTypes.ImageAll },
        WellKnownFolder.Pictures,
        (vm, files) => vm.SelectedHotspot?.AddMedia(MediaEditorType.Images, files)
    );

    /// <summary>
    /// Opens a file picker to import videos.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void VideoEditor_OnAddMedia(object? sender, RoutedEventArgs e) => FetchMediaFiles(
        new[] { IContentProvider.FilePickerVideoType },
        WellKnownFolder.Videos,
        (vm, files) => vm.AddMedia(MediaEditorType.Videos, files)
    );


    /// <summary>
    /// Removes media from the <see cref="IEditorHotspotViewModel.Images" /> collection.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding the media to remove.</param>
    private void ImagesEditor_OnRemoveMedia(object? sender, MediaEditor.RemoveMediaArgs e) =>
        RemoveMedia(MediaEditorType.Images, e);

    /// <summary>
    /// Removes media from the <see cref="IEditorHotspotViewModel.Videos" /> collection.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments holding the media to remove.</param>
    private void VideoEditor_OnRemoveMedia(object? sender, MediaEditor.RemoveMediaArgs e) =>
        RemoveMedia(MediaEditorType.Videos, e);

    /// <summary>
    /// Opens a file picker to imports a new configuration from a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void ConfigImport_OnClick(object? sender, RoutedEventArgs e) => await WithActionLock(async vm =>
    {
        var (result, file) = await WithOverlay(async () =>
        {
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

            if (files.Count == 0) return (Result.Cancelled, "");

            var file = files[0].Path.LocalPath;

            // If the import is safe, don't show a warning dialog
            if (vm.IsImportSafe)
                return (Result.Confirmed, file);

            var result = await new ConfirmationDialog(
                "Import Configuration",
                WarningIconPath,
                "Are you sure you want to import a new configuration? All currently saved data will be lost.",
                "Import"
            ).ShowDialog(this);

            return (result, file);
        });

        if (result == Result.Confirmed)
            await Import(file);
        return;

        async Task Import(string configFile)
        {
            var success = await WithLoadingToast(ImportInProgressMessage, () => vm.ImportConfig(configFile));
            ShowToast(
                success ? ImportSuccessMessage : ImportErrorMessage,
                success ? NotificationType.Success : NotificationType.Error
            );
        }
    });

    /// <summary>
    /// Opens a folder picker to export the current configuration to a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void ConfigExport_OnClick(object? sender, RoutedEventArgs e) => await WithActionLock(async vm =>
    {
        var file = await WithOverlay(async () =>
        {
            var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            return await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Choose export location...",
                SuggestedStartLocation = startFolder,
                SuggestedFileName = IEditorViewModel.ExportFileName,
                ShowOverwritePrompt = true
            });
        });

        // No file selected
        if (file is null) return;

        var path = file.Path.LocalPath;
        var success = await WithLoadingToast(ExportInProgressMessage, () => vm.ExportConfig(path));
        ShowToast(
            success ? ExportSuccessMessage : ExportErrorMessage,
            success ? NotificationType.Success : NotificationType.Error
        );
    });

    /// <summary>
    /// Starts camera calibration.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Calibrate_OnClick(object? sender, RoutedEventArgs e) => await WithActionLock(async vm =>
    {
        vm.ShowCalibrationMarkers();

        var result = await WithOverlay(() =>
            new ConfirmationDialog(
                "Camera calibration",
                CalibrationIconPath,
                "Ensure that the window with calibration patterns is on the correct screen and is in fullscreen mode.",
                "Continue"
            ).ShowDialog(this)
        );

        if (result == Result.Confirmed)
        {
            var success = await WithLoadingToast(CalibrationInProgressMessage, vm.CalibrateCamera);
            ShowToast(
                success ? CalibrationSuccessMessage : CalibrationErrorMessage,
                success ? NotificationType.Success : NotificationType.Error
            );
        }
        else
            vm.HideCalibrationMarkers();
    });

    /// <summary>
    /// Saves the current configuration to a file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Save_OnClick(object? sender, RoutedEventArgs e) => await WithActionLock(Save);

    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm discarding changes,
    /// and closes the Editor if confirmed.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        await CloseEditor(vm);
    }

    /// <summary>
    /// Toggles the <see cref="AbsPositionEditorViewModel.IsInEditMode">edit mode</see> for the PositionEditor.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void EditPosition_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        vm.PositionEditor.IsInEditMode = !vm.PositionEditor.IsInEditMode;
    }

    /// <summary>
    /// Handles key presses:
    /// <ul>
    ///     <li>
    ///         <b>Escape</b>: Exit <see cref="AbsPositionEditorViewModel.IsInEditMode">edit mode</see>
    ///                        for <see cref="IEditorViewModel.PositionEditor" />
    ///     </li>
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

        await CloseEditor(vm);
    }

    //ReSharper restore UnusedParameter.Local

    /// <summary>
    /// Saves the Editor's state while showing a loading toast. Shows an error toast if the save fails.
    /// </summary>
    /// <param name="vm">The Editor's viewmodel</param>
    /// <returns>Whether the save was successful</returns>
    /// <seealso cref="IEditorViewModel.SaveConfig" />
    private async Task<bool> Save(IEditorViewModel vm)
    {
        var success = await WithLoadingToast(SaveInProgressMessage, vm.SaveConfig);

        // An error occurred while saving
        if (!success)
            ShowToast(SaveErrorMessage, NotificationType.Error);

        return success;
    }

    /// <summary>
    /// Closes the Editor if the viewmodel's state is saved, otherwise shows a warning dialog.
    /// </summary>
    /// <param name="vm">The Editor's viewmodel</param>
    /// <seealso cref="IEditorViewModel.IsSaved" />
    /// <seealso cref="IEditorViewModel.CloseEditor" />
    private async Task CloseEditor(IEditorViewModel vm) => await vm.WithActionLock(async () =>
    {
        if (vm.IsSaved)
        {
            vm.CloseEditor();
            return;
        }

        await ShowUnsavedChangesDialog(vm);
    });

    /// <summary>
    /// Shows a new <see cref="ConfirmationDialog" /> to discard changes.
    /// The user can choose to save the changes, discard them, or cancel the action.
    /// If the user chooses to either save or discard the changes, the Editor is closed afterwards
    /// (if the save is successful).
    /// </summary>
    private async Task ShowUnsavedChangesDialog(IEditorViewModel vm)
    {
        var result = await WithOverlay(() =>
            new ConfirmationDialog(
                "Unsaved Changes",
                WarningIconPath,
                "It appears that you have unsaved changes. Do you want save them before closing?",
                "Save",
                "Discard"
            ).ShowDialog(this)
        );

        switch (result)
        {
            case Result.Confirmed:
                if (await Save(vm))
                    vm.CloseEditor();
                break;
            case Result.Refused:
                vm.CloseEditor();
                break;
            case Result.Cancelled:
            default:
                break;
        }
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
    ) => await WithActionLock(async vm =>
    {
        var startFolder = await StorageProvider.TryGetWellKnownFolderAsync(startLocation);
        var files = await WithOverlay(() => StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select media files to import...",
            AllowMultiple = true,
            FileTypeFilter = filter,
            SuggestedStartLocation = startFolder
        }));

        action(vm, files);
    });

    /// <summary>
    /// Removes media from the currently selected hotspot.
    /// </summary>
    /// <param name="type">The type of media to remove.</param>
    /// <param name="args">The event arguments holding the media to remove.</param>
    private async void RemoveMedia(
        MediaEditorType type,
        MediaEditor.RemoveMediaArgs args
    ) => await WithActionLock(async vm =>
    {
        var media = args.Media;
        var result = await WithOverlay(() =>
            new ConfirmationDialog(
                $"Remove {type.Name()}",
                WarningIconPath,
                $"Are you sure you want to remove {media.Length} {type.NumberBasedLabel(media.Length)}? " +
                $"This action cannot be undone.",
                "Remove"
            ).ShowDialog(this)
        );

        if (result == Result.Confirmed)
            vm.RemoveMedia(type, media);
    });

    /// <summary>
    /// Shows a toast with the specified message.
    /// </summary>
    /// <param name="message">The text to show in the toast.</param>
    /// <param name="type">The type of the notification.</param>
    /// <param name="duration">The duration for which the toast should be shown.</param>
    private void ShowToast(
        string message,
        NotificationType type,
        Toast.ShowDuration duration = Toast.ShowDuration.Short
    )
    {
        Toast.Text = message;
        Toast.Show(duration, type);
    }

    /// <summary>
    /// Performs an action while showing an overlay.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <typeparam name="T">The return type of the action.</typeparam>
    /// <returns>The result of the action.</returns>
    private async Task<T> WithOverlay<T>(Func<Task<T>> action)
    {
        PseudoClasses.Add(":hasoverlay");
        var result = await action();
        PseudoClasses.Remove(":hasoverlay");
        return result;
    }

    /// <summary>
    /// Shows a loading toast with the specified message while performing an action.
    /// </summary>
    /// <param name="message">The message to show in the loading toast.</param>
    /// <param name="action">The action to perform.</param>
    /// <typeparam name="T">The return type of the action.</typeparam>
    /// <returns>The result of the action.</returns>
    private async Task<T> WithLoadingToast<T>(string message, Func<Task<T>> action)
    {
        LoadingToast.Text = message;
        await Task.Delay(10);
        LoadingToast.Show();
        var result = await action();
        LoadingToast.Hide();
        return result;
    }

    /// <summary>
    /// Calls <see cref="IEditorViewModel.WithActionLock" /> on <see cref="EditorWindow.DataContext" />
    /// with the specified action given the current DataContext.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    private async Task WithActionLock(Func<IEditorViewModel, Task> action)
    {
        if (DataContext is not IEditorViewModel vm) return;

        await vm.WithActionLock(() => action(vm));
    }
}
