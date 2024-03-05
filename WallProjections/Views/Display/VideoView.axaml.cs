using System;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.Models;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Views.Display;

public partial class VideoView : UserControl
{

    private readonly NativeVideoView _nativeVideoView;
    private IVideoViewModel _viewModel;

    public VideoView()
    {
        InitializeComponent();
        _nativeVideoView = this.Get<NativeVideoView>("VideoViewer");

    }

    internal async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as VideoViewModel;

        await Task.Delay(500);

        if ( _viewModel?.MediaPlayer != null)
        {
            _viewModel.MediaPlayer.SetHandle(_nativeVideoView.Hndl);
            _viewModel.Loaded();
        }

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
            _nativeVideoView.Height = e.NewSize.Width * 9 / 16;
    }
}

