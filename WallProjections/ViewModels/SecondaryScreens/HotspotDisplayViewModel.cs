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
    }

    /// <inheritdoc/>
    public override IEnumerable<IHotspotProjectionViewModel> Projections => _projections.Values;

    //TODO Make initially hidden, and show when a visitor approaches the artifact
    /// <inheritdoc/>
    public override bool IsVisible { get; protected set; } = true;

    /// <inheritdoc/>
    public override void ActivateHotspot(int id)
    {
        foreach (var coord in Projections)
            coord.IsActive = coord.Id == id;
    }

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

    /// <inheritdoc cref="ActivateHotspot" />
    private void HotspotActivating(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsActivating = true;
        hotspot.IsActive = false;
    }

    /// <inheritdoc cref="ActivateHotspot" />
    private void HotspotActivated(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsActivating = false;
        hotspot.IsActive = true;
    }

    private void HotspotDeactivating(object? sender, IHotspotHandler.HotspotArgs e)
    {
        if (!_projections.TryGetValue(e.Id, out var hotspot)) return;

        hotspot.IsActivating = false;
        hotspot.IsActive = false;
    }

    #endregion

    public void Dispose()
    {
        _hotspotHandler.HotspotActivating -= HotspotActivating;
        _hotspotHandler.HotspotActivated -= HotspotActivated;
        _hotspotHandler.HotspotDeactivating -= HotspotDeactivating;
        GC.SuppressFinalize(this);
    }
}
