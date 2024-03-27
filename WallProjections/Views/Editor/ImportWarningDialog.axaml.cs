using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WallProjections.ViewModels.Interfaces.Editor;

namespace WallProjections.Views.Editor;

public partial class ImportWarningDialog : Window
{
    /// <summary>
    /// A part of the message that is the same for all warning levels.
    /// </summary>
    private const string GenericMessage =
        "Importing this file will overwrite the current data.\n\nAre you sure you want to continue?";

    /// <summary>
    /// The path to the file that is being imported.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// Creates a new <see cref="ImportWarningDialog"/>.
    /// </summary>
    /// <param name="filePath">The path to the file that is being imported.</param>
    /// <param name="warningLevel">The level of the warning to show.</param>
    public ImportWarningDialog(string filePath, ImportWarningLevel warningLevel)
    {
        InitializeComponent();
        _filePath = filePath;
        MessageTextBlock.Text = $"{warningLevel.ToWarningText()} {GenericMessage}";
    }

    // ReSharper disable UnusedParameter.Local

    /// <summary>
    /// Imports the file and closes the dialog.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void ImportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IImportViewModel vm) return;

        //TODO Use the result of ImportFromFile to show an error message
        vm.ImportFromFile(_filePath);
        Close();
    }

    /// <summary>
    /// Closes the dialog without importing the file.
    /// </summary>
    /// <param name="sender">The sender of the event (unused).</param>
    /// <param name="e">The event arguments (unused).</param>
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    // ReSharper restore UnusedParameter.Local

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="ImportWarningDialog(string, ImportWarningLevel)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public ImportWarningDialog()
    {
        throw new InvalidOperationException(
            "This constructor should not be called. See the documentation for more information.");
    }
}
