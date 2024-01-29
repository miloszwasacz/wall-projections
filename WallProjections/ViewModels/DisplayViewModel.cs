using System;
using System.IO;
using ReactiveUI;
using WallProjections.Helper.Interfaces;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.ViewModels;

public sealed class DisplayViewModel : ViewModelBase, IDisplayViewModel
{
    //TODO Localized strings?
    internal const string GenericError = @"An error occurred while loading this content.
Please report this to the museum staff.";

    internal const string NotFound = @"Hmm...
Looks like this hotspot has missing content.
Please report this to the museum staff.";

    /// <summary>
    /// The <see cref="IContentProvider" /> used to fetch Hotspot's content files
    /// </summary>
    private readonly IContentProvider _contentProvider;

    /// <summary>
    /// The <see cref="IPythonEventHandler" /> used to listen for Python events
    /// </summary>
    private readonly IPythonEventHandler _pythonEventHandler;

    /// <summary>
    /// The backing field for <see cref="Description" />
    /// </summary>
    private string _description = string.Empty;

    /// <summary>
    /// Creates a new <see cref="DisplayViewModel" /> with <see cref="ImageViewModel" />
    /// and <see cref="VideoViewModel" /> fetched by <paramref name="vmProvider" />,
    /// and starts listening for <see cref="IPythonEventHandler.HotspotSelected">Python events</see>
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> used to fetch internal viewmodels</param>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    /// <param name="pythonEventHandler">The <see cref="IPythonEventHandler" /> used to listen for Python events</param>
    public DisplayViewModel(
        IViewModelProvider vmProvider,
        IConfig config,
        IPythonEventHandler pythonEventHandler
    )
    {
        ImageViewModel = vmProvider.GetImageViewModel();
        VideoViewModel = vmProvider.GetVideoViewModel();
        _pythonEventHandler = pythonEventHandler;
        _pythonEventHandler.HotspotSelected += OnHotspotSelected;
        _contentProvider = new ContentProvider(config);
    }

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        private set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <inheritdoc />
    public IImageViewModel ImageViewModel { get; }

    /// <inheritdoc />
    public IVideoViewModel VideoViewModel { get; }

    /// <inheritdoc />
    public void OnHotspotSelected(object? sender, IPythonEventHandler.HotspotSelectedArgs e)
    {
        ShowHotspot(e.Id);
    }

    /// <summary>
    /// Loads the content of the hotspot with the given ID if <see cref="_contentProvider" /> has been set
    /// </summary>
    /// <param name="hotspotId">The ID of a hotspot to show</param>
    private void ShowHotspot(int hotspotId)
    {
        try
        {
            ImageViewModel.HideImage();
            VideoViewModel.StopVideo();
            var media = _contentProvider.GetMedia(hotspotId);

            Description = media.Description;
            // TODO Add support for multiple images/videos
            if (media.ImagePath is not null && media.ImagePath != "")
                //TODO Make ImageViewModel not throw FileNotFoundException (display a placeholder instead)
                ImageViewModel.ShowImage(media.ImagePath);
            else if (media.VideoPath is not null && media.VideoPath != "")
            {
                VideoViewModel.PlayVideo(media.VideoPath);
            }
        }
        catch (Exception e) when (e is IConfig.HotspotNotFoundException or FileNotFoundException)
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
    /// Unsubscribes from <see cref="IPythonEventHandler.HotspotSelected" /> and disposes of <see cref="VideoViewModel" />
    /// </summary>
    public void Dispose()
    {
        _pythonEventHandler.HotspotSelected -= OnHotspotSelected;
        VideoViewModel.Dispose();
    }
}
