using System.Collections.Generic;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class ImagePlusDescriptionViewModel : ViewModelBase, ILayout
{
    /// <summary>
    /// Constructor for view model.
    /// </summary>
    /// <param name="vmProvider">Provider for any required view models</param>
    /// <param name="title">Title for hotspot</param>
    /// <param name="description">Description for hotspot</param>
    /// <param name="imagePaths">Images to display</param>
    public ImagePlusDescriptionViewModel(
        IViewModelProvider vmProvider,
        string title,
        string description,
        IEnumerable<string> imagePaths)
    {
        Title = title;
        Description = description;
        ImagePaths = new List<string>(imagePaths);
        ImageViewModel = vmProvider.GetImageViewModel();
        ImageViewModel.ShowImage(ImagePaths[0]);
    }

    /// <summary>
    /// Title for hotspot
    /// </summary>
    public string Title { get; } = string.Empty;

    /// <summary>
    /// Description for hotspot
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Image view model used to show image
    /// </summary>
    public IImageViewModel ImageViewModel { get; }

    /// <summary>
    /// All paths to images to be displayed
    /// TODO: Display multiple images
    /// </summary>
    public List<string> ImagePaths { get; }

    public void Dispose ()
    {

    }
}
