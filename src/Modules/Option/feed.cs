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
}