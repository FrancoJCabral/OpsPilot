namespace OpsPilot.Contracts;
public sealed record TroubleshootingResponse(
    string Summary,
    string ProbableCause,
    string RecommendedAction,
    double Confidence,
    string[] Sources,
    string[] Evidence,
    string[] ToolsUsed);
