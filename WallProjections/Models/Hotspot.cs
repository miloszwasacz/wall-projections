using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json.Serialization;

namespace WallProjections.Models;

/// <summary>
/// Stores the location and size of a hotspot
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
    /// Path to a text file containing the description of the hotspot.
    /// </summary>
    [JsonInclude]
    public string DescriptionPath { get; }

    /// <summary>
    /// Fully expanded path to description for hotspot.
    /// </summary>
    public string FullDescriptionPath => DescriptionPath is not null ? Path.Combine(FileHandler.ConfigFolderPath, DescriptionPath): "";

    /// <summary>
    /// A list of paths to images to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> ImagePaths { get; }

    /// <summary>
    /// List of all image paths in fully expanded form.
    /// </summary>
    public ImmutableList<string> FullImagePaths =>
        ImagePaths.ConvertAll(item => Path.Combine(FileHandler.ConfigFolderPath, item));

    /// <summary>
    /// A list of paths to videos to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> VideoPaths { get; }
    
    /// <summary>
    /// List of all video paths in fully expanded form.
    /// </summary>
    public ImmutableList<string> FullVideoPaths =>
        VideoPaths.ConvertAll(item => Path.Combine(FileHandler.ConfigFolderPath, item));

    /// <summary>
    /// Constructor for Hotspot used by JSON deserializer
    /// </summary>
    /// <param name="id">ID used by input detection to show info.</param>
    /// <param name="position">Position of hotspot stored as <see cref="Coord"/> record.</param>
    /// <param name="descriptionPath">Path to text file containing description of hotspot.</param>
    /// <param name="imagePaths">List of paths to images to be displayed in hotspot.</param>
    /// <param name="videoPaths">List of paths to videos to be displayed in hotspot.</param>
    /// <exception cref="ArgumentException">Thrown if both image + video at once, or no content defined.</exception>
    [JsonConstructor]
    public Hotspot(int id, Coord position, string descriptionPath, ImmutableList<string> imagePaths, ImmutableList<string> videoPaths)
    {
        Id = id;
        Position = position;
        DescriptionPath = descriptionPath;
        ImagePaths = imagePaths;
        VideoPaths = videoPaths;
    }

    // TODO Add support for multiple images/videos
    public record Media(
        int Id,
        string Description,
        ImmutableList<string> ImagePaths = null,
        ImmutableList<string> VideoPaths = null);
}
