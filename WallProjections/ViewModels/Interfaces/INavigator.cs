using System;
using System.Collections.Generic;

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
    public void ShowCalibrationMarkers();

    /// <summary>
    /// Hides the calibration markers if they are currently visible.
    /// </summary>
    public void HideCalibrationMarkers();

    /// <summary>
    /// If the calibration markers are visible, returns the positions of the ArUco markers (ID to top-left corner)
    /// otherwise, returns null.
    /// </summary>
    public Dictionary<int, (float, float)>? GetArUcoPositions();

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    public void Shutdown();
}
