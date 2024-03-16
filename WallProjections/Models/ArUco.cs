using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WallProjections.Models;

/// <summary>
/// A record to hold the info about an ArUco marker.
/// </summary>
/// <param name="Id">The id of the marker.</param>
public record ArUco(int Id)
{
    /// <summary>
    /// The image of the marker.
    /// </summary>
    public Bitmap Image { get; } =
        new(AssetLoader.Open(new Uri($"avares://WallProjections/Assets/ArUcos/ArUco_{Id}.png")));
}
