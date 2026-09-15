namespace OpsPilot.Application;

public sealed class EmbeddingOptions
{
    public const string SectionName = "Embeddings";
    public string Provider { get; init; } = "Deterministic";
    public string? OpenAIApiKey { get; init; }
    public string? OpenAIModel { get; init; }
    public string? AzureOpenAIEndpoint { get; init; }
    public string? AzureOpenAIApiKey { get; init; }
    public string? AzureOpenAIDeployment { get; init; }
}
