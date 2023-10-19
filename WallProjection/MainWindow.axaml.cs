using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System.Drawing;

namespace WallProjection;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.ShowImage("./assets/image1.jpg");
    }

    private void ShowImage(string path)
    {
        displayedImage.IsVisible = true;
        var bitmap = new Avalonia.Media.Imaging.Bitmap(@"assets/image1.jpg");
        displayedImage.Source = bitmap;
    }
}