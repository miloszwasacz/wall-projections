﻿using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace WallProjections.ViewModels.Interfaces.Display;

public interface IImageViewModel : IDisposable
{
    /// <summary>
    /// The time to show each image before moving to the next image.
    /// </summary>
    public static TimeSpan DefaultImageInterval => TimeSpan.FromSeconds(20);
    
    /// <summary>
    /// The image to be displayed.
    /// </summary>
    public Bitmap? Image { get; }

    /// <summary>
    /// Whether the viewmodel has images to display.
    /// </summary>
    public bool HasImages { get; }

    /// <summary>
    /// The number of images currently imported.
    /// </summary>
    public int ImageCount { get; }

    /// <summary>
    /// Add images to the slideshow.
    /// </summary>
    /// <param name="imagePaths">Image paths to add to slideshow.</param>
    /// <returns><i>true</i> if all images are added successfully, <i>false</i> otherwise.</returns>
    public bool AddImages(IEnumerable<string> imagePaths);

    /// <summary>
    /// Start the slideshow of images imported.
    /// </summary>
    /// <param name="interval">Time between images being updated. If empty, then interval is <see cref="DefaultImageInterval"/></param>
    /// <exception cref="InvalidOperationException">If no images added to <see cref="IImageViewModel"/> (See <see cref="AddImages"/>)</exception>
    public void StartSlideshow(TimeSpan? interval = null);

    /// <summary>
    /// Stop current slideshow from running.
    /// </summary>
    public void StopSlideshow();

    /// <summary>
    /// Removes all current images from the viewmodel.
    /// </summary>
    public void ClearImages();
}
