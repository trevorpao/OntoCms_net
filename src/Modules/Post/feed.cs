using Dapper;
using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Post;

[MTB("post")]
[MULTILANG(true)]
public sealed class PostFeed : BaseFeedRepository<PostFeed.WriteModel>
{
    private readonly IConfiguration configuration;

    public PostFeed(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<PublishedPostRecord?> GetPublishedBySlugAsync(
        string slug,
        string? lang,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        const string sql = """
SELECT TOP (1)
    post.[id] AS [Id],
    post.[slug] AS [Slug],
    post_lang.[lang] AS [Lang],
    post_lang.[title] AS [Title],
    post_lang.[content] AS [Content]
FROM [dbo].[tbl_post] AS post
INNER JOIN [dbo].[tbl_post_lang] AS post_lang
    ON post_lang.[parent_id] = post.[id]
WHERE post.[status] = N'Enabled'
  AND post.[slug] = @Slug
  AND post_lang.[lang] IN (@RequestedLang, N'tw')
ORDER BY CASE
    WHEN post_lang.[lang] = @RequestedLang THEN 0
    WHEN post_lang.[lang] = N'tw' THEN 1
    ELSE 2
END,
post_lang.[id];
""";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<PublishedPostRecord>(new CommandDefinition(
            sql,
            new
            {
                Slug = slug,
                RequestedLang = NormalizeFrontendLang(lang),
            },
            cancellationToken: cancellationToken));
    }

    public override Task<int> SaveAsync(WriteModel payload, CancellationToken cancellationToken = default)
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

        return WithTransactionAsync(connectionString, async (connection, transaction, token) =>
        {
            var rowId = await SaveMainRowAsync(connection, columns, payload.Id, transaction, token);
            await SaveMetaAsync(connection, transaction, rowId, columns.Meta, token);
            await SaveLangAsync(connection, transaction, rowId, columns.Lang, actorUserId: 0, token);
            return rowId;
        }, cancellationToken);
    }

    public sealed record WriteModel(
        int Id,
        string Status,
        string Slug,
        string Cover,
        string Layout,
        IReadOnlyDictionary<string, object?>? Meta = null,
        IReadOnlyDictionary<string, LangWriteModel>? Lang = null);

    public sealed record LangWriteModel(
        string Title,
        string Content,
        string FromAi = "No");

    public sealed record PublishedPostRecord(
        int Id,
        string Slug,
        string Lang,
        string Title,
        string Content);

    private static string NormalizeFrontendLang(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return "tw";
        }

        var normalized = lang.Trim().ToLowerInvariant();
        return normalized switch
        {
            "en" or "en-us" or "en-gb" => "en",
            "tw" or "zh" or "zh-tw" or "zh-hant" => "tw",
            _ => "tw",
        };
    }
}