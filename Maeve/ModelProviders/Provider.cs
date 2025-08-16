using System.ComponentModel;

namespace Maeve.ModelProviders;

public enum Provider {
    [Description("Ollama")]
    Ollama,
    
    [Description("Antrophic")]
    Antrophic
}