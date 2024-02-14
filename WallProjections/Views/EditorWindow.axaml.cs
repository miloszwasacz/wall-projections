using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
using WallProjections.Views.EditorUserControls;
using ImportWarningDialog = WallProjections.Views.EditorUserControls.ImportWarningDialog;

namespace WallProjections.Views;

public partial class EditorWindow : Window
{
    /// <summary>
    /// The path to the warning icon.
    /// </summary>
    private static readonly Uri WarningIconPath = new("avares://WallProjections/Assets/warning-icon.ico");

    /// <summary>
    /// Whether any dialog is currently shown.
    /// </summary>
    private bool _isDialogShown;

    public EditorWindow()
    {
        InitializeComponent();
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// A callback for showing a <see cref="ExplorerErrorToast">toast</see>
    /// saying that the explorer could not be opened.
    /// </summary>
    private void MediaEditor_OnOpenExplorerFailed(object? sender, RoutedEventArgs e)
    {
        ExplorerErrorToast.Show(Toast.ShowDuration.Short);
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

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Select a file to import...",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });

        // No file was selected
        if (files.Count == 0) return;

        var file = files[0].Path.AbsolutePath;
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
            (vm, files) => vm.AddMedia(MediaEditorType.Videos, files)
        );
    }

    /// <summary>
    /// Removes media from the <see cref="IEditorHotspotViewModel.Images" /> collection.
    /// </summary>
    /// <param name="sender"></param>
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
                    MimeTypes = new[] { "application/zip" },
                }
            }
        });

        if (files.Count == 0) return;

        var file = files[0].Path.AbsolutePath;

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
            ImportErrorToast.Show(Toast.ShowDuration.Short);
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

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose export location...",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        var folder = folders[0].Path.AbsolutePath;
        if (vm.ExportConfig(folder)) return;

        // An error occurred while exporting
        ExportErrorToast.Show(Toast.ShowDuration.Short);
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
        SaveErrorToast.Show(Toast.ShowDuration.Short);
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
            Close();
            return;
        }

        var dialog = CreateDiscardChangesDialog();
        dialog.Confirm += (_, _) => Close();

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    /// <summary>
    /// Shows a <see cref="ConfirmationDialog">dialog</see> to confirm discarding changes,
    /// when the window is being closed. If Editor's state is <see cref="IEditorViewModel.IsSaved">saved</see>
    /// or the window is being closed programmatically, then the window is closed immediately.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private async void Editor_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not IEditorViewModel vm) return;

        // If the configuration is saved or the window is being closed programmatically, then it is safe to close.
        if (e.IsProgrammatic || vm.IsSaved) return;

        // Prevent the window from closing
        e.Cancel = true;

        var dialog = CreateDiscardChangesDialog();
        dialog.Confirm += (_, _) => Close();

        _isDialogShown = true;
        await dialog.ShowDialog(this);
        _isDialogShown = false;
    }

    //ReSharper restore UnusedParameter.Local

    /// <summary>
    /// Creates a new <see cref="ConfirmationDialog" /> to discard changes.
    /// </summary>
    private static ConfirmationDialog CreateDiscardChangesDialog() => new(
        "Discard Changes",
        WarningIconPath,
        "Are you sure you want to discard your changes? All unsaved data will be lost.",
        "Discard"
    );

    /// <summary>
    /// Opens a file picker to import media files. Then, performs the <paramref name="action" /> on the selected files.
    /// </summary>
    /// <param name="filter">A file type filter <i>(see <see cref="FilePickerOpenOptions.FileTypeFilter" />)</i>.</param>
    /// <param name="action">
    /// An action to perform on the selected files using the viewmodel from the <see cref="EditorWindow.DataContext" />.
    /// </param>
    private async void FetchMediaFiles(
        IReadOnlyList<FilePickerFileType> filter,
        Action<IEditorViewModel, IReadOnlyList<IStorageFile>> action
    )
    {
        if (DataContext is not IEditorViewModel vm) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a media files to import...",
            AllowMultiple = true,
            FileTypeFilter = filter
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
}
