using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Maeve.Extensions.Markdown;

public class DaisyUiCodeBlockRenderer : CodeBlockRenderer {
    
    // - Functions
    
    // CodeBlockRenderer Functions
    
    protected override void Write(HtmlRenderer renderer, CodeBlock obj) {
        renderer.EnsureLine();
        renderer.Write("<div class=\"mockup-code w-full\">");
        base.Write(renderer, obj);
        renderer.Write("</div>");
        renderer.EnsureLine();
    }
}