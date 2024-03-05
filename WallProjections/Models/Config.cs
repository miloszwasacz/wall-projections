using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

/// <summary>
/// Stores all user customisable configuration for the program.
/// </summary>
[Serializable]
public class Config : IConfig
{
    /// <summary>
    /// A JSON serializable version of <see cref="HomographyMatrix" />.
    /// </summary>
    [JsonInclude, JsonPropertyName("HomographyMatrix")]
    public double[][] HomographyMatrixJson { get; }

    /// <inheritdoc />
    [JsonIgnore]
    public double[,] HomographyMatrix { get; }

    /// <inheritdoc />
    [JsonInclude]
    public ImmutableList<Hotspot> Hotspots { get; }

    /// <summary>
    /// Constructs a new Config object using list of hotspots and a custom location.
    /// </summary>
    /// <param name="homographyMatrix">Matrix for camera calibration.</param>
    /// <param name="hotspots">Collection of hotspots to create config with.</param>
    public Config(double[,] homographyMatrix, IEnumerable<Hotspot> hotspots)
    {
        HomographyMatrixJson = ConvertToArray(homographyMatrix);
        HomographyMatrix = homographyMatrix;
        Hotspots = hotspots.ToImmutableList();
    }

    /// <summary>
    /// Specific constructor so deserializer matches parameters correctly.
    /// </summary>
    /// <param name="homographyMatrixJson">Matrix for camera calibration.</param>
    /// <param name="hotspots">List of hotspots.</param>
    /// <exception cref="ArgumentNullException">If any parameters are not defined.</exception>
    [JsonConstructor]
    public Config(double[][] homographyMatrixJson, ImmutableList<Hotspot> hotspots)
    {
        HomographyMatrixJson = homographyMatrixJson ?? throw new ArgumentNullException(
            nameof(homographyMatrixJson),
            "Homography matrix cannot be null."
        );
        HomographyMatrix = ConvertToMatrix(homographyMatrixJson);
        Hotspots = hotspots ?? throw new ArgumentNullException(
            nameof(hotspots),
            "Hotspots cannot be null."
        );
    }

    /// <inheritdoc />
    public Hotspot? GetHotspot(int id)
    {
        return Hotspots.Find(x => x.Id == id);
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    /// <summary>
    /// Converts an array of arrays to a 2D array.
    /// </summary>
    private static double[,] ConvertToMatrix(double[][] array)
    {
        var result = new double[array.Length, array[0].Length];
        for (var i = 0; i < array.Length; i++)
        {
            for (var j = 0; j < array[i].Length; j++)
                result[i, j] = array[i][j];
        }

        return result;
    }

    /// <summary>
    /// Converts a 2D array to an array of arrays.
    /// </summary>
    private static double[][] ConvertToArray(double[,] matrix)
    {
        var height = matrix.GetLength(0);
        var width = matrix.GetLength(1);
        var result = new double[height][];
        for (var i = 0; i < height; i++)
        {
            result[i] = new double[width];
            for (var j = 0; j < width; j++)
                result[i][j] = matrix[i, j];
        }

        return result;
    }
}
