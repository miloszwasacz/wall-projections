using System;
using ReactiveUI;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel holding all info required to project a hotspot onto the artifact
/// </summary>
public abstract class AbsHotspotProjectionViewModel : ViewModelBase, IPosition
{
    /// <summary>
    /// The backing field for <see cref="State" />
    /// </summary>
    private HotspotState _state;

    /// <summary>
    /// The backing field for <see cref="AnimationDuration" />
    /// </summary>
    private TimeSpan _animationDuration;

    /// <summary>
    /// The X coordinate of the top-left corner of the hotspot's bounding box
    /// </summary>
    public abstract double X { get; }

    /// <summary>
    /// The Y coordinate of the top-left corner of the hotspot's bounding box
    /// </summary>
    public abstract double Y { get; }

    /// <summary>
    /// The id of the hotspot to be activated
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public abstract double D { get; }

    /// <summary>
    /// The current state of the hotspot projection
    /// </summary>
    public HotspotState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    /// <summary>
    /// The time required to animate the hotspot
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => _animationDuration;
        private set => this.RaiseAndSetIfChanged(ref _animationDuration, value);
    }

    /// <summary>
    /// Sets <see cref="State" /> to <see cref="HotspotState.Activating" />
    /// and <see cref="AnimationDuration" /> to <paramref name="duration" />
    /// </summary>
    /// <param name="duration">The time required to animate the hotspot</param>
    public void SetActivating(TimeSpan duration)
    {
        AnimationDuration = duration;
        State = HotspotState.Activating;
    }

    /// <summary>
    /// Sets <see cref="State" /> to <see cref="HotspotState.Active" />
    /// </summary>
    public void SetActive()
    {
        AnimationDuration = IHotspotHandler.ForcefulDeactivationTime;
        State = HotspotState.Active;
    }

    /// <summary>
    /// Sets <see cref="State" /> to <see cref="HotspotState.Deactivating" />
    /// and <see cref="AnimationDuration" /> to <paramref name="duration" />
    /// </summary>
    /// <param name="duration">The time required to animate the hotspot</param>
    public void SetDeactivating(TimeSpan duration)
    {
        AnimationDuration = duration;
        State = HotspotState.Deactivating;
    }

    /// <summary>
    /// Sets <see cref="State" /> to <see cref="HotspotState.None" />
    /// </summary>
    public void SetIdle()
    {
        AnimationDuration = IHotspotHandler.ForcefulDeactivationTime;
        State = HotspotState.None;
    }
}

/// <summary>
/// An enum to hold the current state of the hotspot, to be used to trigger the animations
/// </summary>
public enum HotspotState
{
    None,
    Activating,
    Active,
    Deactivating
}
