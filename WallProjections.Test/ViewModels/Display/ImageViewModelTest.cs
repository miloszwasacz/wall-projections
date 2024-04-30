using Avalonia.Media.Imaging;
using WallProjections.Test.Mocks;
using WallProjections.ViewModels.Display;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class ImageViewModelTest
{
    private static string ImagePath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");

    private static string ImagePath2 =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image_2.jpg");

    [AvaloniaTest]
    public void HasImageTest()
    {
        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        imageViewModel.AddImages(new List<string>{ImagePath});
        Assert.That(imageViewModel.HasImages, Is.True);
        imageViewModel.ClearImages();
        Assert.That(imageViewModel.HasImages, Is.False);
    }

    [AvaloniaTest]
    public void DisplayImageTest()
    {
        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        Assert.That(imageViewModel.AddImages(new List<string> { ImagePath }), Is.True);
        Assert.That(imageViewModel.Image, Is.Not.Null);
            
        imageViewModel.ClearImages();
        Assert.That(imageViewModel.Image, Is.Null);
    }
    
    [AvaloniaTest]
    public void DisplayTwoImagesTest()
    {
        #region Setup imageViewModel
        
        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        var stream = File.OpenRead(ImagePath);
        var image1 = new Bitmap(stream);
        stream.Close();

        stream = File.OpenRead(ImagePath2);
        var image2 = new Bitmap(stream);
        stream.Close();
        
        Assert.That(imageViewModel.AddImages(new List<string> { ImagePath, ImagePath2 }), Is.True);
        Assert.That(imageViewModel.Image, Is.Not.Null);
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));
        
        #endregion
        
        imageViewModel.StartSlideshow(TimeSpan.FromSeconds(0.5));
        
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));

        // Check second image is shown.
        Task.Delay(TimeSpan.FromSeconds(0.6)).Wait();
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image2.ToString()));

        // Check image rotates back to first image.
        Task.Delay(TimeSpan.FromSeconds(0.6)).Wait();
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));
    }

    [AvaloniaTest]
    public void StopSlideshowTest()
    {
        #region Setup imageViewModel

        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        var stream = File.OpenRead(ImagePath);
        var image1 = new Bitmap(stream);
        stream.Close();
        
        Assert.That(imageViewModel.AddImages(new List<string> { ImagePath, ImagePath2 }), Is.True);
        Assert.That(imageViewModel.Image, Is.Not.Null);
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));

        #endregion
        
        imageViewModel.StartSlideshow(TimeSpan.FromSeconds(0.5));
        
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));
        
        imageViewModel.StopSlideshow();

        Task.Delay(TimeSpan.FromSeconds(0.7)).Wait();
        
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));
    }

    [AvaloniaTest]
    public void ClearImagesTest()
    {
        #region Setup imageViewModel

        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        var stream = File.OpenRead(ImagePath);
        var image1 = new Bitmap(stream);
        stream.Close();
        
        stream = File.OpenRead(ImagePath2);
        var image2 = new Bitmap(stream);
        stream.Close();
        
        Assert.That(imageViewModel.AddImages(new List<string> { ImagePath, ImagePath2 }), Is.True);
        Assert.That(imageViewModel.Image, Is.Not.Null);
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image1.ToString()));

        #endregion
        
        imageViewModel.StartSlideshow(TimeSpan.FromSeconds(0.5));

        Task.Delay(TimeSpan.FromSeconds(0.6)).Wait();
        
        Assert.That(imageViewModel.Image?.ToString(), Is.EqualTo(image2.ToString()));
        
        imageViewModel.ClearImages();
        
        Assert.That(imageViewModel.HasImages, Is.False);
        Assert.That(imageViewModel.ImageCount, Is.EqualTo(0));
        Assert.That(imageViewModel.Image, Is.Null);
    }
    
    [AvaloniaTest]
    public void DisplayNonExistentImageTest()
    {
        const string path = "nonexistent.png";
        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        
        Assert.That(imageViewModel.AddImages(new List<string>{path}), Is.False);
        Assert.That(imageViewModel.HasImages, Is.False);
    }

    [AvaloniaTest]
    public void NonExistentWithRealImageTest()
    {
        var realPath = ImagePath;
        const string fakePath = "nonexistent.png";

        using var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        // Only 1 of the 2 images should be added.
        Assert.That(imageViewModel.AddImages(new List<string>{ realPath, fakePath }), Is.False);
        Assert.That(imageViewModel.HasImages, Is.True);
        Assert.That(imageViewModel.ImageCount, Is.EqualTo(1));
    }

    //TODO Add tests for throwing exceptions while loading images
}
