using System;
using System.Threading.Tasks;

namespace WallProjections.ViewModels.Interfaces.Display.Layouts;

/// <summary>
/// A base class for all available layout types for display content about a hotspot
/// </summary>
public abstract class Layout : ViewModelBase
{
    /// <summary>
    /// The default time after which the layout deactivates.
    /// </summary>
    protected static readonly TimeSpan DefaultDeactivationTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The event that is triggered when the layout deactivates.
    /// The exact moment of deactivation is defined by the layout itself - it can be after some time, after a video ends, etc.
    /// </summary>
    public event EventHandler<DeactivationEventArgs>? Deactivated;

    /// <summary>
    /// The id of the hotspot associated with this layout.
    /// </summary>
    public int? HotspotId { get; }

    /// <summary>
    /// Creates a new <see cref="Layout"/> with the given <paramref name="hotspotId"/>.
    /// </summary>
    /// <param name="hotspotId">
    /// The id of the hotspot associated with this layout, or <i>null"</i> if there is no associated hotspot.
    /// </param>
    protected Layout(int? hotspotId)
    {
        HotspotId = hotspotId;
    }

    /// <summary>
    /// Deactivates the layout <i>(see <see cref="Deactivated" />)</i>.
    /// </summary>
    private async Task DeactivateAfter(TimeSpan time)
    {
        await Task.Delay(time);
        Deactivated?.Invoke(this, new DeactivationEventArgs(this));
    }

    /// <summary>
    /// Calls <see cref="DeactivateAfter(TimeSpan)" /> asynchronously (with no return value).
    /// </summary>
    protected async void DeactivateAfterAsync(TimeSpan time) => await DeactivateAfter(time);

    /// <summary>
    /// Event arguments for the <see cref="Deactivated" /> event.
    /// </summary>
    public class DeactivationEventArgs : EventArgs
    {
        /// <summary>
        /// The layout that was deactivated.
        /// </summary>
        private Layout DeactivatedLayout { get; }

        /// <summary>
        /// Creates a new <see cref="DeactivationEventArgs" /> with the given <paramref name="deactivatedLayout" />.
        /// </summary>
        /// <param name="deactivatedLayout">The layout that was deactivated.</param>
        public DeactivationEventArgs(Layout deactivatedLayout)
        {
            DeactivatedLayout = deactivatedLayout;
        }

        /// <summary>
        /// Whether the given <paramref name="layout" /> is the <see cref="object.ReferenceEquals">same</see>
        /// as the deactivated layout.
        /// </summary>
        /// <param name="layout">The layout to check.</param>
        public bool IsLayoutDeactivated(Layout layout) => ReferenceEquals(layout, DeactivatedLayout);
    }
}

/// <summary>
/// Extension methods related to <see cref="Layout.DeactivationEventArgs" />.
/// </summary>
public static class LayoutDeactivationEventArgsExtensions
{
    /// <summary>
    /// Whether the given <paramref name="layout" /> is deactivated.
    /// </summary>
    /// <param name="layout">The layout to check.</param>
    /// <param name="eventArgs">The event arguments to check against.</param>
    /// <seealso cref="Layout.DeactivationEventArgs.IsLayoutDeactivated" />
    public static bool IsDeactivated(this Layout layout, Layout.DeactivationEventArgs eventArgs) =>
        eventArgs.IsLayoutDeactivated(layout);
}
