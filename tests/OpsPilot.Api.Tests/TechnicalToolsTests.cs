using OpsPilot.Application.Tools;
using Xunit;

namespace OpsPilot.Api.Tests;

public sealed class TechnicalToolsTests
{
    private readonly LocalTechnicalTools tools = new();

    [Theory]
    [InlineData("payments", HealthStatus.Degraded)]
    [InlineData("authentication", HealthStatus.Down)]
    [InlineData("database", HealthStatus.Healthy)]
    public void HealthIsDeterministic(string serviceName, HealthStatus expected)
    {
        var result = tools.GetServiceHealth(serviceName);
        Assert.True(result.KnownService);
        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void LogsAndDeploymentsReturnRelevantEvidence()
    {
        var logs = tools.SearchLogs("payments", "503 after deployment");
        var deployments = tools.GetRecentDeployments("payments");

        Assert.Contains(logs, log => log.Contains("503", StringComparison.Ordinal));
        Assert.Equal("pay-102", deployments[0].Id);
    }

    [Fact]
    public void UnknownServiceReturnsNoInventedTelemetry()
    {
        Assert.False(tools.GetServiceHealth("unknown").KnownService);
        Assert.Empty(tools.SearchLogs("unknown", "failure"));
        Assert.Empty(tools.GetRecentDeployments("unknown"));
    }
}
