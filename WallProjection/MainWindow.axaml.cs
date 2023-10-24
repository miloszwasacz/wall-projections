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

    /*private readonly LibVLC _libVlc = new();*/

    public MainWindow()
    {
        InitializeComponent();
        displayedImage = this.Find<Avalonia.Controls.Image>("displayedImage");
        displayedVideo = this.Find<VideoView>("displayedVideo");
        this.KeyDown += KeyDownHandler;
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
                this.ShowImage("assets/image2.jpg");
                break;
        }
    }


    private void ShowImage(string path)
    {
        try
        {
            displayedImage.IsVisible = true;
            var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
            displayedImage.Source = bitmap;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error displaying image at path {path}: {e.Message}");
        }
    }
}