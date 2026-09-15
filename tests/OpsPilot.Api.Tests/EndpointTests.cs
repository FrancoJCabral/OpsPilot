using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsPilot.Application;
using OpsPilot.Contracts;
using Xunit;
namespace OpsPilot.Api.Tests;

public sealed class EndpointTests : IClassFixture<OpsPilotFactory>
{
    private readonly HttpClient client;
    public EndpointTests(OpsPilotFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task HealthReturnsHealthy()
    {
        var response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task IndexReturnsIndexedCount()
    {
        var response = await client.PostAsync("/api/runbooks/index", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("indexed", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("payments", "payment")]
    [InlineData("authentication", "Token")]
    [InlineData("database", "Connection")]
    [InlineData(" PAYMENTS ", "payment")]
    public async Task AnalyzeReturnsDeterministicResponse(string service, string causeFragment)
    {
        var request = new TroubleshootingRequest(service, "503 errors after deployment");
        var first = await client.PostAsJsonAsync("/api/troubleshooting/analyze", request);
        var second = await client.PostAsJsonAsync("/api/troubleshooting/analyze", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var result = await first.Content.ReadFromJsonAsync<TroubleshootingResponse>();
        Assert.NotNull(result);
        Assert.Contains(causeFragment, result.ProbableCause);
        Assert.NotEmpty(result.Sources);
        var repeated = await second.Content.ReadFromJsonAsync<TroubleshootingResponse>();
        Assert.NotNull(repeated);
        Assert.Equal(result.Summary, repeated.Summary);
        Assert.Equal(result.ProbableCause, repeated.ProbableCause);
        Assert.Equal(result.RecommendedAction, repeated.RecommendedAction);
        Assert.Equal(result.Confidence, repeated.Confidence);
        Assert.Equal(result.Sources, repeated.Sources);
    }

    [Fact]
    public async Task UnknownServiceWithoutMatchesReturnsControlledResponse()
    {
        var response = await client.PostAsJsonAsync("/api/troubleshooting/analyze",
            new TroubleshootingRequest("unknown", "unclassified symptom"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TroubleshootingResponse>();
        Assert.NotNull(result);
        Assert.Equal("Unknown; available information is insufficient.", result.ProbableCause);
        Assert.Equal(0.20, result.Confidence);
        Assert.Empty(result.Sources);
    }

    [Theory]
    [InlineData("", "failure")]
    [InlineData("payments", " ")]
    public async Task MissingFieldsReturnProblemDetails(string service, string issue)
    {
        var response = await client.PostAsJsonAsync("/api/troubleshooting/analyze",
            new TroubleshootingRequest(service, issue));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}

public sealed class OpsPilotFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRunbookIndexer>();
            services.RemoveAll<IRunbookSearchService>();
            services.AddSingleton<TestRunbookService>();
            services.AddSingleton<IRunbookIndexer>(provider =>
                provider.GetRequiredService<TestRunbookService>());
            services.AddSingleton<IRunbookSearchService>(provider =>
                provider.GetRequiredService<TestRunbookService>());
        });
    }
}

internal sealed class TestRunbookService : IRunbookIndexer, IRunbookSearchService
{
    public Task<int> IndexAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(4);

    public Task<IReadOnlyList<RunbookSearchResult>> SearchAsync(
        string query, int limit = 3, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RunbookSearchResult> result =
            query.Contains("unknown", StringComparison.OrdinalIgnoreCase)
                ? []
                : [new("deployment-rollback.md", "Rollback guidance", 0.75)];
        return Task.FromResult(result);
    }
}
