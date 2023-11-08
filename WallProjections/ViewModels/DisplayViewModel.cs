using System.IO;
using System.Linq;
using WallProjections.Models;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ActivatableViewModelBase
{
    public DisplayViewModel(string fileNumber)
    {
        // TODO Refactor to fetch specific file types from FileLocator instead of filtering here
        var files = FileLocator.GetFiles(fileNumber);
        Description = files[0];
        var video = files.FirstOrDefault(file => Path.GetExtension(file) == ".mp4");
        if (video is not null)
            VideoViewModel = new VideoViewModel(video);
    }

    public string Description { get; }
    public VideoViewModel? VideoViewModel { get; }

    protected override void OnStart()
    {
        VideoViewModel?.PlayVideo();
    }

    protected override void OnStop()
    {
        VideoViewModel?.Dispose();
    }
}
