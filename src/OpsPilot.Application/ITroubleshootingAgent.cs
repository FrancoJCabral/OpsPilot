using OpsPilot.Domain;
namespace OpsPilot.Application;

public interface ITroubleshootingAgent
{
    TroubleshootingAnalysis Analyze(
        TechnicalIncident incident,
        IReadOnlyList<RunbookSearchResult> context);
}

public sealed record TroubleshootingAnalysis(
    string Summary, string ProbableCause, string RecommendedAction, double Confidence);
