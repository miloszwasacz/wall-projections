using System;
using System.Diagnostics.CodeAnalysis;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

/// <summary>
/// A dialog with a message and a confirmation button.
/// </summary>
public partial class InfoDialog : ResultDialog
{
    /// <summary>
    /// Creates a new <see cref="InfoDialog"/>.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="iconPath">A <see cref="Uri" /> to the icon of the dialog.</param>
    /// <param name="message">The message displayed by the dialog.</param>
    /// <param name="confirmButtonText">The text for the confirm button.</param>
    public InfoDialog(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText = "Ok"
    )
    {
        InitializeComponent();
#if !RELEASE
        this.AttachDevTools();
#endif
        DataContext = new ConfirmationDialogViewModel(
            title,
            iconPath,
            message,
            confirmButtonText,
            null,
            ""
        );
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="InfoDialog(string, Uri, string, string)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public InfoDialog() => throw new InvalidOperationException(
        "This constructor should not be called. See the documentation for more information."
    );
}
