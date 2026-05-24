using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OntoCms.Modules.Post;

namespace OntoCms.Cli.Smoke;

internal static class PostSaveSmoke
{
    public static async Task RunTagIdsAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var feed = new PostFeed(configuration);
        var slug = $"smoke-post-tagids-{Guid.NewGuid():N}";
        var rowId = 0;

        try
        {
            rowId = await feed.SaveAsync(new PostFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Slug: slug,
                Cover: string.Empty,
                Layout: "normal",
                Lang: new Dictionary<string, PostFeed.LangWriteModel>
                {
                    ["tw"] = new("TagIds 驗證文章", "tag ids content", "No"),
                },
                Tags: new[] { "101", "202" }),
                cancellationToken);

            var tagIds = await feed.GetTagIdsAsync(rowId, cancellationToken);

            if (tagIds.Count != 2
                || tagIds[0] != 101
                || tagIds[1] != 202)
            {
                throw new InvalidOperationException("Post tag ids smoke validation failed.");
            }

            logger.LogInformation("Post tag ids smoke passed for row {RowId}.", rowId);
        }
        finally
        {
            await CleanupAsync(connectionString, rowId, slug, cancellationToken);
        }
    }

    public static async Task RunByTagAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var feed = new PostFeed(configuration);
        var primarySlug = $"smoke-post-bytag-primary-{Guid.NewGuid():N}";
        var secondarySlug = $"smoke-post-bytag-secondary-{Guid.NewGuid():N}";
        var primaryId = 0;
        var secondaryId = 0;

        try
        {
            primaryId = await feed.SaveAsync(new PostFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Slug: primarySlug,
                Cover: string.Empty,
                Layout: "normal",
                Lang: new Dictionary<string, PostFeed.LangWriteModel>
                {
                    ["tw"] = new("ByTag 主文章", "primary bytag content", "No"),
                },
                Tags: new[] { "101", "202" }),
                cancellationToken);

            secondaryId = await feed.SaveAsync(new PostFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Slug: secondarySlug,
                Cover: string.Empty,
                Layout: "normal",
                Lang: new Dictionary<string, PostFeed.LangWriteModel>
                {
                    ["tw"] = new("ByTag 次文章", "secondary bytag content", "No"),
                },
                Tags: new[] { "202" }),
                cancellationToken);

            var singleTag = await feed.GetIdsByTagAsync(new[] { 101 }, cancellationToken);
            var intersectedTags = await feed.GetIdsByTagAsync(new[] { 101, 202 }, cancellationToken);

            if (!singleTag.Contains(primaryId)
                || singleTag.Contains(secondaryId)
                || intersectedTags.Count != 1
                || intersectedTags[0] != primaryId)
            {
                throw new InvalidOperationException("Post byTag smoke validation failed.");
            }

            logger.LogInformation("Post byTag smoke passed for rows {PrimaryId} and {SecondaryId}.", primaryId, secondaryId);
        }
        finally
        {
            await CleanupAsync(connectionString, primaryId, primarySlug, cancellationToken);
            await CleanupAsync(connectionString, secondaryId, secondarySlug, cancellationToken);
        }
    }

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
                },
                Tags: new[] { "101", "202" }),
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

            var tagCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_tag] WHERE [post_id] = @Id AND [tag_id] IN (101, 202);",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            if (!string.Equals(main.Slug, slug, StringComparison.Ordinal)
                || !string.Equals(main.Status, "Enabled", StringComparison.Ordinal)
                || !string.Equals(main.Layout, "normal", StringComparison.Ordinal)
                || metaCount != 1
                || langCount != 2
                || tagCount != 2)
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
                },
                Tags: new[] { "303", "404" }),
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

            var tagCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_post_tag] WHERE [tag_id] IN (303, 404);",
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

            if (tagCount != 0)
            {
                throw new InvalidOperationException("Rollback smoke failed because tbl_post_tag row still exists after SaveAsync error.", ex);
            }

            logger.LogInformation("Post save rollback smoke passed. SaveAsync failed and no tbl_post/tbl_post_meta/tbl_post_lang/tbl_post_tag rows remained for slug {Slug}.", slug);
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
            "DELETE FROM [dbo].[tbl_post_tag] WHERE [post_id] = @Id; DELETE FROM [dbo].[tbl_post_lang] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_post_meta] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_post] WHERE [id] = @Id;",
            new { Id = targetId },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }
}