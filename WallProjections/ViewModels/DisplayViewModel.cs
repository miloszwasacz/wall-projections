using System.IO;
using System.Linq;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ActivatableViewModelBase, IDisplayViewModel
{
    public DisplayViewModel(IViewModelProvider vmProvider, IFileProvider fileProvider, string artifactId)
    {
        // TODO Refactor to fetch specific file types from FileLocator instead of filtering here
        var files = fileProvider.GetFiles(artifactId);
        Description = files[0];
        var video = files.FirstOrDefault(file => Path.GetExtension(file) == ".mp4");
        if (video is not null)
            VideoViewModel = vmProvider.GetVideoViewModel(video);
    }

    public string Description { get; }
    public IVideoViewModel? VideoViewModel { get; }

    protected override void OnStart()
    {
        VideoViewModel?.PlayVideo();
    }

    protected override void OnStop()
    {
        VideoViewModel?.Dispose();
    }
}
