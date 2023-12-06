using Avalonia.Headless.NUnit;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class ImageViewModelTest
{
    private static string ImagePath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "test_image.png");

    [AvaloniaTest]
    public void HasImageTest()
    {
        var imageViewModel = new ImageViewModel();
        imageViewModel.ShowImage(ImagePath);
        Assert.That(imageViewModel.HasImages, Is.True);
        imageViewModel.HideImage();
        Assert.That(imageViewModel.HasImages, Is.False);
    }

    [AvaloniaTest]
    public void DisplayImageTest()
    {
        var imageViewModel = new ImageViewModel();
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
        var imageViewModel = new ImageViewModel();
        Assert.That(imageViewModel.ShowImage(path), Is.False);
    }
}
