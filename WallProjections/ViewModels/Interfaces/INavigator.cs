using System;
using System.Collections.Immutable;
using Avalonia;

namespace WallProjections.ViewModels.Interfaces;

/// <summary>
/// An object for application-wide navigation between views.
/// </summary>
public interface INavigator : IDisposable
{
    /// <summary>
    /// Opens the Editor.
    /// </summary>
    public void OpenEditor();

    /// <summary>
    /// Closes the Editor.
    /// </summary>
    public void CloseEditor();

    /// <summary>
    /// Shows the calibration markers on the secondary display.
    /// </summary>
    /// <remarks>This method has to be called on the UI thread.</remarks>
    public void ShowCalibrationMarkers();

    /// <summary>
    /// Hides the calibration markers if they are currently visible.
    /// </summary>
    /// <remarks>This method has to be called on the UI thread.</remarks>
    public void HideCalibrationMarkers();

    /// <summary>
    /// If the calibration markers are visible, returns the positions of the ArUco markers (ID to top-left corner)
    /// otherwise, returns null.
    /// </summary>
    /// <remarks>This method has to be called on the UI thread.</remarks>
    public ImmutableDictionary<int, Point>? GetArUcoPositions();

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    public void Shutdown(ExitCode exitCode = ExitCode.Success);
}
