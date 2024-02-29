using System.Collections.Generic;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class VideoPlusDescriptionViewModel : ViewModelBase, ILayout
{
    public VideoPlusDescriptionViewModel(
        IViewModelProvider vmProvider,
        string description,
        IEnumerable<string> videoPaths)
    {
        VideoViewModel = vmProvider.GetVideoViewModel();
        Description = description;
        VideoPaths = new List<string>(videoPaths);

        VideoViewModel.PlayVideos(VideoPaths);
    }

    public string Description { get; } = string.Empty;

    public List<string> VideoPaths { get; }

    public IVideoViewModel VideoViewModel { get; }

    public void Dispose()
    {
        VideoViewModel.Dispose();
    }
}
