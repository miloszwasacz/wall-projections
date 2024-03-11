using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WallProjections.Views;

public partial class IconButton : Button
{
    protected override Type StyleKeyOverride => typeof(Button);

    static IconButton()
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

    public IconButton()
    {
        InitializeComponent();
    }
}
