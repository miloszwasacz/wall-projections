using WallProjections.Test.Mocks;
using WallProjections.ViewModels;

namespace WallProjections.Test.ViewModels;

//TODO Improve tests once DisplayViewModel is refactored
[TestFixture]
public class DisplayViewModelTest
{
    private const string ArtifactId = "1";
    private const string TextPath = "test.txt";
    private const string VideoPath = "test.mp4";
    private static string[] Files => new [] {TextPath, VideoPath};
    private static string[] FilesNoVideo => new [] {TextPath};
    private static MockViewModelProvider ViewModelProvider => new();
    private static AssertionException MockException => new("VideoViewModel is not a MockVideoViewModel");

    [Test]
    public void CreationTest()
    {
        var fileProvider = new MockFileProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, fileProvider, ArtifactId);

        Assert.Multiple(() =>
        {
            //TODO Add proper tests for FileProvider once it is refactored
            Assert.That(fileProvider.FileNumber, Is.EqualTo(ArtifactId));

            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.EqualTo(TextPath));

            var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
            Assert.That(videoViewModel.VideoPath, Is.EqualTo(VideoPath));
        });
    }

    [Test]
    public void CreationNoVideoTest()
    {
        var fileProvider = new MockFileProvider(FilesNoVideo);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, fileProvider, ArtifactId);

        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.VideoViewModel, Is.Null);
            //TODO Add proper tests for the Description once DisplayViewModel is refactored
            Assert.That(displayViewModel.Description, Is.EqualTo(TextPath));
        });
    }

    [Test]
    public void ActivationTest()
    {
        var fileProvider = new MockFileProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, fileProvider, ArtifactId);
        var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(0));
        });

        // Deactivate the viewmodel
        disposable.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(1));
        });

        // Activate the viewmodel again
        disposable = displayViewModel.Activator.Activate();
        var videoViewModel2 = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
        Assert.Multiple(() =>
        {
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(1));

            Assert.That(videoViewModel2.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel2.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel2.DisposeCounter, Is.EqualTo(0));
        });
        disposable.Dispose();
    }

    [Test]
    public void ActivationNoVideoTest()
    {
        var fileProvider = new MockFileProvider(FilesNoVideo);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, fileProvider, ArtifactId);

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);

        // Deactivate the viewmodel
        disposable.Dispose();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);

        // Activate the viewmodel again
        disposable = displayViewModel.Activator.Activate();
        Assert.That(displayViewModel.VideoViewModel, Is.Null);
        disposable.Dispose();
    }


    [Test]
    public void ActivationNoMediaPlayerTest()
    {
        var fileProvider = new MockFileProvider(Files);
        var displayViewModel = new DisplayViewModel(ViewModelProvider, fileProvider, ArtifactId);
        var videoViewModel = displayViewModel.VideoViewModel as MockVideoViewModel ?? throw MockException;
        videoViewModel.CanPlay = false;

        // Activate the viewmodel
        var disposable = displayViewModel.Activator.Activate();
        Assert.Multiple(() =>
        {
            Assert.That(displayViewModel.VideoViewModel, Is.Not.Null);
            Assert.That(videoViewModel.PlayCounter, Is.EqualTo(1));
            Assert.That(videoViewModel.StopCounter, Is.EqualTo(0));
            Assert.That(videoViewModel.DisposeCounter, Is.EqualTo(0));
        });
        disposable.Dispose();
    }
}
