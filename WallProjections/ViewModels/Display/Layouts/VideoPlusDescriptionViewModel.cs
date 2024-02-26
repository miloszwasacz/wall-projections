using System;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class VideoPlusDescriptionViewModel : ViewModelBase, ILayout
{
    public VideoPlusDescriptionViewModel(
        IViewModelProvider vmProvider,
        string description,
        string videoPath)
    {
        VideoViewModel = vmProvider.GetVideoViewModel();
        Description = description;
        VideoPath = videoPath;
    }

    public string Description { get; } = string.Empty;

    public string VideoPath { get; } = string.Empty;

    public IVideoViewModel VideoViewModel { get; }

    public void Dispose()
    {
        Console.WriteLine("Disposing of VideoPlusDescriptionViewModel");
        VideoViewModel.Dispose();
    }
}
