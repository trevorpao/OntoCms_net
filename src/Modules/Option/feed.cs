using Dapper;
using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Option;

[MTB("option")]
[MULTILANG(false)]
public sealed class OptionFeed : BaseFeedRepository<OptionFeed.WriteModel>
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

        const string sql = """
SELECT TOP (1)
    [id] AS [Id],
    [group] AS [Group],
    [loader] AS [Loader],
    [status] AS [Status],
    [name] AS [Name],
    [content] AS [Content]
FROM [dbo].[tbl_option]
WHERE [id] = @Id
ORDER BY [id];
""";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<OptionRecord>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<string> GetSiteTitleAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "OntoCMS";
        }

        const string sql = """
SELECT TOP (1) [content]
FROM [dbo].[tbl_option]
WHERE [group] = @OptionGroup
  AND [name] = @OptionName
  AND [status] = N'Enabled'
ORDER BY [id];
""";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var siteTitle = await connection.QueryFirstOrDefaultAsync<string>(new CommandDefinition(
            sql,
            new
            {
                OptionGroup = "page",
                OptionName = "title",
            },
            cancellationToken: cancellationToken));

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

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return await SaveMainRowAsync(connection, columns, payload.Id, cancellationToken);
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
}