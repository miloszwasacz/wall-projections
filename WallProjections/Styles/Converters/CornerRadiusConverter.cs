using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WallProjections.Styles.Converters;

/// <summary>
/// A converter that converts applies <see cref="CornerRadiusExtensions.PiecewiseMultiply" />
/// to the input value using parameter.
/// </summary>
public class CornerRadiusConverter : IValueConverter
{
    /// <summary>
    /// Converts the input value to a <see cref="CornerRadius" /> by applying
    /// <see cref="CornerRadiusExtensions.PiecewiseMultiply" /> with the parameter if it is provided.
    /// </summary>
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var radii = value switch
            {
                double d => new CornerRadius(d),
                CornerRadius r => r,
                string s => CornerRadius.Parse(s),
                _ => throw new ArgumentException("Invalid type of value")
            };
            if (parameter is null)
                return radii;

            var factor = parameter switch
            {
                double d => new CornerRadius(d),
                CornerRadius r => r,
                string s => CornerRadius.Parse(s),
                _ => throw new ArgumentException("Invalid type of parameter")
            };

            return radii.PiecewiseMultiply(factor);
        }
        catch (ArgumentException e)
        {
            return new BindingNotification(e, BindingErrorType.Error);
        }
    }

    /// <summary>
    /// This converter does not support converting back.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new InvalidOperationException();
}

/// <summary>
/// A multi-value converter that aggregates <see cref="CornerRadius" /> values
/// into a single <see cref="CornerRadius" /> value using <see cref="CornerRadiusExtensions.PiecewiseAdd" />.
/// </summary>
public class CornerRadiusMultiConverter : AggregateMultiConverter<CornerRadius, CornerRadius, CornerRadius>
{
    public CornerRadiusMultiConverter() : base(
        new CornerRadiusConverter(),
        (acc, r) => acc.PiecewiseAdd(r),
        (acc, param) => param switch
        {
            null => acc,
            double d => new CornerRadius(d),
            CornerRadius r => r,
            string s => CornerRadius.Parse(s),
            _ => throw new ArgumentException("Invalid type of parameter")
        }
    )
    {
    }
}

/// <summary>
/// Extension methods for <see cref="CornerRadius" />.
/// </summary>
public static class CornerRadiusExtensions
{
    /// <summary>
    /// Adds two <see cref="CornerRadius" /> values piecewise.
    /// </summary>
    public static CornerRadius PiecewiseAdd(this CornerRadius r1, CornerRadius r2) => new(
        r1.TopLeft + r2.TopLeft,
        r1.TopRight + r2.TopRight,
        r1.BottomRight + r2.BottomRight,
        r1.BottomLeft + r2.BottomLeft
    );

    /// <summary>
    /// Multiplies two <see cref="CornerRadius" /> values piecewise.
    /// </summary>
    public static CornerRadius PiecewiseMultiply(this CornerRadius r1, CornerRadius r2) => new(
        r1.TopLeft * r2.TopLeft,
        r1.TopRight * r2.TopRight,
        r1.BottomRight * r2.BottomRight,
        r1.BottomLeft * r2.BottomLeft
    );
}
