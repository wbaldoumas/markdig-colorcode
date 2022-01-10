﻿using ColorCode.Styling;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.ColorCode;

/// <summary>
///     An extension which colorizes code.
/// </summary>
public class ColorCodeExtension : IMarkdownExtension
{
    private readonly StyleDictionary _styleDictionary;

    /// <summary>
    ///     Creates a new <see cref="ColorCodeExtension"/> with the specified <paramref name="styleDictionary"/>.
    /// </summary>
    /// <param name="styleDictionary">A dictionary indicating how to style the code.</param>
    public ColorCodeExtension(StyleDictionary styleDictionary) => _styleDictionary = styleDictionary;

    /// <summary>
    /// Setups this extension for the specified pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    /// <summary>
    /// Setups this extension for the specified renderer.
    /// </summary>
    /// <param name="pipeline">The pipeline used to parse the document.</param>
    /// <param name="renderer">The renderer.</param>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer)
        {
            return;
        }

        var codeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();

        if (codeBlockRenderer != null)
        {
            htmlRenderer.ObjectRenderers.Remove(codeBlockRenderer);
        }
        else
        {
            codeBlockRenderer = new CodeBlockRenderer();
        }

        htmlRenderer.ObjectRenderers.AddIfNotAlready(
            new ColorCodeBlockRenderer(
                codeBlockRenderer,
                _styleDictionary
            )
        );
    }
}
