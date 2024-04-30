using System;
using System.IO;
using Microsoft.Extensions.Logging;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IImportViewModel" />
public class ImportViewModel : ViewModelBase, IImportViewModel
{
    /// <summary>
    /// A logger for this class.
    /// </summary>
    private readonly ILogger _logger;

    /// <inheritdoc />
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    /// <summary>
    /// Creates a new <see cref="ImportViewModel"/> belonging to the given <see cref="IDescriptionEditorViewModel"/>.
    /// </summary>
    /// <param name="descriptionEditor">The parent <see cref="IDescriptionEditorViewModel"/>.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    public ImportViewModel(IDescriptionEditorViewModel descriptionEditor, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ImportViewModel>();
        DescriptionEditor = descriptionEditor;
    }

    /// <inheritdoc />
    public ImportWarningLevel IsImportSafe()
    {
        var titleEmpty = string.IsNullOrWhiteSpace(DescriptionEditor.Title);
        var descriptionEmpty = string.IsNullOrWhiteSpace(DescriptionEditor.Description);

        return (titleEmpty, descriptionEmpty) switch
        {
            (true, true) => ImportWarningLevel.None,
            (false, true) => ImportWarningLevel.Title,
            (true, false) => ImportWarningLevel.Description,
            (false, false) => ImportWarningLevel.Both
        };
    }

    /// <inheritdoc />
    public bool ImportFromFile(string path)
    {
        try
        {
            var lines = File.ReadAllLines(path);
            if (lines.Length == 0)
            {
                _logger.LogWarning("File {Path} is empty.", path);
                return true;
            }

            DescriptionEditor.Title = lines[0].Trim();
            DescriptionEditor.Description = lines.Length > 1
                ? string.Join("\n", lines[1..]).Trim()
                : "";

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import from file {Path}.", path);
            return false;
        }
    }
}
