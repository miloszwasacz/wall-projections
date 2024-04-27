using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
#if !RELEASE
using Avalonia;
#endif

namespace WallProjections.Views;

/// <summary>
/// A generic confirmation dialog.
/// </summary>
public partial class ConfirmationDialog : ResultDialog
{
    /// <summary>
    /// Creates a new <see cref="ConfirmationDialog"/>.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="iconPath">A <see cref="Uri" /> to the icon of the dialog.</param>
    /// <param name="message">The message displayed by the dialog.</param>
    /// <param name="confirmButtonText">The text for the confirm button.</param>
    /// <param name="refuseButtonText">The text for the refuse button. If null or empty, the button is hidden.</param>
    /// <param name="cancelButtonText">The text for the cancel button.</param>
    public ConfirmationDialog(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string? refuseButtonText = null,
        string cancelButtonText = "Cancel"
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
            refuseButtonText,
            cancelButtonText
        );
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This method should not be called. Use <see cref="ResultDialog.ShowDialog(Window)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [Obsolete("This method should not be called. See the documentation for more information.", true)]
    public new Task<TResult> ShowDialog<TResult>(Window _) => throw new InvalidOperationException(
        "This method should not be called. See the documentation for more information."
    );

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This constructor should not be called. It is only here to suppress an Avalonia warning.
    /// Use <see cref="ConfirmationDialog(string, Uri, string, string, string?, string)" /> instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This constructor should not be called. See the documentation for more information.", true)]
    public ConfirmationDialog() => throw new InvalidOperationException(
        "This constructor should not be called. See the documentation for more information."
    );
}

/// <summary>
/// A viewmodel for <see cref="ConfirmationDialog" />.
/// </summary>
internal class ConfirmationDialogViewModel
{
    public string Title { get; }
    public WindowIcon Icon { get; }
    public string Message { get; }
    public string ConfirmButtonText { get; }
    public string RefuseButtonText { get; }
    public string CancelButtonText { get; }
    public bool HasRefuseButton { get; }

    public ConfirmationDialogViewModel(
        string title,
        Uri iconPath,
        string message,
        string confirmButtonText,
        string? refuseButtonText,
        string cancelButtonText
    )
    {
        Title = title;
        Icon = new WindowIcon(new Bitmap(AssetLoader.Open(iconPath)));
        Message = message;
        ConfirmButtonText = confirmButtonText;
        RefuseButtonText = refuseButtonText ?? "";
        CancelButtonText = cancelButtonText;
        HasRefuseButton = !string.IsNullOrEmpty(RefuseButtonText);
    }
}
