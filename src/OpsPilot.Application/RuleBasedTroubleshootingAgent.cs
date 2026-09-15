using OpsPilot.Domain;
namespace OpsPilot.Application;

public sealed class RuleBasedTroubleshootingAgent : ITroubleshootingAgent
{
    public TroubleshootingAnalysis Analyze(TechnicalIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);
        return incident.ServiceName.ToLowerInvariant() switch
        {
            "payments" => new(
                "Payment service incident requires investigation.",
                "Deployment configuration or an unavailable payment dependency.",
                "Check deployment changes, service logs and payment provider health; consider rollback.", 0.65),
            "authentication" => new(
                "Authentication service incident requires investigation.",
                "Token configuration, expired credentials or an unavailable identity provider.",
                "Check token issuer, audience, credential expiry and identity provider health.", 0.60),
            "database" => new(
                "Database service incident requires investigation.",
                "Connection exhaustion, connectivity failure or slow queries.",
                "Check database connectivity, connection pool usage and slow query logs.", 0.60),
            _ => new(
                "No troubleshooting rule is available for this service.",
                "Unknown; available information is insufficient.",
                "Collect service logs, recent changes and dependency health for manual investigation.", 0.20)
        };
    }
}
