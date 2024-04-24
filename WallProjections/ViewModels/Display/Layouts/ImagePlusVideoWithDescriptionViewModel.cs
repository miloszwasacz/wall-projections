using System;
using System.Collections.Generic;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

public class ImagePlusVideoWithDescriptionViewModel : Layout, IDisposable
{
    /// <summary>
    /// Viewmodel used to show images
    /// </summary>
    public IImageViewModel ImageViewModel { get; }

    /// <summary>
    /// Viewmodel used to show videos
    /// </summary>
    public IVideoViewModel VideoViewModel { get; }

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
    /// Creates a new <see cref="VideoWithDescriptionViewModel"/>
    /// with the given <paramref name="title" />, <paramref name="description" />,
    /// and paths to the videos that <see cref="VideoViewModel" /> will play.
    /// </summary>
    /// <param name="vmProvider">The <see cref="IViewModelProvider" /> to get the <see cref="IVideoViewModel" />.</param>
    /// <param name="hotspotId">The id of the hotspot.</param>
    /// <param name="title">The title of the hotspot.</param>
    /// <param name="description">The description of the hotspot.</param>
    /// <param name="imagePaths">The paths to the images to show.</param>
    /// <param name="videoPaths">The paths to the videos to play.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout will deactivate after the <see cref="Layout.DefaultDeactivationTime">default time</see>.
    /// </param>
    /// <remarks>
    /// This layout will deactivate after the last video finishes playing (plus <paramref name="deactivateAfter" />).
    /// </remarks>
    public ImagePlusVideoWithDescriptionViewModel(
        IViewModelProvider vmProvider,
        int hotspotId,
        string title,
        string description,
        IEnumerable<string> imagePaths,
        IEnumerable<string> videoPaths,
        TimeSpan? deactivateAfter = null
    ) : base(hotspotId)
    {
        Title = title;
        Description = description;

        ImageViewModel = vmProvider.GetImageViewModel();
        ImageViewModel.AddImages(imagePaths);
        ImageViewModel.StartSlideshow();

        VideoViewModel = vmProvider.GetVideoViewModel();
        VideoViewModel.AllVideosFinished += (_, _) => DeactivateAfterAsync(deactivateAfter ?? DefaultDeactivationTime);
        VideoViewModel.PlayVideos(videoPaths);
    }

    public void Dispose()
    {
        ImageViewModel.Dispose();
        VideoViewModel.Dispose();
        GC.SuppressFinalize(this);
    }


    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// A factory for creating <see cref="VideoWithDescriptionViewModel" />s.
    /// </summary>
    public class Factory : LayoutFactory
    {
        /// <inheritdoc />
        public override bool IsCompatibleData(Hotspot.Media hotspot)
        {
            var imagesCompatible = hotspot.ImagePaths.Count >= 1;
            var videosCompatible = hotspot.VideoPaths.Count >= 1;

            return imagesCompatible && videosCompatible;
        }

        /// <inheritdoc />
        protected override Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
            new ImagePlusVideoWithDescriptionViewModel(
                vmProvider,
                hotspot.Id,
                hotspot.Title,
                hotspot.Description,
                hotspot.ImagePaths,
                hotspot.VideoPaths
            );
    }
}
