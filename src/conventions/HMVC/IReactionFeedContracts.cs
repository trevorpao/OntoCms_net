namespace OntoCms.Conventions.HMVC;

public interface IReactionGetFeed<TRow>
{
    Task<TRow?> GetAsync(int id, CancellationToken cancellationToken = default);
}

public interface IReactionListFeed<TRow>
{
    Task<FeedPageResult<TRow>> LimitRowsAsync(
        string query,
        int page,
        int limit,
        CancellationToken cancellationToken = default);
}

public interface IReactionOptionsFeed<TOption>
{
    Task<IReadOnlyList<TOption>> GetOptionsAsync(
        string query,
        CancellationToken cancellationToken = default);
}