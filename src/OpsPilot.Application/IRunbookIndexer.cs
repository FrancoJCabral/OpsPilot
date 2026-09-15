namespace OpsPilot.Application;

public interface IRunbookIndexer
{
    Task<int> IndexAsync(CancellationToken cancellationToken = default);
}
