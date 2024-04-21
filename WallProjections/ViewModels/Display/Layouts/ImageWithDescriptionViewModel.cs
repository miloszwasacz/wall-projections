using System;
using System.Collections.Generic;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

//TODO Add code generation for multiple viewmodels that have different number of images
/// <summary>
/// A viewmodel for a view that displays an image with a title and description.
/// </summary>
public class ImageWithDescriptionViewModel : Layout
{
    /// <summary>
    /// Image view model used to show image
    /// </summary>
    public IImageViewModel ImageViewModel { get; }

    /// <summary>
    /// Title for hotspot
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Description for hotspot
    /// </summary>
    public string Description { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Creates a new <see cref="ImageWithDescriptionViewModel" />
    /// with the given <paramref name="title" />, <paramref name="description" />,
    /// and paths to the images that <see cref="ImageViewModel" /> will show.
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> to get the <see cref="IImageViewModel" />.</param>
    /// <param name="hotspotId">The id of the hotspot.</param>
    /// <param name="title">The title of the hotspot.</param>
    /// <param name="description">The description of the hotspot.</param>
    /// <param name="imagePaths">The paths to the images to show.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout will deactivate after the <see cref="Layout.DefaultDeactivationTime">default time</see>.
    /// </param>
    public ImageWithDescriptionViewModel(
        IViewModelProvider vmProvider,
        int hotspotId,
        string title,
        string description,
        IEnumerable<string> imagePaths,
        TimeSpan? deactivateAfter = null
    ) : base(hotspotId)
    {
        Title = title;
        Description = description;
        ImageViewModel = vmProvider.GetImageViewModel();
        ImageViewModel.AddImages(imagePaths);
        ImageViewModel.StartSlideshow(TimeSpan.FromSeconds(5));
        DeactivateAfterAsync(deactivateAfter ?? DefaultDeactivationTime);
    }

    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// A factory for creating <see cref="ImageWithDescriptionViewModel" />s.
    /// </summary>
    public class Factory : LayoutFactory
    {
        /// <inheritdoc />
        public override bool IsCompatibleData(Hotspot.Media hotspot)
        {
            var imagesCompatible = hotspot.ImagePaths.Count > 0;
            var videosCompatible = hotspot.VideoPaths.Count == 0;

            return videosCompatible && imagesCompatible;
        }

        /// <inheritdoc />
        protected override Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
            new ImageWithDescriptionViewModel(
                vmProvider,
                hotspot.Id,
                hotspot.Title,
                hotspot.Description,
                hotspot.ImagePaths
            );
    }
}
