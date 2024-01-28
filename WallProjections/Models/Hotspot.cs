using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace WallProjections.Models;

/// <summary>
/// Stores data about a hotspot -- position, title, description, and media.
/// </summary>
[Serializable]
public class Hotspot
{
    /// <summary>
    /// ID of the hotspot. Used by input to tell UI which hotspot info to show.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Record of coordinates for hotspot position (X, Y, Radius)
    /// </summary>
    [JsonInclude]
    public Coord Position { get; }

    /// <summary>
    /// The title of the hotspot.
    /// </summary>
    [JsonInclude]
    public string Title { get; }

    /// <summary>
    /// Path to a text file containing the description of the hotspot.
    /// </summary>
    [JsonInclude]
    public string DescriptionPath { get; }

    /// <summary>
    /// A list of paths to images to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> ImagePaths { get; }

    /// <summary>
    /// A list of paths to videos to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> VideoPaths { get; }

    /// <summary>
    /// Constructor for the Hotspot used by JSON deserializer
    /// </summary>
    /// <param name="id">ID used by input detection to show info.</param>
    /// <param name="position">Position of the hotspot stored as <see cref="Coord"/> record.</param>
    /// <param name="title">Title of the hotspot.</param>
    /// <param name="descriptionPath">Path to text file containing description of the hotspot.</param>
    /// <param name="imagePaths">List of paths to images to be displayed in the hotspot.</param>
    /// <param name="videoPaths">List of paths to videos to be displayed in the hotspot.</param>
    [JsonConstructor]
    public Hotspot(int id,
        Coord position,
        string title,
        string descriptionPath,
        ImmutableList<string> imagePaths,
        ImmutableList<string> videoPaths)
    {
        Id = id;
        Position = position;
        Title = title;
        DescriptionPath = descriptionPath;
        ImagePaths = imagePaths;
        VideoPaths = videoPaths;
    }

    // TODO Add support for multiple images/videos
    public record Media(string Description, string? ImagePath = null, string? VideoPath = null);
}
