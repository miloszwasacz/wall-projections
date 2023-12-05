using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

[TestFixture]
public class ImageViewModelTest
{
    private const string ImagePath = "test.png";
    
    [Test]
    public void HasImageTest()
    {
        var imageViewModel = new ImageViewModel();
        imageViewModel.ShowImage(ImagePath);
        Assert.That(imageViewModel.HasImages, Is.True);
        imageViewModel.HideImage();
        Assert.That(imageViewModel.HasImages, Is.False);
    }
    
    [Test]
    public void DisplayImageTest()
    {
        var imageViewModel = new ImageViewModel();
        imageViewModel.ShowImage(ImagePath);
        Assert.That(imageViewModel.Image, Is.Not.Null);
        imageViewModel.HideImage();
        Assert.That(imageViewModel.Image, Is.Null);
    }
    
    [Test]
    public void DisplayNonExistentImageTest()
    {
        var path = "nonexistent.png";
        var imageViewModel = new ImageViewModel();
        imageViewModel.ShowImage(path);
        Assert.That(imageViewModel.HasImages, Is.False);
    }
}
