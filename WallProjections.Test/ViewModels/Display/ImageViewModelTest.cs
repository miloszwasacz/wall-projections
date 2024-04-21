using WallProjections.Test.Mocks;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class ImageViewModelTest
{
    private static string ImagePath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");

    [AvaloniaTest]
    public void HasImageTest()
    {
        var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        imageViewModel.AddImages(new List<string>{ImagePath});
        Assert.That(imageViewModel.HasImages, Is.True);
        imageViewModel.ClearImages();
        Assert.That(imageViewModel.HasImages, Is.False);
    }

    [AvaloniaTest]
    public void DisplayImageTest()
    {
        var imageViewModel = new ImageViewModel(new MockLoggerFactory());

        Assert.That(imageViewModel.AddImages(new List<string> { ImagePath }), Is.True);
        Assert.That(imageViewModel.Image, Is.Not.Null);
            
        imageViewModel.ClearImages();
        Assert.That(imageViewModel.Image, Is.Null);
    }

    [AvaloniaTest]
    public void DisplayNonExistentImageTest()
    {
        const string path = "nonexistent.png";
        var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        
        Assert.That(imageViewModel.AddImages(new List<string>{path}), Is.False);
    }

    //TODO Add tests for throwing exceptions while loading images
}
