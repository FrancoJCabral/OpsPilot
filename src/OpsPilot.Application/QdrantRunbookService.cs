using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OpsPilot.Application;

public sealed class QdrantRunbookService(
    HttpClient client,
    IEmbeddingProvider embeddings,
    IOptions<QdrantOptions> options) : IRunbookIndexer, IRunbookSearchService
{
    private readonly QdrantOptions settings = options.Value;

    public async Task<int> IndexAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.GetFullPath(settings.RunbooksPath);
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Runbook directory not found: {path}");

        using var collectionResponse = await client.PutAsJsonAsync(
            $"/collections/{settings.Collection}",
            new { vectors = new { size = embeddings.Dimensions, distance = "Cosine" } },
            cancellationToken);
        if (collectionResponse.StatusCode != HttpStatusCode.Conflict)
            collectionResponse.EnsureSuccessStatusCode();

        var points = new List<object>();
        foreach (var file in Directory.EnumerateFiles(path, "*.md").Order())
        {
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            points.Add(new
            {
                id = CreateId(Path.GetFileName(file)),
                vector = await embeddings.EmbedAsync(content, cancellationToken),
                payload = new { source = Path.GetFileName(file), content }
            });
        }

        using var upsertResponse = await client.PutAsJsonAsync(
            $"/collections/{settings.Collection}/points?wait=true", new { points }, cancellationToken);
        upsertResponse.EnsureSuccessStatusCode();
        return points.Count;
    }

    public async Task<IReadOnlyList<RunbookSearchResult>> SearchAsync(
        string query, int limit = 3, CancellationToken cancellationToken = default)
    {
        var vector = await embeddings.EmbedAsync(query, cancellationToken);
        using var response = await client.PostAsJsonAsync(
            $"/collections/{settings.Collection}/points/search",
            new { vector, limit = Math.Max(limit * 3, 10), score_threshold = settings.MinimumScore, with_payload = true },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var queryTerms = query.ToLowerInvariant().Split(
            [' ', '-', '_', '/', '.'], StringSplitOptions.RemoveEmptyEntries);
        return json.RootElement.GetProperty("result").EnumerateArray()
            .Select(item => new RunbookSearchResult(
                item.GetProperty("payload").GetProperty("source").GetString() ?? "unknown",
                item.GetProperty("payload").GetProperty("content").GetString() ?? string.Empty,
                item.GetProperty("score").GetDouble()))
            .OrderByDescending(item => item.Score +
                queryTerms.Count(term => item.Source.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Take(limit)
            .ToArray();
    }

    private static string CreateId(string source)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(source));
        return new Guid(hash).ToString();
    }
}
