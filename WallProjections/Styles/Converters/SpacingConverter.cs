using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace WallProjections.Styles.Converters;

/// <summary>
/// Converts a <see cref="GridLength" /> to a <see cref="double" />.
/// </summary>
public class SpacingConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GridLength gridLength && targetType == typeof(double))
            return gridLength.Value;

        return 0;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d && targetType == typeof(GridLength))
            return new GridLength(d);

        return GridLength.Auto;
    }
}
