using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OntoCms.Modules.Post;

namespace OntoCms.Cli.Smoke;

internal static class PostSaveSmoke
{
    public static async Task RunAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var slug = $"smoke-post-{Guid.NewGuid():N}";
        var feed = new PostFeed(configuration);
        int rowId = 0;

        try
        {
            rowId = await feed.SaveAsync(new PostFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Slug: slug,
                Cover: string.Empty,
                Layout: "normal",
                Meta: new Dictionary<string, object?>
                {
                    ["seo_desc"] = "cli smoke meta",
                },
                Lang: new Dictionary<string, PostFeed.LangWriteModel>
                {
                    ["tw"] = new("CLI 驗證文章", "內文測試", "No"),
                    ["en"] = new("CLI Smoke Post", "Smoke content", "No"),
                }),
                cancellationToken);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var main = await connection.QuerySingleAsync<(string Slug, string Status, string Layout)>(new CommandDefinition(
                "SELECT [slug] AS [Slug], [status] AS [Status], [layout] AS [Layout] FROM [dbo].[tbl_post] WHERE [id] = @Id;",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            var metaCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_meta] WHERE [parent_id] = @Id AND [k] = N'seo_desc' AND [v] = N'cli smoke meta';",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            var langCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_lang] WHERE [parent_id] = @Id AND [lang] IN (N'tw', N'en');",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            if (!string.Equals(main.Slug, slug, StringComparison.Ordinal)
                || !string.Equals(main.Status, "Enabled", StringComparison.Ordinal)
                || !string.Equals(main.Layout, "normal", StringComparison.Ordinal)
                || metaCount != 1
                || langCount != 2)
            {
                throw new InvalidOperationException("Post save smoke validation failed.");
            }

            logger.LogInformation("Post save smoke passed for row {RowId}.", rowId);
        }
        finally
        {
            await CleanupAsync(connectionString, rowId, slug, cancellationToken);
        }
    }

    public static async Task RunRollbackAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var slug = $"smoke-post-rollback-{Guid.NewGuid():N}";
        var metaValue = $"rollback meta {Guid.NewGuid():N}";
        var langTitle = $"Rollback 驗證文章 {Guid.NewGuid():N}";
        var langContent = $"rollback content {Guid.NewGuid():N}";
        var feed = new PostFeed(configuration);

        try
        {
            await feed.SaveAsync(new PostFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Slug: slug,
                Cover: string.Empty,
                Layout: "normal",
                Meta: new Dictionary<string, object?>
                {
                    ["seo_desc"] = metaValue,
                },
                Lang: new Dictionary<string, PostFeed.LangWriteModel>
                {
                    ["tw"] = new(langTitle, langContent, "Maybe"),
                }),
                cancellationToken);

            throw new InvalidOperationException("Rollback smoke expected PostFeed.SaveAsync to fail, but it succeeded.");
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var postCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post] WHERE [slug] = @Slug;",
                new { Slug = slug },
                cancellationToken: cancellationToken));

            var metaCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_meta] WHERE [k] = N'seo_desc' AND [v] = @MetaValue;",
                new { MetaValue = metaValue },
                cancellationToken: cancellationToken));

            var langCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_lang] WHERE [title] = @LangTitle AND [content] = @LangContent;",
                new { LangTitle = langTitle, LangContent = langContent },
                cancellationToken: cancellationToken));

            if (postCount != 0)
            {
                throw new InvalidOperationException("Rollback smoke failed because tbl_post row still exists after SaveAsync error.", ex);
            }

            if (metaCount != 0)
            {
                throw new InvalidOperationException("Rollback smoke failed because tbl_post_meta row still exists after SaveAsync error.", ex);
            }

            if (langCount != 0)
            {
                throw new InvalidOperationException("Rollback smoke failed because tbl_post_lang row still exists after SaveAsync error.", ex);
            }

            logger.LogInformation("Post save rollback smoke passed. SaveAsync failed and no tbl_post/tbl_post_meta/tbl_post_lang rows remained for slug {Slug}.", slug);
        }
        finally
        {
            await CleanupAsync(connectionString, 0, slug, cancellationToken);
        }
    }

    private static async Task CleanupAsync(string connectionString, int rowId, string slug, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var targetId = rowId;
        if (targetId <= 0)
        {
            targetId = await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition(
                "SELECT TOP (1) [id] FROM [dbo].[tbl_post] WHERE [slug] = @Slug ORDER BY [id] DESC;",
                new { Slug = slug },
                cancellationToken: cancellationToken));
        }

        if (targetId <= 0)
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM [dbo].[tbl_post_lang] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_post_meta] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_post] WHERE [id] = @Id;",
            new { Id = targetId },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }
}