using System;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.ViewModels.Display.Layouts;

/// <summary>
/// A viewmodel for a view that displays a title and description.
/// </summary>
public class DescriptionViewModel : Layout
{
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Creates a new <see cref="DescriptionViewModel" />
    /// with the given <paramref name="title" /> and <paramref name="description" />
    /// not based on a hotspot.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="description">The description to display.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout won't deactivate on its own.
    /// </param>
    protected DescriptionViewModel(string title, string description, TimeSpan? deactivateAfter) : base(null)
    {
        Title = title;
        Description = description;
        if (deactivateAfter is not null)
            DeactivateAfterAsync(deactivateAfter.Value);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Creates a new <see cref="DescriptionViewModel" />
    /// with the given <paramref name="title" /> and <paramref name="description" />.
    /// </summary>
    /// <param name="hotspotId">The id of the hotspot.</param>
    /// <param name="title">The title of the hotspot.</param>
    /// <param name="description">The description of the hotspot.</param>
    /// <param name="deactivateAfter">
    /// The time after which the layout should deactivate.
    /// If <i>null</i>, the layout will deactivate after the <see cref="Layout.DefaultDeactivationTime">default time</see>.
    /// </param>
    public DescriptionViewModel(
        int hotspotId,
        string title,
        string description,
        TimeSpan? deactivateAfter = null
    ) : base(hotspotId)
    {
        Title = title;
        Description = description;
        DeactivateAfterAsync(deactivateAfter ?? DefaultDeactivationTime);
    }

    /// <summary>
    /// The title of the hotspot.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The description of the hotspot.
    /// </summary>
    public string Description { get; }

    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// A factory for creating <see cref="DescriptionViewModel" />s.
    /// </summary>
    public class Factory : LayoutFactory
    {
        /// <inheritdoc />
        public override bool IsCompatibleData(Hotspot.Media hotspot)
        {
            var imagesCompatible = hotspot.ImagePaths.Count == 0;
            var videosCompatible = hotspot.VideoPaths.Count == 0;

            return imagesCompatible && videosCompatible;
        }

        /// <inheritdoc />
        protected override Layout ConstructLayout(IViewModelProvider vmProvider, Hotspot.Media hotspot) =>
            new DescriptionViewModel(hotspot.Id, hotspot.Title, hotspot.Description);
    }
}
