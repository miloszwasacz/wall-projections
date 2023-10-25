using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Interactivity;
using System.Drawing;
using Avalonia.Input;
using LibVLCSharp.Shared;
using LibVLCSharp.Avalonia;

namespace WallProjection;

public partial class MainWindow : Window
{

    private readonly LibVLC _libVlc = new();

    public MainWindow()
    {
        InitializeComponent();
        displayedImage = this.Find<Avalonia.Controls.Image>("displayedImage");
        displayedVideo = this.Find<VideoView>("displayedVideo");
        MediaPlayer = new MediaPlayer(_libVlc);
        displayedVideo.MediaPlayer = MediaPlayer;
        this.KeyDown += KeyDownHandler;
        ShowVideo("S:/uni/coding/2023-WallProjections/WallProjection/bin/x64/Debug/net6.0/assets/video1.mp4");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void KeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (sender == null)
        {
            throw new ArgumentNullException(nameof(sender));
        }

        switch (e.Key)
        {
            case Key.A:
                this.ShowImage("assets/image1.jpg");
                break;
            case Key.B:
                this.ShowVideo("S:/uni/coding/2023-WallProjections/WallProjection/bin/x64/Debug/net6.0/assets/video1.mp4");
                break;
        }
    }


    private void ShowImage(string path)
    {
        try
        {
            displayedVideo.MediaPlayer.Stop();
            displayedVideo.IsVisible = false;
            displayedImage.IsVisible = true;
            var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
            displayedImage.Source = bitmap;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error displaying image at path {path}: {e.Message}");
        }
    }

    private void ShowVideo(string path)
    {
        try
        {
            displayedImage.IsVisible = false;
            displayedVideo.IsVisible = true;
            using var media = new Media(
                _libVlc,
                new Uri(path)
            );
            displayedVideo.MediaPlayer.Play(media);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error displaying image at path {path}: {e.Message}");
        }
    }

    public MediaPlayer MediaPlayer { get; }
}