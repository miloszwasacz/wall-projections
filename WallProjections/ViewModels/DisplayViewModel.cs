using System.IO;
using System.Linq;
using ReactiveUI;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ActivatableViewModelBase, IDisplayViewModel
{
    private readonly IViewModelProvider _vmProvider;
    private readonly string? _videoPath;

    public DisplayViewModel(IViewModelProvider vmProvider, IFileProvider fileProvider, string artifactId)
    {
        _vmProvider = vmProvider;
        // TODO Refactor to fetch specific file types from FileLocator instead of filtering here
        var files = fileProvider.GetFiles(artifactId);
        Description = files[0];
        var video = files.FirstOrDefault(file => Path.GetExtension(file) == ".mp4");
        if (video is not null)
        {
            _videoPath = video;
            VideoViewModel = vmProvider.GetVideoViewModel(video);
        }
    }

    public string Description { get; }

    private IVideoViewModel? _videoViewModel;
    public IVideoViewModel? VideoViewModel
    {
        get => _videoViewModel;
        private set
        {
            if (value is null)
                _videoViewModel?.Dispose();

            this.RaiseAndSetIfChanged(ref _videoViewModel, value);
        }
    }

    protected override void OnStart()
    {
        if (_videoPath is not null && VideoViewModel is null)
            VideoViewModel = _vmProvider.GetVideoViewModel(_videoPath);

        VideoViewModel?.PlayVideo();
    }

    protected override void OnStop()
    {
        VideoViewModel = null;
    }
}
