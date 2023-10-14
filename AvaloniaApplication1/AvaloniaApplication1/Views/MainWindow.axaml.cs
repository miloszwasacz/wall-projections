using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaApplication1.ViewModels;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    // private VideoView videoView;
    // private LibVLC _libVlc;
    // private MediaPlayer _mediaPlayer;

    public MainWindow()
    {
        InitializeComponent();

        // using var libvlc = new LibVLC(enableDebugLogs: true);
        // using var media = new Media(libvlc, new Uri(@"C:\Users\Milosz\GitRepos\AvaloniaApplication1\AvaloniaApplication1\Nalesniki.mp4"));
        // using var mediaplayer = new MediaPlayer(media);
        //
        // mediaplayer.Play();

//         videoView = this.Get<VideoView>("VideoView");
//
//         _libVlc = new LibVLC();
//         _mediaPlayer = new MediaPlayer(_libVlc);
//
//         videoView.MediaPlayer = _mediaPlayer;
//         videoView.MediaPlayer.Hwnd = videoView.hndl.Handle;
//
// #if DEBUG
//         this.AttachDevTools();
// #endif
//
//         videoView.MediaPlayer.Play(
//             new Media(
//                 _libVlc,
//                 @"C:\Users\Milosz\GitRepos\AvaloniaApplication1\AvaloniaApplication1\Nalesniki.mp4",
//                 FromType.FromLocation
//             )
//         );
    }

    private void OnOpened(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            var vm = DataContext as MainWindowViewModel;
            vm?.Play();
        }
    }
}