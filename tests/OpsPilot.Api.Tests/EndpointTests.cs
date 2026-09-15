using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OpsPilot.Contracts;
using Xunit;
namespace OpsPilot.Api.Tests;

public sealed class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    public EndpointTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task HealthReturnsHealthy()
    {
        var response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
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
        Assert.False(string.IsNullOrWhiteSpace(result.Summary));
        Assert.False(string.IsNullOrWhiteSpace(result.RecommendedAction));
        Assert.InRange(result.Confidence, 0, 1);
        Assert.Equal(result, await second.Content.ReadFromJsonAsync<TroubleshootingResponse>());
    }

    [Fact]
    public async Task UnknownServiceReturnsControlledResponse()
    {
        var response = await client.PostAsJsonAsync("/api/troubleshooting/analyze",
            new TroubleshootingRequest("unknown", "failure"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TroubleshootingResponse>();
        Assert.NotNull(result);
        Assert.Equal("Unknown; available information is insufficient.", result.ProbableCause);
        Assert.Equal(0.20, result.Confidence);
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
        Assert.Contains("errors", await response.Content.ReadAsStringAsync());
    }
}
