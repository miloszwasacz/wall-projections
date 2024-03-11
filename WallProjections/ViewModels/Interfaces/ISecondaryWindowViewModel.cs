using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Editor;
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
    /// Sets the <see cref="Content" /> to <see cref="PositionEditorView" />
    /// with <paramref name="editorViewModel" />'s <see cref="IEditorViewModel.PositionEditor" /> as the DataContext.
    /// </summary>
    /// <param name="editorViewModel">
    /// The parent <see cref="IEditorViewModel" /> owning the <see cref="IPositionEditorViewModel" /> to be displayed.
    /// </param>
    public void ShowPositionEditor(IEditorViewModel editorViewModel);

    /// <summary>
    /// Sets the <see cref="Content" /> to <see cref="ArUcoGridView" />.
    /// </summary>
    public void ShowArUcoGrid();
}
