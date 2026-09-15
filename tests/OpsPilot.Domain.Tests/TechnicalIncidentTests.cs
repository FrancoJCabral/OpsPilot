using OpsPilot.Domain;
using Xunit;
namespace OpsPilot.Domain.Tests;

public sealed class TechnicalIncidentTests
{
    [Fact]
    public void CreatesValidIncident()
    {
        var before = DateTime.UtcNow;
        var incident = new TechnicalIncident(" Failure ", " Description ", " payments ", Severity.High);
        Assert.NotEqual(Guid.Empty, incident.Id);
        Assert.Equal("Failure", incident.Title);
        Assert.Equal("Description", incident.Description);
        Assert.Equal("payments", incident.ServiceName);
        Assert.Equal(Severity.High, incident.Severity);
        Assert.Equal(DateTimeKind.Utc, incident.CreatedAtUtc.Kind);
        Assert.InRange(incident.CreatedAtUtc, before, DateTime.UtcNow);
    }

    [Theory]
    [InlineData("", "description", "payments")]
    [InlineData("title", " ", "payments")]
    [InlineData("title", "description", null)]
    public void RejectsMissingFields(string title, string description, string? serviceName)
    {
        Assert.Throws<ArgumentException>(() =>
            new TechnicalIncident(title, description, serviceName!, Severity.Low));
    }

    [Fact]
    public void RejectsUndefinedSeverity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TechnicalIncident("title", "description", "payments", (Severity)99));
    }
}
