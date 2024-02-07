using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json.Serialization;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

/// <summary>
/// Stores data about a hotspot -- position, title, description, and media.
/// </summary>
[Serializable]
public class Hotspot
{
    /// <summary>
    /// Path to the folder containing the imported media files and config.
    /// </summary>
    [JsonIgnore] private readonly string _filePath;

    /// <summary>
    /// ID of the hotspot. Used by input to tell UI which hotspot info to show.
    /// </summary>
    [JsonInclude]
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
    /// Fully expanded path to description for hotspot.
    /// </summary>
    [JsonIgnore]
    public string FullDescriptionPath => Path.Combine(_filePath, DescriptionPath);

    /// <summary>
    /// A list of paths to images to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> ImagePaths { get; }

    /// <summary>
    /// List of all image paths in fully expanded form.
    /// </summary>
    [JsonIgnore]
    public ImmutableList<string> FullImagePaths =>
        ImagePaths.ConvertAll(item => Path.Combine(_filePath, item));

    /// <summary>
    /// A list of paths to videos to be displayed in the hotspot.
    /// </summary>
    [JsonInclude]
    public ImmutableList<string> VideoPaths { get; }

    /// <summary>
    /// List of all video paths in fully expanded form.
    /// </summary>
    [JsonIgnore]
    public ImmutableList<string> FullVideoPaths =>
        VideoPaths.ConvertAll(item => Path.Combine(_filePath, item));

    // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    /// <summary>
    /// Constructor for the Hotspot used by JSON deserializer
    /// </summary>
    /// <param name="id">ID used by input detection to show info.</param>
    /// <param name="position">Position of hotspot stored as <see cref="Coord"/> record.</param>
    /// <param name="title">Title of the hotspot.</param>
    /// <param name="descriptionPath">Path to text file containing description of hotspot.</param>
    /// <param name="imagePaths">List of paths to images to be displayed in hotspot.</param>
    /// <param name="videoPaths">List of paths to videos to be displayed in hotspot.</param>
    /// <exception cref="ArgumentNullException">If any parameters are not defined.</exception>
    [JsonConstructor]
    public Hotspot(
        int id,
        Coord position,
        string title,
        string descriptionPath,
        ImmutableList<string> imagePaths,
        ImmutableList<string> videoPaths
    )
    {
        Id = id;
        Position = position ?? throw new ArgumentNullException(nameof(position), "Position cannot be null");
        Title = title ?? throw new ArgumentNullException(nameof(title), "Title cannot be null");
        DescriptionPath = descriptionPath ??
                          throw new ArgumentNullException(nameof(descriptionPath), "DescriptionPath cannot be null");
        ImagePaths = imagePaths ?? throw new ArgumentNullException(nameof(imagePaths), "ImagePaths cannot be null");
        VideoPaths = videoPaths ?? throw new ArgumentNullException(nameof(videoPaths), "VideoPaths cannot be null");
        _filePath = IFileHandler.ConfigFolderPath;
    }
    // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

    public record Media(
        int Id,
        string Title,
        string Description,
        ImmutableList<string> ImagePaths,
        ImmutableList<string> VideoPaths);
}
