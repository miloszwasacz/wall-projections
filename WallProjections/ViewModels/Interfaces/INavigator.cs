using System;

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
    /// Shuts down the application.
    /// </summary>
    public void Shutdown();
}
