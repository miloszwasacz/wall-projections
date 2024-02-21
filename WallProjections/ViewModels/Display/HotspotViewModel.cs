using System;
using System.Collections.Immutable;
using System.Linq;
using WallProjections.Helper.Interfaces;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.ViewModels.Display;

/// <inheritdoc cref="IHotspotViewModel" />
public class HotspotViewModel : ViewModelBase, IHotspotViewModel, IDisposable
{
    /// <summary>
    /// The Python handler that invokes events about hotspot activation
    /// </summary>
    private readonly IPythonHandler _pythonHandler;

    /// <summary>
    /// Creates a new instance of <see cref="HotspotViewModel"/> based on the provided <paramref name="config" />
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> holding information about the hotspots</param>
    /// <param name="pythonHandler">The Python handler that invokes events about hotspot activation</param>
    public HotspotViewModel(IConfig config, IPythonHandler pythonHandler)
    {
        _pythonHandler = pythonHandler;
        Projections = GetHotspots(config);

        _pythonHandler.HotspotSelected += HotspotActivated;
    }

    /// <inheritdoc/>
    public ImmutableList<HotspotProjectionViewModel> Projections { get; }

    //TODO Make initially hidden, and show when a visitor approaches the artifact
    /// <inheritdoc/>
    public bool IsVisible { get; private set; } = true;

    /// <inheritdoc/>
    public void ActivateHotspot(int id)
    {
        foreach (var coord in Projections)
        {
            if (coord.Id == id)
                coord.IsActive = true;
            else if (coord.IsActive)
                coord.IsActive = false;
        }
    }

    /// <inheritdoc/>
    public void DeactivateHotspots()
    {
        var active = Projections.Where(coord => coord.IsActive);
        foreach (var coord in active)
            coord.IsActive = false;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Goes through all the hotspots in the config file and turns them into
    /// <see cref="HotspotProjectionViewModel"/> instances
    /// </summary>
    /// <returns>List of <see cref="HotspotProjectionViewModel"/> relating to all hotspots in config file</returns>
    private static ImmutableList<HotspotProjectionViewModel> GetHotspots(IConfig config) =>
        config.Hotspots.Select(hotspot => new HotspotProjectionViewModel(hotspot)).ToImmutableList();

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
