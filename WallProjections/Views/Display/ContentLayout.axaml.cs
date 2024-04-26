using Avalonia;
using Avalonia.Controls;
using WallProjections.ViewModels.Interfaces.Display.Layouts;

namespace WallProjections.Views.Display;

/// <summary>
/// A generic view to be used in different <see cref="Layout" />s of hotspot content.
/// </summary>
public class ContentLayout : ContentControl
{
    /// <summary>
    /// The <see cref="DirectProperty{T, TRet}" /> for <see cref="HotspotTitle" />.
    /// </summary>
    public static readonly DirectProperty<ContentLayout, string> HotspotTitleProperty =
        AvaloniaProperty.RegisterDirect<ContentLayout, string>(
            nameof(HotspotTitle),
            o => o.HotspotTitle,
            (o, v) => o.HotspotTitle = v
        );

    /// <summary>
    /// The backing field for <see cref="HotspotTitle"/>.
    /// </summary>
    private string _hotspotTitle = "";

    /// <summary>
    /// The title of the hotspot.
    /// </summary>
    public string HotspotTitle
    {
        get => _hotspotTitle;
        set => SetAndRaise(HotspotTitleProperty, ref _hotspotTitle, value);
    }
}
