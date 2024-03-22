using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="AbsHotspotDisplayViewModel" />
public class HotspotDisplayViewModel : AbsHotspotDisplayViewModel, IDisposable
{
    /// <summary>
    /// The <see cref="IHotspotHandler" /> that sends events about hotspot activation and deactivation
    /// </summary>
    private readonly IHotspotHandler _hotspotHandler;

    /// <summary>
    /// The backing field for <see cref="Projections" /> mapped to the id of the hotspot
    /// </summary>
    private readonly ImmutableDictionary<int, IHotspotProjectionViewModel> _projections;

    /// <summary>
    /// Creates a new instance of <see cref="HotspotDisplayViewModel"/> based on the provided <paramref name="config" />
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> holding information about the hotspots</param>
    /// <param name="hotspotHandler">
    /// The <see cref="IHotspotHandler" /> that sends events about hotspot activation and deactivation
    /// </param>
    /// <param name="vmProvider">
    /// The <see cref="IViewModelProvider" /> for creating <see cref="IHotspotProjectionViewModel" />s
    /// </param>
    public HotspotDisplayViewModel(IConfig config, IHotspotHandler hotspotHandler, IViewModelProvider vmProvider)
    {
        _hotspotHandler = hotspotHandler;
        _projections = GetHotspots(config, vmProvider);

        _hotspotHandler.HotspotActivating += HotspotActivating;
        _hotspotHandler.HotspotActivated += HotspotActivated;
        _hotspotHandler.HotspotDeactivating += HotspotDeactivating;
        _hotspotHandler.HotspotForcefullyDeactivated += HotspotForcefullyDeactivated;
    }

    /// <inheritdoc/>
    public override IEnumerable<IHotspotProjectionViewModel> Projections => _projections.Values;

    //TODO Make initially hidden, and show when a visitor approaches the artifact
    /// <inheritdoc/>
    public override bool IsVisible { get; protected set; } = true;

    /// <inheritdoc/>
    public override void DeactivateHotspots()
    {
        var active = Projections.Where(coord => coord.IsActive);
        foreach (var coord in active)
            coord.IsActive = false;
    }

    /// <inheritdoc/>
    public override void DisplayHotspots()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Goes through all the hotspots in the config file and turns them into
    /// <see cref="IHotspotProjectionViewModel"/> instances
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> holding information about the hotspots</param>
    /// <param name="vmProvider">
    /// The <see cref="IViewModelProvider" /> for creating <see cref="IHotspotProjectionViewModel" />s
    /// </param>
    /// <returns>List of <see cref="IHotspotProjectionViewModel"/> relating to all hotspots in config file</returns>
    private static ImmutableDictionary<int, IHotspotProjectionViewModel> GetHotspots(
        IConfig config,
        IViewModelProvider vmProvider
    ) => config.Hotspots
        .Select(vmProvider.GetHotspotProjectionViewModel)
        .ToImmutableDictionary(vm => vm.Id, vm => vm);

    #region Event Callbacks

    /// <summary>
    /// Looks up the hotspot with the given id and sets <see cref="IHotspotProjectionViewModel.IsActivating" /> to true
    /// and <see cref="IHotspotProjectionViewModel.IsActive" /> to false.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the id of the hotspot to be activated.</param>
    private void HotspotActivating(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsDeactivating = false;
        hotspot.IsActive = false;
        hotspot.IsActivating = true;
    }

    /// <summary>
    /// Looks up the hotspot with the given id and sets <see cref="IHotspotProjectionViewModel.IsActivating" />
    /// and <see cref="IHotspotProjectionViewModel.IsDeactivating" /> to false,
    /// and <see cref="IHotspotProjectionViewModel.IsActive" /> to true.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the id of the hotspot to be activated.</param>
    private void HotspotActivated(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsDeactivating = false;
        hotspot.IsActivating = false;
        hotspot.IsActive = true;
    }

    /// <summary>
    /// Looks up the hotspot with the given id and sets <see cref="IHotspotProjectionViewModel.IsActive" /> and
    /// <see cref="IHotspotProjectionViewModel.IsActivating" /> to false,
    /// and <see cref="IHotspotProjectionViewModel.IsDeactivating" /> to true.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments containing the id of the hotspot to be deactivated.</param>
    private void HotspotDeactivating(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsActivating = false;
        hotspot.IsActive = false;
        hotspot.IsDeactivating = true;
    }

    /// <summary>
    /// Looks up the hotspot with the given id and sets <see cref="IHotspotProjectionViewModel.IsActive" />,
    /// <see cref="IHotspotProjectionViewModel.IsActivating" />, and <see cref="IHotspotProjectionViewModel.IsDeactivating" />
    /// to false.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HotspotForcefullyDeactivated(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsActivating = false;
        hotspot.IsActive = false;
        hotspot.IsDeactivating = false;
    }

    #endregion

    public void Dispose()
    {
        _hotspotHandler.HotspotActivating -= HotspotActivating;
        _hotspotHandler.HotspotActivated -= HotspotActivated;
        _hotspotHandler.HotspotDeactivating -= HotspotDeactivating;
        _hotspotHandler.HotspotForcefullyDeactivated -= HotspotForcefullyDeactivated;
        GC.SuppressFinalize(this);
    }
}
