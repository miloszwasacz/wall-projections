using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Avalonia.Controls;
using WallProjections.Models;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

public partial class CameraChooserDialog : Window
{
    /// <summary>
    /// Whether the selection event has been handled.
    /// </summary>
    private bool _handled;

    public CameraChooserDialog()
    {
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
    }

    // ReSharper disable once UnusedParameter.Local
    /// <summary>
    /// Handles the selection of a camera.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments, containing the selected camera.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the selected camera or the viewmodel is invalid.
    /// </exception>
    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        lock (this)
        {
            if (_handled)
                return;

            _handled = true;
        }

        if (e.AddedItems[0] is not Camera camera)
            throw new ArgumentException("Invalid camera selection");

        if (DataContext is not CameraChooserViewModel viewModel)
            throw new ArgumentException("Invalid viewmodel");

        Hide();
        viewModel.CameraSelected(camera);
    }
}

/// <summary>
/// A viewmodel for the <see cref="CameraChooserDialog"/>.
/// </summary>
public class CameraChooserViewModel
{
    /// <summary>
    /// A callback for when a camera is selected.
    /// </summary>
    private readonly Action<Camera> _cameraSelected;

    /// <summary>
    /// An iterator of cameras.
    /// </summary>
    public IEnumerable<Camera> Cameras { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraChooserViewModel"/>.
    /// </summary>
    /// <param name="cameras">A dictionary of available cameras.</param>
    /// <param name="cameraSelected">A callback for when a camera is selected.</param>
    public CameraChooserViewModel(
        ImmutableList<Camera> cameras,
        Action<Camera> cameraSelected
    )
    {
        Cameras = cameras;
        _cameraSelected = cameraSelected;
    }

    /// <summary>
    /// Selects a camera, invoking the <see cref="_cameraSelected"/> callback.
    /// </summary>
    /// <param name="camera">The selected camera.</param>
    public void CameraSelected(Camera camera) => _cameraSelected(camera);
}
