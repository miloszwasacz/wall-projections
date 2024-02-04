using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WallProjections.ViewModels.Interfaces.Editor;
using ImportWarningDialog = WallProjections.Views.EditorUserControls.ImportWarningDialog;

namespace WallProjections.Views;

public partial class EditorWindow : Window
{
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

    //ReSharper restore UnusedParameter.Local
}
