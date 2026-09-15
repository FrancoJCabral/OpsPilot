using OpsPilot.Application;
using Xunit;

namespace OpsPilot.Api.Tests;

public sealed class RetrievalRelevanceTests
{
    [Fact]
    public async Task Payments503RanksPaymentsThenDeploymentAndRejectsAuthentication()
    {
        var provider = new DeterministicEmbeddingProvider();
        var query = await provider.EmbedAsync("payments 503 errors after deployment");
        var payments = await provider.EmbedAsync(
            "payments errors payments 503 payment gateway deployment HTTP 503 after deployment");
        var deployment = await provider.EmbedAsync(
            "deployment rollback failures immediately after deployment error rate prior release");
        var authentication = await provider.EmbedAsync(
            "authentication errors 401 token issuer signing keys credentials identity provider");

        var paymentsScore = Cosine(query, payments);
        var deploymentScore = Cosine(query, deployment);
        var authenticationScore = Cosine(query, authentication);

        Assert.True(paymentsScore > deploymentScore);
        Assert.True(deploymentScore > authenticationScore);
        Assert.True(authenticationScore < 0.20);
    }

    private static double Cosine(float[] left, float[] right) =>
        left.Zip(right, (x, y) => x * y).Sum();
}
