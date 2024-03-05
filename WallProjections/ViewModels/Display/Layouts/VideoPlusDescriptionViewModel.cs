using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class VideoPlusDescriptionViewModel : ViewModelBase, ILayout
{
    /// <summary>
    /// Constructor for view model.
    /// </summary>
    /// <param name="vmProvider">Provider for any required view models</param>
    /// <param name="title">Title for hotspot</param>
    /// <param name="description">Description for hotspot</param>
    /// <param name="videoPaths">Videos to play in order, one after another</param>
    public VideoPlusDescriptionViewModel(
        IViewModelProvider vmProvider,
        string title,
        string description,
        IEnumerable<string> videoPaths)
    {
        VideoViewModel = vmProvider.GetVideoViewModel();
        Title = title;
        Description = description;

        VideoViewModel.PlayVideos(videoPaths);
    }

    /// <summary>
    /// Description for hotspot
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Title for hotspot
    /// </summary>
    public string Title { get; } = string.Empty;

    /// <summary>
    /// View model for internal video player
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

    public void Dispose()
    {
        VideoViewModel.Dispose();
    }
}
