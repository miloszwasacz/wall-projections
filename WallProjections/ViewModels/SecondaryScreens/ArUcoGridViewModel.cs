using System.Collections.Immutable;
using System.Linq;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.ViewModels.SecondaryScreens;

/// <inheritdoc cref="IArUcoGridViewModel" />
public class ArUcoGridViewModel : IArUcoGridViewModel
{
    /// <summary>
    /// The number of available ArUco markers.
    /// <br /><br />
    /// <b>See also:</b> /WallProjections/Scripts/Internal/aruco_generator.py
    /// </summary>
    private const int ArUcoCount = 100;

    /// <inheritdoc />
    public override ImmutableList<ArUco> ArUcoList { get; }

    /// <summary>
    /// Creates a new instance of <see cref="ArUcoGridViewModel"/>
    /// with <see cref="ArUcoCount" /> <see cref="ArUco" /> markers.
    /// </summary>
    public ArUcoGridViewModel()
    {
        ArUcoList = Enumerable.Range(0, ArUcoCount)
            .Select(i => new ArUco(i))
            .ToImmutableList();
    }
}
