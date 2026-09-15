namespace OpsPilot.Application;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";
    public string Url { get; init; } = "http://localhost:6333";
    public string Collection { get; init; } = "opspilot-runbooks";
    public double MinimumScore { get; init; } = 0.20;
    public string RunbooksPath { get; init; } = "data/runbooks";
}
