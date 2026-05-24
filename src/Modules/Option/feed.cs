using Dapper;
using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;
using SqlKata;

namespace OntoCms.Modules.Option;

[MTB("option")]
[MULTILANG(false)]
public sealed class OptionFeed : BaseFeedRepository<OptionFeed.WriteModel>
    , IReactionGetFeed<OptionFeed.OptionRecord>
    , IReactionListFeed<OptionFeed.OptionRecord>
    , IReactionOptionsFeed<OptionFeed.OptionOption>
{
    private readonly IConfiguration configuration;

    public OptionFeed(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<OptionRecord?> GetAsync(int id, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var query = NewQuery("[dbo].[tbl_option]")
            .Select(
                "id as Id",
                "group as [Group]",
                "loader as Loader",
                "status as Status",
                "name as Name",
                "content as Content")
            .Where("id", id)
            .OrderBy("id");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await OneAsync<OptionRecord>(connection, query, cancellationToken);
    }

    public async Task<FeedPageResult<OptionRecord>> LimitRowsAsync(
        string query,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new FeedPageResult<OptionRecord>(Array.Empty<OptionRecord>(), 0, limit, 0, 0);
        }

        var normalizedQuery = query.Trim();
        var rowsQuery = NewQuery("[dbo].[tbl_option]")
            .Select(
                "id as Id",
                "group as [Group]",
                "loader as Loader",
                "status as Status",
                "name as Name",
                "content as Content")
            .OrderBy("group")
            .OrderBy("id");

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            rowsQuery.Where(q => q
                .WhereLike("group", $"%{normalizedQuery}%")
                .OrWhereLike("name", $"%{normalizedQuery}%")
                .OrWhereLike("content", $"%{normalizedQuery}%"));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await LimitRowsAsync<OptionRecord>(connection, rowsQuery, page, limit, cancellationToken);
    }

    public async Task<IReadOnlyList<OptionOption>> GetOptionsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Array.Empty<OptionOption>();
        }

        var normalizedQuery = query.Trim();
        var optionsQuery = NewQuery("[dbo].[tbl_option]")
            .Select(
                "id as Id",
                "name as Title")
            .Where("status", "Enabled")
            .OrderBy("group")
            .OrderBy("id");

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            optionsQuery.Where(q => q
                .WhereLike("group", $"%{normalizedQuery}%")
                .OrWhereLike("name", $"%{normalizedQuery}%")
                .OrWhereLike("content", $"%{normalizedQuery}%"));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await LotsAsync<OptionOption>(connection, optionsQuery, cancellationToken);
    }

    public async Task<string> GetSiteTitleAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "OntoCMS";
        }

        var query = NewQuery("[dbo].[tbl_option]")
            .Select("content")
            .Where("group", "page")
            .Where("name", "title")
            .Where("status", "Enabled")
            .OrderBy("id");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var siteTitle = (await LotsAsync<string>(
            connection,
            query,
            cancellationToken,
            limit: 1))
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(siteTitle) ? "OntoCMS" : siteTitle;
    }

    public override async Task<int> SaveAsync(WriteModel payload, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var columns = ApplyAuditFields(
            HandleColumns(payload),
            actorUserId: 0,
            isInsert: payload.Id <= 0);

        return await WithTransactionAsync(connectionString, (connection, transaction, token) =>
            SaveMainRowAsync(connection, columns, payload.Id, transaction, token), cancellationToken);
    }

    public sealed record WriteModel(
        int Id,
        string Group,
        string Loader,
        string Status,
        string Name,
        string Content,
        IReadOnlyDictionary<string, object?>? Meta = null,
        IReadOnlyDictionary<string, object?>? Lang = null,
        IReadOnlyList<string>? Tags = null);

    public sealed record OptionRecord(
        int Id,
        string Group,
        string Loader,
        string Status,
        string Name,
        string Content);

    public sealed record OptionOption(
        int Id,
        string Title);
}