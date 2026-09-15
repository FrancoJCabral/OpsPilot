namespace OpsPilot.Application;

public interface IEmbeddingProvider
{
    int Dimensions { get; }
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
