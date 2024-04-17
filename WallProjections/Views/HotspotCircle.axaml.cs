using System;
using System.Globalization;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace WallProjections.Views;

/// <summary>
/// A circle representing a hotspot. Set the <see cref="StyledElement.DataContext" />
/// </summary>
public partial class HotspotCircle : Panel, IDisposable
{
    /// <summary>
    /// The time required to pulse the hotspot (one direction only)
    /// </summary>
    public static readonly TimeSpan PulseTime = TimeSpan.FromSeconds(1);

    #region Styled/Direct properties

    /// <summary>
    /// A <see cref="StyledProperty{T}">StyledProperty</see> that defines the <see cref="Diameter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> DiameterProperty =
        AvaloniaProperty.Register<HotspotCircle, double>(nameof(Diameter));

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="IsActivating" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> IsActivatingProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(IsActivating),
            o => o.IsActivating,
            (o, v) => o.IsActivating = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="IsDeactivating" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> IsDeactivatingProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(IsDeactivating),
            o => o.IsDeactivating,
            (o, v) => o.IsDeactivating = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="IsActivated" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> IsActivatedProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(IsActivated),
            o => o.IsActivated,
            (o, v) => o.IsActivated = v
        );

    /// <summary>
    /// A <see cref="DirectProperty{T, TP}">DirectProperty</see> that defines the <see cref="Pulse" /> property.
    /// </summary>
    public static readonly DirectProperty<HotspotCircle, bool> PulseProperty =
        AvaloniaProperty.RegisterDirect<HotspotCircle, bool>(
            nameof(Pulse),
            o => o.Pulse,
            (o, v) => o.Pulse = v
        );

    #endregion

    #region Backing fields

    /// <summary>
    /// The subject that holds the value of <see cref="IsActivating" />
    /// </summary>
    private readonly BehaviorSubject<bool> _isActivating = new(false);

    /// <summary>
    /// The subject that holds the value of <see cref="IsDeactivating" />
    /// </summary>
    private readonly BehaviorSubject<bool> _isDeactivating = new(false);

    /// <summary>
    /// The subject that holds the value of <see cref="IsActivated" />
    /// </summary>
    private readonly BehaviorSubject<bool> _isActivated = new(false);

    /// <summary>
    /// The subject that holds the value of <see cref="Pulse" />
    /// </summary>
    private readonly BehaviorSubject<bool> _pulse = new(false);

    #endregion

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    /// <summary>
    /// Whether the hotspot is activating
    /// </summary>
    public bool IsActivating
    {
        get => _isActivating.Value;
        set
        {
            var oldValue = _isActivating.Value;
            _isActivating.OnNext(value);
            RaisePropertyChanged(IsActivatingProperty, oldValue, value);
        }
    }

    /// <summary>
    /// Whether the hotspot is deactivating
    /// </summary>
    public bool IsDeactivating
    {
        get => _isDeactivating.Value;
        set
        {
            var oldValue = _isDeactivating.Value;
            _isDeactivating.OnNext(value);
            RaisePropertyChanged(IsDeactivatingProperty, oldValue, value);
        }
    }

    /// <summary>
    /// Whether the hotspot is fully activated
    /// </summary>
    public bool IsActivated
    {
        get => _isActivated.Value;
        set
        {
            var oldValue = _isActivated.Value;
            _isActivated.OnNext(value);
            lock (_pulse)
            {
                RaisePropertyChanged(IsActivatedProperty, oldValue, value);

                if (value)
                    StartPulsing();
                else
                    Pulse = false;
            }
        }
    }

    /// <summary>
    /// Whether the hotspot should be scaled up
    /// </summary>
    private bool Pulse
    {
        get => _pulse.Value && IsActivated;
        set
        {
            var oldValue = _pulse.Value;
            _pulse.OnNext(value);
            RaisePropertyChanged(PulseProperty, oldValue, value);
        }
    }

    public HotspotCircle()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Starts pulsing the hotspot
    /// </summary>
    /// <remarks>Stops pulsing if the hotspot is no longer <see cref="IsActivated">activated</see></remarks>
    private async void StartPulsing()
    {
        while (true)
        {
            lock (_pulse)
            {
                Pulse = false;

                if (!IsActivated)
                    return;
            }

            await Task.Delay(PulseTime);

            lock (_pulse)
            {
                if (!IsActivated)
                    return;

                Pulse = true;
            }

            await Task.Delay(PulseTime);
        }
    }

    public void Dispose()
    {
        _isActivating.Dispose();
        _isActivated.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class FullArcDiameterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double diameter) return 0.0;

        return diameter * 1.25 + 20;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ArcRadiusConverter : IValueConverter
{
    private readonly FullArcDiameterConverter _diameterConverter = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return _diameterConverter.Convert(value, typeof(double), null, culture) is double diameter
            ? new Size(diameter / 2, diameter / 2)
            : new Size(0.0, 0.0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ArcStartPointConverter : IValueConverter
{
    private readonly FullArcDiameterConverter _diameterConverter = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return _diameterConverter.Convert(value, typeof(double), null, culture) is double diameter
            ? new Point(diameter / 2 + 5, 5)
            : new Point(0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
