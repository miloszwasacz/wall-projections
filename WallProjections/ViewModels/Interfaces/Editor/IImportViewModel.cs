using System;

namespace WallProjections.ViewModels.Interfaces.Editor;

/// <summary>
/// A viewmodel for importing hotspot's title and description from a file.
/// </summary>
public interface IImportViewModel
{
    /// <summary>
    /// The parent <see cref="IDescriptionEditorViewModel"/> to which the data is imported.
    /// </summary>
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    /// <summary>
    /// Checks whether the title and description are empty.
    /// </summary>
    /// <returns>The level of warning to show.</returns>
    public ImportWarningLevel IsImportSafe();

    /// <summary>
    /// Imports the hotspot's title and description from a file. The first line is the title,
    /// then a blank line, then the description.
    /// </summary>
    /// <param name="path">A path to the file containing the description.</param>
    public void ImportFromFile(string path);
}

/// <summary>
/// A level of warning to show when the title and/or description are empty.
/// </summary>
public enum ImportWarningLevel
{
    /// <summary>
    /// Both the title and description are empty.
    /// </summary>
    None,

    /// <summary>
    /// The description is empty, but the title is not.
    /// </summary>
    Title,

    /// <summary>
    /// The title is empty, but the description is not.
    /// </summary>
    Description,

    /// <summary>
    /// Both the title and description are not empty.
    /// </summary>
    Both
}

/// <summary>
/// Extension methods for <see cref="ImportWarningLevel"/>.
/// </summary>
public static class ImportWarningLevelExtensions
{
    /// <summary>
    /// Converts a <see cref="ImportWarningLevel"/> to a string to show in a warning dialog.
    /// </summary>
    /// <param name="level">The level of warning.</param>
    /// <returns>The text to show in a warning dialog.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="level"/> is not a valid <see cref="ImportWarningLevel"/>.
    /// </exception>
    public static string ToWarningText(this ImportWarningLevel level)
    {
        return level switch
        {
            ImportWarningLevel.None => "",
            ImportWarningLevel.Title => "The title is not empty.",
            ImportWarningLevel.Description => "The description is not empty.",
            ImportWarningLevel.Both => "The title and description are not empty.",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}
