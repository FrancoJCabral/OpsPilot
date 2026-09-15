namespace OpsPilot.Application.Tools;

public sealed class LocalTechnicalTools : ITechnicalTools
{
    public ServiceHealth GetServiceHealth(string serviceName)
    {
        var service = Normalize(serviceName);
        HealthStatus? status = service switch
        {
            "payments" => HealthStatus.Degraded,
            "authentication" => HealthStatus.Down,
            "database" => HealthStatus.Healthy,
            _ => null
        };
        return new(service, status, status.HasValue);
    }

    public string[] SearchLogs(string serviceName, string query)
    {
        var service = Normalize(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        string[] logs = service switch
        {
            "payments" =>
            [
                "2026-09-15T10:02:00Z ERROR payments HTTP 503 payment-provider timeout after deployment pay-102",
                "2026-09-15T10:03:00Z WARN payments gateway circuit breaker open; downstream latency 5000ms"
            ],
            "authentication" =>
            [
                "2026-09-15T10:02:00Z ERROR authentication HTTP 401 token validation failed: signing credential expired",
                "2026-09-15T10:03:00Z ERROR authentication identity provider connection refused"
            ],
            "database" =>
            [
                "2026-09-15T10:02:00Z WARN database slow query duration 2400ms; connection pool usage 95%",
                "2026-09-15T10:03:00Z WARN database latency: active lock on orders table"
            ],
            _ => []
        };

        var terms = query.Split([' ', '-', '/', '.', ':'], StringSplitOptions.RemoveEmptyEntries);
        return logs.Where(log => terms.Any(term =>
            log.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray();
    }

    public Deployment[] GetRecentDeployments(string serviceName)
    {
        var service = Normalize(serviceName);
        if (!GetServiceHealth(service).KnownService)
            return [];

        var id = service switch
        {
            "payments" => "pay-102",
            "authentication" => "auth-204",
            _ => "db-301"
        };
        return
        [
            new(id, "1.2.0", new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc), "Completed"),
            new($"{id}-previous", "1.1.0", new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc), "Completed")
        ];
    }

    private static string Normalize(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        return serviceName.Trim().ToLowerInvariant();
    }
}
