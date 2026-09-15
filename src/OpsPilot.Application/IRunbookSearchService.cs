namespace OpsPilot.Application;

public interface IRunbookSearchService
{
    Task<IReadOnlyList<RunbookSearchResult>> SearchAsync(
        string query, int limit = 3, CancellationToken cancellationToken = default);
}
