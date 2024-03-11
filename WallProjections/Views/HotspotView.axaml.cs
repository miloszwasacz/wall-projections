using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WallProjections.Views;

public partial class HotspotView : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    /// <summary>
    /// A <see cref="StyledProperty{Geometry}">StyledProperty</see> that defines the <see cref="HotspotFill" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HotspotFillProperty =
        AvaloniaProperty.Register<HotspotView, IBrush?>(nameof(HotspotFill));

    /// <summary>
    /// A <see cref="StyledProperty{Geometry}">StyledProperty</see> that defines the <see cref="HotspotStroke" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HotspotStrokeProperty =
        AvaloniaProperty.Register<HotspotView, IBrush?>(nameof(HotspotStroke));

    /// <summary>
    /// The fill color of the hotspot
    /// </summary>
    public IBrush? HotspotFill
    {
        get => GetValue(HotspotFillProperty);
        set => SetValue(HotspotFillProperty, value);
    }

    /// <summary>
    /// The stroke color of the hotspot
    /// </summary>
    public IBrush? HotspotStroke
    {
        get => GetValue(HotspotStrokeProperty);
        set => SetValue(HotspotStrokeProperty, value);
    }

    public HotspotView()
    {
        InitializeComponent();
    }
}
