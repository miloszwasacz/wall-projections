using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WallProjection.ViewModels;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace WallProjection.Views;

public partial class MainWindow : Window
{

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
            PlayVideo("C:/Users/danie/Downloads/video1.mp4");
        }
        if (e.Key == Key.A)
        {
            ShowImage("C:/Users/danie/Downloads/image2.jpg");
        }
    }

    private void PlayVideo(string path)
    {
        displayedImage.IsVisible = false;
        displayedVideo.IsVisible = true;
        var vm = DataContext as MainWindowViewModel;
        vm?.Play(path);
        
    }

    private void ShowImage(string path)
    {

        displayedVideo.IsVisible = false;
        displayedImage.IsVisible = true;
        var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
        displayedImage.Source = bitmap;
    }
}