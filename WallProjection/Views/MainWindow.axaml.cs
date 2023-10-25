using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WallProjection.ViewModels;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Avalonia.Threading;

namespace WallProjection.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer imageTimer = new DispatcherTimer();

    public MainWindow()
    {
        InitializeComponent();
        displayedImage = this.Find<Avalonia.Controls.Image>("displayedImage");
        displayedVideo = this.Find<VideoView>("displayedVideo");
    }

    private void OnOpened(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            PlayVideo(@"S:/uni/coding/2023-WallProjections/WallProjection/Assets/video1.mp4");
        }
        if (e.Key == Key.A)
        {
            ShowImage(@"S:/uni/coding/2023-WallProjections/WallProjection/Assets/image1.jpg", 1.2);
        }
    }

    private void PlayVideo(string path)
    {
        displayedImage.IsVisible = false;
        displayedVideo.IsVisible = true;
        var vm = DataContext as MainWindowViewModel;
        vm?.Play(path);
        
    }

    private void ShowImage(string path, double time)
    {
        imageTimer.Stop();

        displayedVideo.IsVisible = false;
        displayedImage.IsVisible = true;
        var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
        displayedImage.Source = bitmap;

        imageTimer.Interval = TimeSpan.FromSeconds(time);
        imageTimer.Tick += (sender, e) => //what happens when timer runs out
        {
            imageTimer.Stop();
            displayedImage.IsVisible = false;
        };
        imageTimer.Start(); //start new timer
    }
}