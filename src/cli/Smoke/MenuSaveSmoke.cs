using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OntoCms.Modules.Menu;

namespace OntoCms.Cli.Smoke;

internal static class MenuSaveSmoke
{
    public static async Task RunAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var uri = $"smoke/menu/{Guid.NewGuid():N}";
        var twTitle = $"選單 smoke {Guid.NewGuid():N}";
        var enTitle = $"Menu smoke {Guid.NewGuid():N}";
        var feed = new MenuFeed(configuration);
        int rowId = 0;

        try
        {
            rowId = await feed.SaveAsync(new MenuFeed.WriteModel(
                Id: 0,
                Status: "Enabled",
                Blank: "No",
                ParentId: 0,
                Uri: uri,
                Theme: "Basic",
                Color: "info",
                Icon: "list",
                Sorter: 99,
                Cover: string.Empty,
                Lang: new Dictionary<string, MenuFeed.LangWriteModel>
                {
                    ["tw"] = new(twTitle, string.Empty, "menu smoke info"),
                    ["en"] = new(enTitle, string.Empty, "menu smoke info"),
                }),
                cancellationToken);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var main = await connection.QuerySingleAsync<(string Uri, string Status, string Blank, int ParentId)>(new CommandDefinition(
                "SELECT [uri] AS [Uri], [status] AS [Status], [blank] AS [Blank], [parent_id] AS [ParentId] FROM [dbo].[tbl_menu] WHERE [id] = @Id;",
                new { Id = rowId },
                cancellationToken: cancellationToken));

            var langCount = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT COUNT(1) FROM [dbo].[tbl_menu_lang] WHERE [parent_id] = @Id AND [title] IN (@TwTitle, @EnTitle);",
                new { Id = rowId, TwTitle = twTitle, EnTitle = enTitle },
                cancellationToken: cancellationToken));

            if (!string.Equals(main.Uri, uri, StringComparison.Ordinal)
                || !string.Equals(main.Status, "Enabled", StringComparison.Ordinal)
                || !string.Equals(main.Blank, "No", StringComparison.Ordinal)
                || main.ParentId != 0
                || langCount != 2)
            {
                throw new InvalidOperationException("Menu save smoke validation failed.");
            }

            logger.LogInformation("Menu save smoke passed for row {RowId}.", rowId);
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
                "SELECT TOP (1) [id] FROM [dbo].[tbl_menu] WHERE [uri] = @Uri ORDER BY [id] DESC;",
                new { Uri = uri },
                cancellationToken: cancellationToken));
        }

        if (targetId <= 0)
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM [dbo].[tbl_menu_lang] WHERE [parent_id] = @Id; DELETE FROM [dbo].[tbl_menu] WHERE [id] = @Id;",
            new { Id = targetId },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }
}