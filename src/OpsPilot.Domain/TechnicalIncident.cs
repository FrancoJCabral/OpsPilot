namespace OpsPilot.Domain;

public sealed class TechnicalIncident
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; }
    public string Description { get; }
    public string ServiceName { get; }
    public Severity Severity { get; }
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

    public TechnicalIncident(string title, string description, string serviceName, Severity severity)
    {
        Title = Required(title, nameof(title));
        Description = Required(description, nameof(description));
        ServiceName = Required(serviceName, nameof(serviceName));
        if (!Enum.IsDefined(severity))
            throw new ArgumentOutOfRangeException(nameof(severity));
        Severity = severity;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", name)
            : value.Trim();
}
