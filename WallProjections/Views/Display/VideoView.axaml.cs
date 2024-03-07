using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Views.Display;

public partial class VideoView : UserControl
{
    public VideoView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the video player handle in the viewmodel.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    internal async void OnPlayerLoaded(object? sender, RoutedEventArgs e)
    {
        await Task.Delay(100);
        var viewModel = DataContext as IVideoViewModel;
        var handle = VideoViewer.Handle;
        if (viewModel?.MediaPlayer is null || handle is null) return;

        viewModel.MediaPlayer.SetHandle(handle);
        viewModel.IsLoaded();
    }

    /// <summary>
    /// Resizes the video view to maintain a 16:9 aspect ratio.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the new size of the video view.</param>
    internal void OnVideoViewResize(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            //TODO Don't use hardcoded ratio
            VideoViewer.Height = e.NewSize.Width * 9 / 16;
    }
}
