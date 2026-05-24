using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OntoCms.Modules.Adv;

namespace OntoCms.Cli.Smoke;

internal static class AdvSaveSmoke
{
    public static async Task RunAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var uri = $"https://example.test/adv/{Guid.NewGuid():N}";
        var twTitle = $"廣告 smoke {Guid.NewGuid():N}";
        var enTitle = $"Adv smoke {Guid.NewGuid():N}";
        var metaValue = $"adv smoke meta {Guid.NewGuid():N}";
        var feed = new AdvFeed(configuration);
        int rowId = 0;

        try
        {
            rowId = await feed.SaveAsync(new AdvFeed.WriteModel(
                Id: 0,
                PositionId: 1,
                Counter: 0,
                Exposure: 0,
                Status: "Enabled",
                Weight: 10,
                Theme: "hero",
                StartDate: null,
                EndDate: null,
                Uri: uri,
                Cover: "/smoke/adv-cover.jpg",
                Background: "/smoke/adv-background.jpg",
                Meta: new Dictionary<string, object?>
                {
                    ["cta"] = metaValue,
                },
                Lang: new Dictionary<string, AdvFeed.LangWriteModel>
                {
                    ["tw"] = new(twTitle, "副標 smoke", "內容 smoke"),
                    ["en"] = new(enTitle, "subtitle smoke", "content smoke"),
                }),
                cancellationToken);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var main = await connection.QuerySingleAsync<(string Uri, string Status, int PositionId, int Weight)>(new CommandDefinition(
                "SELECT [uri] AS [Uri], [status] AS [Status], [position_id] AS [PositionId], [weight] AS [Weight] FROM [dbo].[tbl_adv] WHERE [id] = @Id;",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            var metaCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_adv_meta] WHERE [parent_id] = @Id AND [k] = N'cta' AND [v] = @MetaValue;",
                new { Id = rowId, MetaValue = metaValue },
                cancellationToken: cancellationToken));

            var langCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_adv_lang] WHERE [parent_id] = @Id AND [title] IN (@TwTitle, @EnTitle);",
                new { Id = rowId, TwTitle = twTitle, EnTitle = enTitle },
                cancellationToken: cancellationToken));

            if (!string.Equals(main.Uri, uri, StringComparison.Ordinal)
                || !string.Equals(main.Status, "Enabled", StringComparison.Ordinal)
                || main.PositionId != 1
                || main.Weight != 10
                || metaCount != 1
                || langCount != 2)
            {
                throw new InvalidOperationException("Adv save smoke validation failed.");
            }

            logger.LogInformation("Adv save smoke passed for row {RowId}.", rowId);
        }
        finally
        {
            await CleanupAsync(connectionString, rowId, uri, cancellationToken);
        }
    }

    private static async Task CleanupAsync(string connectionString, int rowId, string uri, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var targetId = rowId;
        if (targetId <= 0)
        {
            targetId = await connection.QuerySingleOrDefaultAsync<int>(new CommandDefinition(
                "SELECT TOP (1) [id] FROM [dbo].[tbl_adv] WHERE [uri] = @Uri ORDER BY [id] DESC;",
                new { Uri = uri },
                cancellationToken: cancellationToken));
        }

        if (targetId <= 0)
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM [dbo].[tbl_adv_lang] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_adv_meta] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_adv] WHERE [id] = @Id;",
            new { Id = targetId },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }
}