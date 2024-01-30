using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WallProjections.Views;

public partial class EditorWindow : Window
{
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
}
