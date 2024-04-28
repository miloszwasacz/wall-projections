using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace WallProjections.Styles.Converters;

/// <summary>
/// A converter that converts applies <see cref="ThicknessExtensions.PiecewiseMultiply" />
/// to the input value using parameter.
/// </summary>
public class ThicknessConverter : IValueConverter
{
    /// <summary>
    /// Converts the input value to a <see cref="Thickness" /> by applying
    /// <see cref="ThicknessExtensions.PiecewiseMultiply" /> with the parameter if it is provided.
    /// </summary>
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var thickness = value switch
            {
                double d => new Thickness(d),
                Thickness t => t,
                string s => Thickness.Parse(s),
                _ => throw new ArgumentException("Invalid type of value")
            };
            if (parameter is null)
                return thickness;

            var factor = parameter switch
            {
                double d => new Thickness(d),
                Thickness t => t,
                string s => Thickness.Parse(s),
                _ => throw new ArgumentException("Invalid type of parameter")
            };

            return thickness.PiecewiseMultiply(factor);
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
/// A multi-value converter that aggregates <see cref="Thickness" /> values
/// into a single <see cref="Thickness" /> value using <see cref="Thickness.op_Addition(Thickness,Thickness)" />.
/// </summary>
public class ThicknessMultiConverter : AggregateMultiConverter<Thickness, Thickness, Thickness>
{
    public ThicknessMultiConverter() : base(
        new ThicknessConverter(),
        (acc, t) => acc + t,
        (acc, param) => param switch
        {
            null => acc,
            double d => acc * d,
            Thickness t => acc.PiecewiseMultiply(t),
            string s => acc.PiecewiseMultiply(Thickness.Parse(s)),
            _ => throw new ArgumentException("Invalid type of parameter")
        }
    )
    {
    }
}

/// <summary>
/// Extension methods for <see cref="Thickness" />.
/// </summary>
public static class ThicknessExtensions
{
    /// <summary>
    /// Multiplies two <see cref="Thickness" /> values piecewise.
    /// </summary>
    public static Thickness PiecewiseMultiply(this Thickness t1, Thickness t2) => new(
        t1.Left * t2.Left,
        t1.Top * t2.Top,
        t1.Right * t2.Right,
        t1.Bottom * t2.Bottom
    );
}
