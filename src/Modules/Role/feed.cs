using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Role;

[MTB("role")]
[MULTILANG(false)]
public sealed class RoleFeed : BaseFeedRepository<RoleFeed.WriteModel>
    , IReactionGetFeed<RoleFeed.RoleRecord>
    , IReactionListFeed<RoleFeed.RoleRecord>
    , IReactionOptionsFeed<RoleFeed.RoleOption>
{
    private static readonly IReadOnlyDictionary<string, AuthorityDefinition> authorityDefinitions =
        new Dictionary<string, AuthorityDefinition>(StringComparer.Ordinal)
        {
            ["base.cms"] = new AuthorityDefinition(1, 1, "base.cms", "基本管理"),
            ["mgr.cms"] = new AuthorityDefinition(2, 2, "mgr.cms", "進階內容管理"),
            ["base.member"] = new AuthorityDefinition(3, 4, "base.member", "基本客戶管理"),
            ["mgr.member"] = new AuthorityDefinition(4, 8, "mgr.member", "進階客戶管理"),
            ["mgr.site"] = new AuthorityDefinition(5, 16, "mgr.site", "完整網站管理"),
        };

    private readonly IConfiguration configuration;

    public RoleFeed(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public static IReadOnlyList<AuthorityOption> GetAuthorityOptions()
    {
        return authorityDefinitions.Values
            .OrderBy(static authority => authority.Idx)
            .Select(static authority => new AuthorityOption(authority.Val, authority.Name, authority.Title))
            .ToArray();
    }

    public static IReadOnlyList<int> ExpandAuthorityValues(int priv)
    {
        return authorityDefinitions.Values
            .OrderBy(static authority => authority.Idx)
            .Where(authority => (priv & authority.Val) == authority.Val)
            .Select(static authority => authority.Val)
            .ToArray();
    }

    public static IReadOnlyList<string> ExpandAuthorityNames(int priv)
    {
        return authorityDefinitions.Values
            .OrderBy(static authority => authority.Idx)
            .Where(authority => (priv & authority.Val) == authority.Val)
            .Select(static authority => authority.Name)
            .ToArray();
    }

    public static IReadOnlyList<string> ExpandAuthorityTitles(int priv)
    {
        return authorityDefinitions.Values
            .OrderBy(static authority => authority.Idx)
            .Where(authority => (priv & authority.Val) == authority.Val)
            .Select(static authority => authority.Title)
            .ToArray();
    }

    public static bool HasAuthority(int priv, string authority)
    {
        return authorityDefinitions.TryGetValue(authority, out var definition)
            && (priv & definition.Val) == definition.Val;
    }

    public static int ParseAuthorityValues(IEnumerable<int>? values)
    {
        if (values is null)
        {
            return 0;
        }

        var allowedValues = authorityDefinitions.Values
            .Select(static authority => authority.Val)
            .ToHashSet();

        return values
            .Where(allowedValues.Contains)
            .Distinct()
            .Sum();
    }

    public async Task<RoleRecord?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var query = NewQuery("dbo.tbl_role")
            .Select(
                "id as Id",
                "status as Status",
                "menu_id as MenuId",
                "title as Title",
                "priv as PrivValue",
                "info as Info")
            .Where("id", id)
            .OrderBy("id");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await OneAsync<RoleRecord>(connection, query, cancellationToken);
    }

    public async Task<FeedPageResult<RoleRecord>> LimitRowsAsync(
        string query,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new FeedPageResult<RoleRecord>(Array.Empty<RoleRecord>(), 0, limit, 0, 0);
        }

        var normalizedQuery = query.Trim();
        var rowsQuery = NewQuery("dbo.tbl_role")
            .Select(
                "id as Id",
                "status as Status",
                "menu_id as MenuId",
                "title as Title",
                "priv as PrivValue",
                "info as Info")
            .OrderBy("title")
            .OrderBy("id");

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            rowsQuery.Where(q => q
                .WhereLike("title", $"%{normalizedQuery}%")
                .OrWhereLike("info", $"%{normalizedQuery}%"));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await LimitRowsAsync<RoleRecord>(connection, rowsQuery, page, limit, cancellationToken);
    }

    public async Task<IReadOnlyList<RoleOption>> GetOptionsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Array.Empty<RoleOption>();
        }

        var normalizedQuery = query.Trim();
        var optionsQuery = NewQuery("dbo.tbl_role")
            .Select(
                "id as Id",
                "title as Title")
            .Where("status", "Enabled")
            .OrderBy("title")
            .OrderBy("id");

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            optionsQuery.Where(q => q
                .WhereLike("title", $"%{normalizedQuery}%")
                .OrWhereLike("info", $"%{normalizedQuery}%"));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await LotsAsync<RoleOption>(connection, optionsQuery, cancellationToken);
    }

    public override async Task<int> SaveAsync(WriteModel payload, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var normalizedPriv = payload.Auth is { Count: > 0 }
            ? ParseAuthorityValues(payload.Auth)
            : payload.Priv ?? 0;

        var normalizedPayload = payload with { Priv = normalizedPriv, Auth = null };
        var columns = ApplyAuditFields(
            HandleColumns(normalizedPayload),
            actorUserId: 0,
            isInsert: payload.Id <= 0);

        return await WithTransactionAsync(
            connectionString,
            (connection, transaction, token) => SaveMainRowAsync(connection, columns, payload.Id, transaction, token),
            cancellationToken);
    }

    protected override bool FilterColumn(string columnName)
    {
        return !string.Equals(columnName, "auth", StringComparison.OrdinalIgnoreCase)
            && base.FilterColumn(columnName);
    }

    public sealed record WriteModel(
        int Id,
        string? Status,
        int MenuId,
        string? Title,
        int? Priv,
        string? Info,
        IReadOnlyList<int>? Auth = null,
        IReadOnlyDictionary<string, object?>? Meta = null,
        IReadOnlyDictionary<string, object?>? Lang = null,
        IReadOnlyList<string>? Tags = null);

    public sealed record RoleRecord
    {
        public int Id { get; init; }

        public string Status { get; init; } = string.Empty;

        public int MenuId { get; init; }

        public string Title { get; init; } = string.Empty;

        public int PrivValue { get; init; }

        public string? Info { get; init; }

        public string Priv { get; init; } = string.Empty;

        public IReadOnlyList<int>? Auth { get; init; }

        public IReadOnlyList<string>? Authorities { get; init; }
    }

    public sealed record RoleOption
    {
        public int Id { get; init; }

        public string Title { get; init; } = string.Empty;
    }

    public sealed record AuthorityOption(int Id, string Name, string Title);

    private sealed record AuthorityDefinition(int Idx, int Val, string Name, string Title);
}