using OpsPilot.Application.Tools;
using OpsPilot.Domain;

namespace OpsPilot.Application;

public sealed class TroubleshootingWorkflow(
    IRunbookSearchService search,
    ITechnicalTools tools,
    ITroubleshootingAgent agent)
{
    public async Task<WorkflowResult> AnalyzeAsync(
        TechnicalIncident incident, CancellationToken cancellationToken = default)
    {
        var context = await search.SearchAsync(
            $"{incident.ServiceName} {incident.Description}", cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<string>();
        var used = new List<string> { ToolNames.Health };
        var health = tools.GetServiceHealth(incident.ServiceName);

        if (!health.KnownService)
        {
            evidence.Add($"Simulated telemetry: service '{health.ServiceName}' is unknown; no health data available.");
        }
        else
        {
            evidence.Add($"Simulated health: {health.ServiceName} is {health.Status}.");
            used.Add(ToolNames.Logs);
            var logs = tools.SearchLogs(incident.ServiceName, incident.Description);
            evidence.AddRange(logs.Select(log => $"Simulated log: {log}"));
            if (logs.Length == 0)
                evidence.Add("Simulated logs: no entries match the issue.");

            used.Add(ToolNames.Deployments);
            var deployments = tools.GetRecentDeployments(incident.ServiceName);
            evidence.AddRange(deployments.Select(deployment =>
                $"Simulated deployment: {deployment.Id}, version {deployment.Version}, " +
                $"{deployment.DeployedAtUtc:O}, {deployment.Status}."));
        }

        var analysis = agent.Analyze(incident, context);
        if (health.KnownService)
        {
            analysis = analysis with
            {
                ProbableCause = $"{analysis.ProbableCause} Simulated health is {health.Status}; " +
                    "correlate the retrieved logs with recent deployments before confirming the cause.",
                RecommendedAction = $"{analysis.RecommendedAction} Verify the simulated evidence against real telemetry."
            };
        }

        if (context.Count > 0)
        {
            evidence.AddRange(context.Select(item => $"Runbook: {item.Source} (score {item.Score:F3})."));
            analysis = analysis with
            {
                RecommendedAction = $"{analysis.RecommendedAction} Runbook guidance ({context[0].Source}): " +
                    context[0].Content.Trim()
            };
        }

        return new(
            analysis,
            context.Select(item => item.Source).Distinct().ToArray(),
            evidence.ToArray(),
            used.ToArray());
    }
}

public sealed record WorkflowResult(
    TroubleshootingAnalysis Analysis, string[] Sources, string[] Evidence, string[] ToolsUsed);
