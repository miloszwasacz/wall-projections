using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace WallProjections.Views;

public class IconButton : Button
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
        VerticalAlignment = VerticalAlignment.Stretch;
        Padding = new Thickness(10);

        // Maintain square aspect ratio based on height
        var actualHeightProperty = this.GetObservable(BoundsProperty).Select(b => b.Height);
        Bind(WidthProperty, actualHeightProperty);

        Content = new Viewbox
        {
            Child = new PathIcon
            {
                Width = 20, Height = 20,
                [!PathIcon.DataProperty] = this[!IconDataProperty]
            }
        };
    }
}
