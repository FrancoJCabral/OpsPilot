using OpsPilot.Application;
using OpsPilot.Contracts;
using OpsPilot.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ITroubleshootingAgent, RuleBasedTroubleshootingAgent>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));
app.MapPost("/api/troubleshooting/analyze",
    (TroubleshootingRequest request, ITroubleshootingAgent agent) =>
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.ServiceName))
            errors["serviceName"] = ["Service name is required."];
        if (string.IsNullOrWhiteSpace(request.Issue))
            errors["issue"] = ["Issue is required."];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var incident = new TechnicalIncident(
            "Troubleshooting request", request.Issue, request.ServiceName, Severity.Medium);
        var result = agent.Analyze(incident);
        return Results.Ok(new TroubleshootingResponse(
            result.Summary, result.ProbableCause, result.RecommendedAction, result.Confidence));
    })
    .Produces<TroubleshootingResponse>()
    .ProducesValidationProblem();
app.Run();

public partial class Program { }
