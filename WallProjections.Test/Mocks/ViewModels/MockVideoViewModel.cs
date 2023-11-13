using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="VideoViewModel"/> for injecting into <see cref="DisplayViewModel"/>
/// </summary>
public sealed class MockVideoViewModel : IVideoViewModel
{
    /// <summary>
    /// The path of the video the viewmodel was constructed with
    /// </summary>
    public string VideoPath { get; }

    /// <summary>
    /// Determines if <see cref="MediaPlayer"/> is null or not (<i>true</i> means not <i>null</i>)
    /// </summary>
    public bool CanPlay { get; set; } = true;

    /// <summary>
    /// The number of times <see cref="PlayVideo"/> has been called
    /// </summary>
    public int PlayCounter { get; private set; }

    /// <summary>
    /// The number of times <see cref="StopVideo"/> has been called
    /// </summary>
    public int StopCounter { get; private set; }

    /// <summary>
    /// The number of times <see cref="Dispose"/> has been called
    /// </summary>
    public int DisposeCounter { get; private set; }

    /// <summary>
    /// The backing field for <see cref="MediaPlayer"/>
    /// </summary>
    private readonly MockMediaPlayer _mediaPlayer = new();

    /// <summary>
    /// A mock of <see cref="WallProjections.Models.VLCMediaPlayer"/> if <see cref="CanPlay"/> is <i>true</i>,
    /// <i>null</i> otherwise
    /// </summary>
    public IMediaPlayer? MediaPlayer => CanPlay ? _mediaPlayer : null;

    /// <summary>
    /// A mock of <see cref="VideoViewModel"/> for injecting into <see cref="DisplayViewModel"/>
    /// </summary>
    /// <param name="videoPath">The path to the video the viewmodel is supposed to play</param>
    public MockVideoViewModel(string videoPath)
    {
        VideoPath = videoPath;
    }

    /// <summary>
    /// Increases the number of times <see cref="PlayVideo"/> has been called
    /// </summary>
    /// <returns><i>True</i> if <see cref="MediaPlayer"/> is not <i>null</i></returns>
    public bool PlayVideo()
    {
        PlayCounter++;
        return MediaPlayer is not null;
    }

    /// <summary>
    /// Increases the number of times <see cref="StopVideo"/> has been called
    /// </summary>
    public void StopVideo()
    {
        StopCounter++;
    }

    /// <summary>
    /// Calls <see cref="StopVideo"/> and increases the number of times <see cref="Dispose"/> has been called
    /// </summary>
    public void Dispose()
    {
        StopVideo();
        DisposeCounter++;
    }
}
