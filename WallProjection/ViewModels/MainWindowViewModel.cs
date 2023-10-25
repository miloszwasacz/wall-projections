using System;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace WallProjection.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly LibVLC _libVlc = new();

    public MainWindowViewModel()
    {
        MediaPlayer = new MediaPlayer(_libVlc);
    }

    public void Play(string path)
    {
        // Task.Run(async delegate
        // {
        //     await Task.Delay(1000);
            using var media = new Media(
                _libVlc,
                new Uri(path)
            );
            MediaPlayer.Play(media);
        // }).Start();
    }

    public MediaPlayer MediaPlayer { get; }

    public void Dispose()
    {
        MediaPlayer?.Dispose();
        _libVlc?.Dispose();
    }


    public string Greeting => "Welcome to Avalonia!";
}