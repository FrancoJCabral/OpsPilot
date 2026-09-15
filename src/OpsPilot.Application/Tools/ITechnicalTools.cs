namespace OpsPilot.Application.Tools;

public interface ITechnicalTools
{
    ServiceHealth GetServiceHealth(string serviceName);
    string[] SearchLogs(string serviceName, string query);
    Deployment[] GetRecentDeployments(string serviceName);
}

public enum HealthStatus { Healthy, Degraded, Down }

public sealed record ServiceHealth(
    string ServiceName, HealthStatus? Status, bool KnownService);

public sealed record Deployment(string Id, string Version, DateTime DeployedAtUtc, string Status);

public static class ToolNames
{
    public const string Health = "get_service_health";
    public const string Logs = "search_logs";
    public const string Deployments = "get_recent_deployments";
}
