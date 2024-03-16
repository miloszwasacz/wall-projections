using System;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;

namespace WallProjections.Views.SecondaryScreens;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class ArUcoGridView : UserControl
{
    public ArUcoGridView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the positions of all displayed ArUco markers in the grid.
    /// </summary>
    /// <returns>A dictionary of ArUco ids and their positions,
    /// or <i>null</i> if the <see cref="ArUcoGridView.DataContext" /> is not an <see cref="AbsArUcoGridViewModel"/>.
    /// </returns>
    public ImmutableDictionary<int, Point> GetArUcoPositions() => this.GetVisualDescendants().OfType<Image>()
        .Where(image => image.Tag is ArUco)
        .ToImmutableDictionary(
            image => (image.Tag as ArUco)!.Id,
            image => image.TranslatePoint(image.Bounds.TopLeft, this) ??
                     throw new NullReferenceException("No common ancestor found.")
        );
}
