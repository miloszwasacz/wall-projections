using System;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces;

public interface IVideoViewModel : IDisposable
{
    public IMediaPlayer? MediaPlayer { get; }
    public bool PlayVideo();
    public void StopVideo();
}
