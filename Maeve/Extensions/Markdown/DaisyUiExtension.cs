using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Maeve.Extensions.Markdown;

/// <summary>
/// Extension for tagging some HTML elements with DaisyUI classes.
/// </summary>
public class DaisyUiExtension: IMarkdownExtension {
    
    // - Functions
    
    // IMarkdownExtension Functions
    
    public void Setup(MarkdownPipelineBuilder pipeline) {
        pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
        pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
        
    }
    
    
    // - Private Functions

    private void PipelineOnDocumentProcessed(MarkdownDocument document) {
        foreach (var table in document.Descendants<Table>()) {
            table.GetAttributes().AddClass("table table-sm table-zebra");
        }

        foreach (var paragraph in document.Descendants<ParagraphBlock>()) {
            paragraph.GetAttributes().AddClass("my-2");
        }

        foreach (var heading in document.Descendants<HeadingBlock>()) {
            switch (heading.Level) {
                case 1:
                    heading.GetAttributes().AddClass("text-xxl my-4");
                    break;
                case 2: 
                    heading.GetAttributes().AddClass("text-xl my-3");
                    break;
                case 3:
                    heading.GetAttributes().AddClass("text-lg my-2");
                    break;
            }
        }

        foreach (var listItem in document.Descendants<ListItemBlock>()) {
            listItem.GetAttributes().AddClass("my-2");
        }
    }
}