using System;
using System.Text.Json.Serialization;

namespace WallProjections.Configuration;

/// <summary>
/// Stores the location of a hotspot, its size, and the content to be displayed.
/// </summary>
[Serializable]
public class Hotspot
{
    /// <summary>
    /// ID of the hotspot. Used by input to tell UI which hotspot info to show.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Path to file/filename for text to show.
    /// </summary>
    public string TextFile { get; }

    /// <summary>
    /// Path to file/filename for image file to show.
    /// </summary>
    public string? ImgFile { get; }

    /// <summary>
    /// Path to file/filename for a video file to show.
    /// </summary>
    public string? VideoFile { get; }

    /// <summary>
    /// X value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? X { get; }

    /// <summary>
    /// Y value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? Y { get; }

    /// <summary>
    /// Radius of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? R { get; }

    /// <summary>
    /// Constructor for Hotspot
    /// </summary>
    /// <param name="id">ID used by input detection to show info.</param>
    /// <param name="textFile">Filename + path which stores text for hotspot.</param>
    /// <param name="imgFile">Filename + path which stores image for hotspot.</param>
    /// <param name="videoFile">Filename + path which stores video for hotspot.</param>
    /// <param name="x">X value for hotspot in camera. If not default, then </param>
    /// <param name="y">Y value for hotspot in camera.</param>
    /// <param name="r">Radius for hotspot in camera.</param>
    /// <exception cref="ArgumentException">Thrown if both image + video at once, or no content defined.</exception>
    public Hotspot(
        int id,
        string textFile,
        string? imgFile = null,
        string? videoFile = null,
        double? x = default,
        double? y = default,
        double? r = default
        )
    {
        Id = id;

        if (imgFile != null && videoFile != null)
        {
            throw new ArgumentException("Cannot have both an image and a video.");
        }

        TextFile = textFile;
        ImgFile = imgFile;
        VideoFile = videoFile;

        X = x;
        Y = y;
        R = r;
    }
}
