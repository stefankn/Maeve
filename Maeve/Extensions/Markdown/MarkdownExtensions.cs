using Markdig;

namespace Maeve.Extensions.Markdown;

public static class MarkdownExtensions {
    
    // - Functions
    
    public static MarkdownPipelineBuilder UseDaisyUi(this MarkdownPipelineBuilder pipeline) {
        pipeline.Extensions.AddIfNotAlready<DaisyUiExtension>();
        return pipeline;
    }
}