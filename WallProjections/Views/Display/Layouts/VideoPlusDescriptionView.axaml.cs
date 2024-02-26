using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.Models;
using WallProjections.ViewModels.Display.Layouts;

namespace WallProjections.Views.Display.Layouts;

public partial class VideoPlusDescriptionView : UserControl
{
    private readonly VideoView _videoView;
    private VideoPlusDescriptionViewModel? _viewModel;

    public VideoPlusDescriptionView()
    {
        InitializeComponent();

        _videoView = this.Get<VideoView>("VideoViewer");

        Console.WriteLine("Starting VideoPlusDescriptionView");
    }

    internal async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {

        _viewModel = DataContext as VideoPlusDescriptionViewModel;

        if (_videoView is not null && _viewModel is { VideoViewModel.MediaPlayer: not null })
        {
            Console.WriteLine("Set handle for player");
            _viewModel.VideoViewModel.MediaPlayer.SetHandle(_videoView.Hndl);

            _viewModel.VideoViewModel.PlayVideo(_viewModel.VideoPath);

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
            _videoView.Height = e.NewSize.Width * 9 / 16;
    }

}

