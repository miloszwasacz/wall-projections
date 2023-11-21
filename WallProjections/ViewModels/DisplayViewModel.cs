using System;
using System.IO;
using ReactiveUI;
using WallProjections.Helper;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ViewModelBase, IDisplayViewModel
{
    //TODO Localized strings?
    private const string GenericError = @"An error occurred while loading this content.
Please report this to the museum staff.";

    private const string NotFound = @"Hmm...
Looks like this hotspot has no content.
Please report this to the museum staff.";

    private IConfig? _config;

    /// <summary>
    /// The <see cref="IContentProvider" /> used to fetch Hotspot's content files
    /// </summary>
    private IContentProvider? _contentProvider;

    private string _description = string.Empty;

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> with <see cref="ImageViewModel" /> and <see cref="VideoViewModel" />
    /// fetched by <paramref name="vmProvider" />, and starts listening for <see cref="PythonEventHandler.HotspotSelected">Python events</see>
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> used to fetch internal viewmodels</param>
    public DisplayViewModel(IViewModelProvider vmProvider)
    {
        //TODO Refactor to use VMProvider
        ImageViewModel = new ImageViewModel();
        VideoViewModel = vmProvider.GetVideoViewModel();
        PythonEventHandler.Instance.HotspotSelected += OnHotspotSelected;
    }

    /// <inheritdoc />
    public IConfig Config
    {
        set
        {
            _config = value;
            _contentProvider = new ContentProvider(ContentCache.Instance, _config);
            this.RaisePropertyChanged(nameof(IsContentLoaded));
        }
    }

    /// <inheritdoc />
    public bool IsContentLoaded => _contentProvider is not null;

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        private set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <inheritdoc />
    public ImageViewModel ImageViewModel { get; }

    /// <inheritdoc />
    public IVideoViewModel VideoViewModel { get; }

    /// <inheritdoc />
    public void OnHotspotSelected(object? sender, PythonEventHandler.HotspotSelectedArgs e)
    {
        LoadHotspot(e.Id);
    }

    /// <summary>
    /// Loads the content of the hotspot with the given ID if <see cref="_contentProvider" /> has been set
    /// </summary>
    /// <param name="hotspotId">The ID of a hotspot to fetch data about</param>
    private void LoadHotspot(int hotspotId)
    {
        if (_contentProvider is null) return;

        try
        {
            var media = _contentProvider.GetMedia(hotspotId);
            Description = media.Description;
            ImageViewModel.HideImage();
            VideoViewModel.StopVideo();
            if (media.ImagePath is not null)
                //TODO Refactor to use VMProvider
                //TODO Make ImageViewModel not throw FileNotFoundException (display a placeholder instead)
                ImageViewModel.ShowImage(media.ImagePath);
            else if (media.VideoPath is not null)
                VideoViewModel.PlayVideo(media.VideoPath);
        }
        catch (Exception e) when (e is Models.Config.HotspotNotFoundException or FileNotFoundException)
        {
            //TODO Write to Log instead of Console
            Console.Error.WriteLine(e);
            Description = NotFound;
        }
        catch (Exception e)
        {
            //TODO Write to Log instead of Console
            Console.Error.WriteLine(e);
            Description = GenericError;
        }
    }

    /// <summary>
    /// Unsubscribes from <see cref="PythonEventHandler.HotspotSelected" /> and disposes of <see cref="VideoViewModel" />
    /// </summary>
    public void Dispose()
    {
        PythonEventHandler.Instance.HotspotSelected -= OnHotspotSelected;
        VideoViewModel.Dispose();
    }
}
