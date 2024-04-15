using WallProjections.Test.Mocks;
using WallProjections.ViewModels.Display;

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
        imageViewModel.ShowImage(ImagePath);
        Assert.That(imageViewModel.HasImages, Is.True);
        imageViewModel.HideImage();
        Assert.That(imageViewModel.HasImages, Is.False);
    }

    [AvaloniaTest]
    public void DisplayImageTest()
    {
        var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        Assert.Multiple(() =>
            {
                Assert.That(imageViewModel.ShowImage(ImagePath), Is.True);
                Assert.That(imageViewModel.Image, Is.Not.Null);
            }
        );
        imageViewModel.HideImage();
        Assert.That(imageViewModel.Image, Is.Null);
    }

    [AvaloniaTest]
    public void DisplayNonExistentImageTest()
    {
        const string path = "nonexistent.png";
        var imageViewModel = new ImageViewModel(new MockLoggerFactory());
        Assert.That(imageViewModel.ShowImage(path), Is.False);
    }

    //TODO Add tests for throwing exceptions while loading images
}
