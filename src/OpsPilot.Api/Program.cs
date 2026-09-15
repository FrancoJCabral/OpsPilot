using Microsoft.Extensions.Options;
using OpsPilot.Application;
using OpsPilot.Contracts;
using OpsPilot.Domain;
using OpsPilot.Application.Tools;
using OpsPilot.Api.Mcp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection(QdrantOptions.SectionName));
builder.Services.Configure<EmbeddingOptions>(builder.Configuration.GetSection(EmbeddingOptions.SectionName));
builder.Services.AddSingleton<IEmbeddingProvider, DeterministicEmbeddingProvider>();
builder.Services.AddHttpClient<QdrantRunbookService>((services, client) =>
{
    var settings = services.GetRequiredService<IOptions<QdrantOptions>>().Value;
    client.BaseAddress = new Uri(settings.Url);
});
builder.Services.AddScoped<IRunbookIndexer>(
    services => services.GetRequiredService<QdrantRunbookService>());
builder.Services.AddScoped<IRunbookSearchService>(
    services => services.GetRequiredService<QdrantRunbookService>());
builder.Services.AddSingleton<ITroubleshootingAgent, RuleBasedTroubleshootingAgent>();

builder.Services.AddSingleton<ITechnicalTools, LocalTechnicalTools>();
builder.Services.AddScoped<TroubleshootingWorkflow>();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<TroubleshootingTools>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapMcp("/mcp");

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/api/runbooks/index", async (
    IRunbookIndexer indexer, CancellationToken cancellationToken) =>
{
    var indexed = await indexer.IndexAsync(cancellationToken);
    return Results.Ok(new { indexed });
});

app.MapPost("/api/troubleshooting/analyze", async (
    TroubleshootingRequest request,
    TroubleshootingWorkflow workflow,
    CancellationToken cancellationToken) =>
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
    var result = await workflow.AnalyzeAsync(incident, cancellationToken);
    return Results.Ok(new TroubleshootingResponse(
        result.Analysis.Summary,
        result.Analysis.ProbableCause,
        result.Analysis.RecommendedAction,
        result.Analysis.Confidence,
        result.Sources,
        result.Evidence,
        result.ToolsUsed));
})
.Produces<TroubleshootingResponse>()
.ProducesValidationProblem();

app.Run();
public partial class Program { }
