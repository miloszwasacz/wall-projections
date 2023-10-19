using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Drawing;

namespace WallProjection;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.ShowImage(@"assets/image1.jpg");
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
            Console.Error.WriteLine($"Error displaying image at path {path}: {e.Message});
        }
    }
}