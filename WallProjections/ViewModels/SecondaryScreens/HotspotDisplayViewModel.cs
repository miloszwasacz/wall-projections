using System;
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
    /// The Python handler that invokes events about hotspot activation
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// Creates a new instance of <see cref="HotspotDisplayViewModel"/> based on the provided <paramref name="config" />
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> holding information about the hotspots</param>
    /// <param name="pythonHandler">The Python handler that invokes events about hotspot activation</param>
    /// <param name="vmProvider">
    /// The <see cref="IViewModelProvider" /> for creating <see cref="IHotspotProjectionViewModel" />s
    /// </param>
    public HotspotDisplayViewModel(IConfig config, IPythonHandler pythonHandler, IViewModelProvider vmProvider)
    {
        _pythonHandler = pythonHandler;
        Projections = GetHotspots(config, vmProvider);

        _pythonHandler.HotspotSelected += HotspotActivated;
    }

    /// <inheritdoc/>
    public override ImmutableList<IHotspotProjectionViewModel> Projections { get; }

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
    private static ImmutableList<IHotspotProjectionViewModel> GetHotspots(
        IConfig config,
        IViewModelProvider vmProvider
    ) => config.Hotspots
        .Select(vmProvider.GetHotspotProjectionViewModel)
        .ToImmutableList();

    private void HotspotActivated(object? sender, IPythonHandler.HotspotSelectedArgs e)
    {
        ActivateHotspot(e.Id);
    }

    public void Dispose()
    {
        _pythonHandler.HotspotSelected -= HotspotActivated;
        GC.SuppressFinalize(this);
    }
}
