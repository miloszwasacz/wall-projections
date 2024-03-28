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
    public static readonly TimeSpan ActivationTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The total time required to deactivate a hotspot.
    /// </summary>
    public static readonly TimeSpan ForcefulDeactivationTime = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// An event that is fired when a hotspot is being activated.
    /// </summary>
    public event EventHandler<HotspotArgs>? HotspotActivating;

    /// <summary>
    /// An event that is fired when a hotspot has been activated.
    /// </summary>
    public event EventHandler<HotspotArgs>? HotspotActivated;

    /// <summary>
    /// An event that is fired when a hotspot is being deactivated.
    /// </summary>
    /// <remarks>Note that there is no event for when the hotspot has fully deactivated.</remarks>
    public event EventHandler<HotspotArgs>? HotspotDeactivating;

    /// <summary>
    /// An event that is fired when a hotspot has been forcefully deactivated.
    /// </summary>
    /// <remarks>
    /// Note that if the deactivation is not forced (e.g. time passes after firing <see cref="HotspotDeactivating" />),
    /// this event will not be fired.
    /// </remarks>
    public event EventHandler<HotspotArgs>? HotspotForcefullyDeactivated;

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
}
