using System;
using System.Diagnostics;
using System.IO;
using LibVLCSharp.Shared;
using Python.Runtime;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly LibVLC _libVlc = new();

    public MainWindowViewModel()
    {
        MediaPlayer = new MediaPlayer(_libVlc);
    }

    public void Play()
    {
        using var media = new Media(
            _libVlc,
            new Uri(@"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")
        );
        MediaPlayer.Play(media);
    }

    public MediaPlayer MediaPlayer { get; }

    public void OpenPython()
    {
        string file = @"Scripts\main.py";
        Debug.Print("Opening python");
        using var scope = Py.CreateScope();
        var code = File.ReadAllText(file);
        Debug.Print("Code read");
        var scriptCompiled = PythonEngine.Compile(code, file);
        scope.Execute(scriptCompiled);
    }

    public void Dispose()
    {
        MediaPlayer.Dispose();
        _libVlc.Dispose();
    }


    public string Greeting => "Welcome to Avalonia!";
}