using System;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

public interface IVideoViewModel : IDisposable
{
    public IMediaPlayer? MediaPlayer { get; }
    public bool HasVideos { get; }
    public int Volume { get; set; }

    public bool PlayVideo(string path);
    public void StopVideo();
}
