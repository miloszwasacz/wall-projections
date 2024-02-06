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

    //ReSharper restore UnusedParameter.Local

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
    private void RemoveMedia(MediaEditorType type, MediaEditor.RemoveMediaArgs args)
    {
        //TODO Add a confirmation dialog
        if (DataContext is not IEditorViewModel vm) return;

        vm.RemoveMedia(type, args.Media);
    }
}
