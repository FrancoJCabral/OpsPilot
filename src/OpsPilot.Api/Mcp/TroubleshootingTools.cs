using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;
using OpsPilot.Application.Tools;

namespace OpsPilot.Api.Mcp;

[McpServerToolType]
public sealed class TroubleshootingTools(ITechnicalTools tools)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [McpServerTool(Name = ToolNames.Health), Description("Get deterministic simulated service health; unknown services have no status.")]
    public string GetServiceHealth(
        [Description("Service name: payments, authentication or database.")] string serviceName) =>
        JsonSerializer.Serialize(tools.GetServiceHealth(serviceName), JsonOptions);

    [McpServerTool(Name = ToolNames.Logs), Description("Search deterministic local technical logs for matching issue terms.")]
    public string SearchLogs(
        [Description("Service name.")] string serviceName,
        [Description("Troubleshooting query to match in simulated logs.")] string query) =>
        JsonSerializer.Serialize(tools.SearchLogs(serviceName, query), JsonOptions);

    [McpServerTool(Name = ToolNames.Deployments), Description("Get recent simulated deployments with fixed UTC timestamps.")]
    public string GetRecentDeployments(
        [Description("Service name.")] string serviceName) =>
        JsonSerializer.Serialize(tools.GetRecentDeployments(serviceName), JsonOptions);
}
