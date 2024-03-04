using System.Collections.Immutable;
using Avalonia;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

public class MockNavigator : INavigator
{
    /// <summary>
    /// The positions returned by <see cref="GetArUcoPositions" />.
    /// </summary>
    private readonly ImmutableDictionary<int, Point>? _arucoPositions;

    /// <summary>
    /// Whether the editor is currently open
    /// (i.e. <see cref="OpenEditor"/> has been called and <see cref="CloseEditor"/> has not been called).
    /// </summary>
    public bool IsEditorOpen { get; private set; }

    /// <summary>
    /// Whether the ArUco markers are currently visible.
    /// </summary>
    public bool AreArUcoMarkersVisible { get; private set; }

    /// <summary>
    /// Whether the <see cref="Shutdown"/> method has been called.
    /// </summary>
    public bool HasBeenShutDown { get; private set; }

    /// <summary>
    /// Whether the <see cref="Dispose"/> method has been called.
    /// </summary>
    public bool HasBeenDisposed { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="MockNavigator" /> with <i>null</i> <see cref="_arucoPositions" />.
    /// </summary>
    public MockNavigator()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="MockNavigator" /> with the given <paramref name="arucoPositions" />.
    /// </summary>
    /// <param name="arucoPositions">The positions returned by <see cref="GetArUcoPositions" />.</param>
    public MockNavigator(ImmutableDictionary<int, Point> arucoPositions)
    {
        _arucoPositions = arucoPositions;
    }

    /// <summary>
    /// Sets <see cref="IsEditorOpen"/> to true.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="HasBeenShutDown"/> is true or <see cref="HasBeenDisposed"/> is true.
    /// </exception>
    public void OpenEditor()
    {
        if (HasBeenShutDown)
            throw new InvalidOperationException("Cannot open editor after shutdown");

        if (HasBeenDisposed)
            throw new InvalidOperationException("Cannot open editor after disposal");

        IsEditorOpen = true;
    }

    /// <summary>
    /// Sets <see cref="IsEditorOpen"/> to false.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="HasBeenShutDown"/> is true or <see cref="HasBeenDisposed"/> is true.
    /// </exception>
    public void CloseEditor()
    {
        if (HasBeenShutDown)
            throw new InvalidOperationException("Cannot close editor after shutdown");

        if (HasBeenDisposed)
            throw new InvalidOperationException("Cannot close editor after disposal");

        IsEditorOpen = false;
    }

    public void ShowCalibrationMarkers()
    {
        AreArUcoMarkersVisible = true;
    }

    public void HideCalibrationMarkers()
    {
        AreArUcoMarkersVisible = false;
    }

    public ImmutableDictionary<int, Point>? GetArUcoPositions() => _arucoPositions;

    /// <summary>
    /// Sets <see cref="HasBeenShutDown"/> to true and calls <see cref="Dispose"/>.
    /// </summary>
    public void Shutdown()
    {
        HasBeenShutDown = true;
        Dispose();
    }

    /// <summary>
    /// Sets <see cref="HasBeenDisposed"/> to true.
    /// </summary>
    public void Dispose()
    {
        HasBeenDisposed = true;
        GC.SuppressFinalize(this);
    }
}
