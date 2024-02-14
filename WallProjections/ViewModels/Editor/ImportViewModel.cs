using System;
using System.IO;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.ViewModels.Editor;

/// <inheritdoc cref="IImportViewModel" />
public class ImportViewModel : ViewModelBase, IImportViewModel
{
    /// <inheritdoc />
    public IDescriptionEditorViewModel DescriptionEditor { get; }

    /// <summary>
    /// Creates a new <see cref="ImportViewModel"/> belonging to the given <see cref="IDescriptionEditorViewModel"/>.
    /// </summary>
    /// <param name="descriptionEditor">The parent <see cref="IDescriptionEditorViewModel"/>.</param>
    public ImportViewModel(IDescriptionEditorViewModel descriptionEditor)
    {
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
            if (lines.Length == 0) return true;

            DescriptionEditor.Title = lines[0].Trim();
            DescriptionEditor.Description = lines.Length > 1
                ? string.Join("\n", lines[1..]).Trim()
                : "";

            return true;
        }
        catch (Exception e)
        {
            //TODO Log to file
            Console.Error.WriteLine(e);
            return false;
        }
    }
}
