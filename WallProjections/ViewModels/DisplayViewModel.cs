using System;
using System.IO;
using ReactiveUI;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ActivatableViewModelBase, IDisplayViewModel
{
    //TODO Localized strings?
    private const string GenericError = @"An error occurred while loading this content.
Please report this to the museum staff.";

    private const string NotFound = @"Hmm...
Looks like this hotspot has no content.
Please report this to the museum staff.";

    private readonly IViewModelProvider _vmProvider;
    private readonly string? _videoPath;
    private IVideoViewModel? _videoViewModel;

    public DisplayViewModel(IViewModelProvider vmProvider, IContentProvider contentProvider, int hotspotId)
    {
        _vmProvider = vmProvider;
        try
        {
            var media = contentProvider.GetMedia(hotspotId);
            Description = media.Description;
            if (media.ImagePath is not null)
                //TODO Refactor to use VMProvider
                //TODO Make ImageViewModel not throw FileNotFoundException (display a placeholder instead)
                ImageViewModel = new ImageViewModel(media.ImagePath);
            else if (media.VideoPath is not null)
                _videoPath = media.VideoPath;
        }
        catch (Exception e) when (e is Config.HotspotNotFoundException or FileNotFoundException)
        {
            Console.WriteLine(e);
            Description = NotFound;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Description = GenericError;
        }
    }

    public string Description { get; }

    public bool HasImages => ImageViewModel is not null;
    public bool HasVideos => VideoViewModel is not null && !HasImages;

    public ViewModelBase? ImageViewModel { get; }

    public IVideoViewModel? VideoViewModel
    {
        get => _videoViewModel;
        private set
        {
            if (value is null)
                _videoViewModel?.Dispose();

            this.RaiseAndSetIfChanged(ref _videoViewModel, value);
            this.RaisePropertyChanged(nameof(HasVideos));
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
