using System;

namespace WallProjections.Helper.Interfaces;

/// <summary>
/// A handler for activating and deactivating hotspots
/// </summary>
public interface IHotspotHandler : IDisposable
{
    /// <summary>
    /// The total time required to activate a hotspot.
    /// </summary>
    public static readonly TimeSpan ActivationTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The total time required to deactivate a hotspot.
    /// </summary>
    public static readonly TimeSpan ForcefulDeactivationTime = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// An event that is fired when a hotspot is being activated.
    /// </summary>
    public event EventHandler<HotspotChangingArgs>? HotspotActivating;

    /// <summary>
    /// An event that is fired when a hotspot has been activated.
    /// </summary>
    public event EventHandler<HotspotArgs>? HotspotActivated;

    /// <summary>
    /// An event that is fired when a hotspot is being deactivated.
    /// </summary>
    /// <remarks>Note that there is no event for when the hotspot has fully deactivated.</remarks>
    public event EventHandler<HotspotChangingArgs>? HotspotDeactivating;

    /// <summary>
    /// An event that is fired when a hotspot has been forcefully deactivated.
    /// </summary>
    /// <remarks>
    /// Note that if the deactivation is not forced (e.g. time passes after firing <see cref="HotspotDeactivating" />),
    /// this event will not be fired.
    /// </remarks>
    public event EventHandler<HotspotArgs>? HotspotForcefullyDeactivated;

    /// <summary>
    /// Immediately deactivates the hotspot with the given <paramref name="id" />
    /// (i.e. invokes <see cref="HotspotForcefullyDeactivated" />).
    /// </summary>
    /// <param name="id"></param>
    public void DeactivateHotspot(int id);

    /// <summary>
    /// Arguments for the events in <see cref="IHotspotHandler" />
    /// </summary>
    public class HotspotArgs : EventArgs
    {
        /// <summary>
        /// The id of the hotspot
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// <inheritdoc cref="HotspotArgs" /> with the given <paramref name="id" />
        /// </summary>
        public HotspotArgs(int id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Arguments for <see cref="HotspotActivating" /> and <see cref="HotspotDeactivating" /> events.
    /// </summary>
    public class HotspotChangingArgs : HotspotArgs
    {
        /// <summary>
        /// The time remaining until the hotspot is fully activated/deactivated.
        /// </summary>
        public TimeSpan RemainingTime { get; }

        /// <summary>
        /// <inheritdoc cref="HotspotChangingArgs" /> with the given <paramref name="id" /> and <paramref name="remainingTime" />
        /// </summary>
        /// <param name="id">The id of the changing hotspot</param>
        /// <param name="remainingTime">The time remaining until the hotspot is fully activated/deactivated</param>
        public HotspotChangingArgs(int id, TimeSpan remainingTime) : base(id)
        {
            RemainingTime = remainingTime;
        }
    }
}
