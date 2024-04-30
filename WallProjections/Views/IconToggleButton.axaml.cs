using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace WallProjections.Views;

public partial class IconToggleButton : ToggleButton
{
    protected override Type StyleKeyOverride => typeof(ToggleButton);

    static IconToggleButton()
    {
        AffectsRender<PathIcon>(IconDataProperty);
    }

    /// <summary>
    /// A <see cref="StyledProperty{Geometry}">StyledProperty</see> that defines the <see cref="IconData" /> property.
    /// </summary>
    public static readonly StyledProperty<Geometry> IconDataProperty =
        AvaloniaProperty.Register<PathIcon, Geometry>(nameof(IconData));

    /// <inheritdoc cref="PathIcon.Data" />
    public Geometry IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public IconToggleButton()
    {
        InitializeComponent();
    }
}
