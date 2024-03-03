using WallProjections.Models.Interfaces;
using WallProjections.Views.SecondaryScreens;

namespace WallProjections.ViewModels.Interfaces;

/// <summary>
/// A viewmodel responsible for displaying the correct content on the secondary window.
/// </summary>
public interface ISecondaryWindowViewModel
{
    /// <summary>
    /// The currently displayed content on the secondary window.
    /// </summary>
    public ViewModelBase? Content { get; }

    /// <summary>
    /// Sets the <see cref="Content" /> to <see cref="HotspotDisplayView" />.
    /// </summary>
    /// <param name="config">The <see cref="IConfig" /> containing data about the hotspots</param>
    public void ShowHotspotDisplay(IConfig config);

    /// <summary>
    /// Sets the <see cref="Content" /> to <see cref="PositionEditorView" />.
    /// </summary>
    public void ShowPositionEditor();

    /// <summary>
    /// Sets the <see cref="Content" /> to <see cref="ArUcoGridView" />.
    /// </summary>
    public void ShowArUcoGrid();
}
