using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace WallProjections.Views;

/// <summary>
/// <inheritdoc cref="TextBlock" />
/// It justifies text apart from the last line.
/// </summary>
/// <remarks>
/// This is a workaround for this <a href="https://github.com/AvaloniaUI/Avalonia/issues/15198">issue</a>.
/// </remarks>
public class JustifiedTextBlock : TextBlock
{
    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected override TextLayout CreateTextLayout(string? text)
    {
        TextAlignment = TextAlignment.Start;
        var textLayoutNormal = base.CreateTextLayout(text);
        TextAlignment = TextAlignment.Justify;
        var textLayoutJustified = base.CreateTextLayout(text);
        var firstLines = textLayoutJustified.TextLines.Take(textLayoutJustified.TextLines.Count - 1);
        var lastLine = textLayoutNormal.TextLines.TakeLast(1);
        var textLines = firstLines.Concat(lastLine).ToArray();
        var field = textLayoutJustified.GetType()
            .GetField("_textLines", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(textLayoutJustified, textLines);
        return textLayoutJustified;
    }
}
