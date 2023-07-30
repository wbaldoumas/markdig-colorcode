﻿using ColorCode;
using ColorCode.Styling;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Text;

namespace Markdown.ColorCode;

/// <summary>
///     A renderer which colorizes code blocks using ColorCode.
/// </summary>
public class ColorCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderer _underlyingCodeBlockRenderer;
    private readonly StyleDictionary _styleDictionary;
    private readonly bool _useCssFormatter;

    /// <summary>
    ///     Create a new <see cref="ColorCodeBlockRenderer"/> with the specified <paramref name="underlyingCodeBlockRenderer"/> and <paramref name="styleDictionary"/>.
    /// </summary>
    /// <param name="underlyingCodeBlockRenderer">The underlying CodeBlockRenderer to handle unsupported languages.</param>
    /// <param name="styleDictionary">A StyleDictionary for custom styling.</param>
    /// <param name="useCssFormatter">Indicates whether to use the CSS formatter.</param>
    public ColorCodeBlockRenderer(
        CodeBlockRenderer underlyingCodeBlockRenderer,
        StyleDictionary styleDictionary,
        bool useCssFormatter = false)
    {
        _underlyingCodeBlockRenderer = underlyingCodeBlockRenderer;
        _styleDictionary = styleDictionary;
        _useCssFormatter = useCssFormatter;
    }

    /// <summary>
    ///     Writes the specified <paramref name="codeBlock"/> to the <paramref name="renderer"/>.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="codeBlock">The code block to render.</param>
    protected override void Write(HtmlRenderer renderer, CodeBlock codeBlock)
    {
        if (codeBlock is not FencedCodeBlock fencedCodeBlock ||
            codeBlock.Parser is not FencedCodeBlockParser fencedCodeBlockParser)
        {
            _underlyingCodeBlockRenderer.Write(renderer, codeBlock);

            return;
        }

        var language = ExtractLanguage(fencedCodeBlock, fencedCodeBlockParser);

        if (language is null)
        {
            _underlyingCodeBlockRenderer.Write(renderer, codeBlock);

            return;
        }

        var code = ExtractCode(codeBlock);
        var html = GetHtml(code, language);

        renderer.Write(html);
    }

    private static ILanguage? ExtractLanguage(IFencedBlock fencedCodeBlock, FencedCodeBlockParser parser)
    {
        var languageId = fencedCodeBlock.Info!.Replace(parser.InfoPrefix!, string.Empty);

        return string.IsNullOrWhiteSpace(languageId) ? null : Languages.FindById(languageId);
    }

    private static string ExtractCode(LeafBlock leafBlock)
    {
        var code = new StringBuilder();
        var lines = leafBlock.Lines.Lines ?? Array.Empty<StringLine>();
        var totalLines = lines.Length;

        for (var index = 0; index < totalLines; index++)
        {
            var line = lines[index];
            var slice = line.Slice;

            if (slice.Text == null)
            {
                continue;
            }

            var lineText = slice.Text.Substring(slice.Start, slice.Length);

            if (index > 0)
            {
                code.AppendLine();
            }

            code.Append(lineText);
        }

        return code.ToString();
    }

    private string? GetHtml(string sourceCode, ILanguage language) => _useCssFormatter
        ? new HtmlClassFormatter(_styleDictionary).GetHtmlString(sourceCode, language)
        : new HtmlFormatter(_styleDictionary).GetHtmlString(sourceCode, language);
}
