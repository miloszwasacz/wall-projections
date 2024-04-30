using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
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

    /// <inheritdoc cref="TextBlock.CreateTextLayout" />
    /// <remarks>
    /// This method mirrors <see cref="TextBlock.CreateTextLayout" />, but with a custom alignment.
    /// </remarks>
    private TextLayout CreateTextLayout(string? text, TextAlignment alignment)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        var defaultProperties = new GenericTextRunProperties(
            typeface,
            FontSize,
            TextDecorations,
            Foreground);

        var paragraphProperties = new GenericTextParagraphProperties(
            FlowDirection,
            alignment,
            true,
            false,
            defaultProperties,
            TextWrapping,
            LineHeight,
            0,
            LetterSpacing
        );

        var inlinesTextSourceType = typeof(TextBlock).GetNestedType("InlinesTextSource", BindingFlags.NonPublic)!;
        var simpleTextSourceType = typeof(TextBlock).GetNestedType("SimpleTextSource", BindingFlags.NonPublic)!;

        var textRunsField = typeof(TextBlock).GetField("_textRuns", BindingFlags.NonPublic | BindingFlags.Instance);
        var textSource = textRunsField?.GetValue(this) is IReadOnlyList<TextRun> textRuns
            ? (ITextSource)Activator.CreateInstance(inlinesTextSourceType, textRuns)!
            : (ITextSource)Activator.CreateInstance(simpleTextSourceType, text ?? "", defaultProperties)!;

        var constraintField = typeof(TextBlock).GetField("_constraint", BindingFlags.NonPublic | BindingFlags.Instance);
        if (constraintField?.GetValue(this) is not Size constraint)
            throw new InvalidOperationException("Constraint is null");

        return new TextLayout(
            textSource,
            paragraphProperties,
            TextTrimming,
            constraint.Width,
            constraint.Height,
            MaxLines
        );
    }

    protected override TextLayout CreateTextLayout(string? text)
    {
        var textLayoutNormal = CreateTextLayout(text, TextAlignment.Start);
        var textLayoutJustified = CreateTextLayout(text, TextAlignment.Justify);

        var textLines = new List<TextLine>(textLayoutNormal.TextLines.Count);
        for (var i = 0; i < textLayoutNormal.TextLines.Count; i++)
        {
            var lineNormal = textLayoutNormal.TextLines[i];
            var lineJustified = textLayoutJustified.TextLines[i];
            var splitToNewLine = lineNormal.TextLineBreak?.IsSplit ?? false;
            textLines.Add(splitToNewLine ? lineJustified : lineNormal);
        }

        var field = textLayoutJustified.GetType()
            .GetField("_textLines", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(textLayoutJustified, textLines.ToArray());
        return textLayoutJustified;
    }
}
