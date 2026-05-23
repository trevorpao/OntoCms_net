using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

namespace OntoCms.Web.Bootstrap;

internal static class DatabaseBootstrapper
{
    private const int MaxRetryAttempts = 30;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public static async Task InitializeAsync(IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Skipping DB bootstrap because DefaultConnection is not configured.");
            return;
        }

        var sqlDirectory = Path.Combine(AppContext.BaseDirectory, "sql");
        if (!Directory.Exists(sqlDirectory))
        {
            logger.LogWarning("Skipping DB bootstrap because SQL directory was not published: {SqlDirectory}", sqlDirectory);
            return;
        }

        await RetryAsync(() => EnsureDatabaseExistsAsync(connectionString, cancellationToken), logger, cancellationToken);

        var needsBootstrap = await RetryAsync(() => NeedsBootstrapAsync(connectionString, cancellationToken), logger, cancellationToken);
        if (!needsBootstrap)
        {
            logger.LogInformation("DB bootstrap skipped because dbo.tbl_option already exists.");
            return;
        }

        foreach (var scriptPath in GetOrderedScriptPaths(sqlDirectory))
        {
            logger.LogInformation("Running SQL bootstrap script: {ScriptPath}", Path.GetFileName(scriptPath));
            var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            await RetryAsync(() => ExecuteScriptAsync(connectionString, script, cancellationToken), logger, cancellationToken);
        }
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException("DefaultConnection must include an initial catalog.");
        }

        var databaseName = builder.InitialCatalog;
        var escapedDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);

        builder.InitialCatalog = "master";

        const string template = """
IF DB_ID(@DatabaseName) IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [{0}]');
END;
""";

        var sql = string.Format(template, escapedDatabaseName);

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { DatabaseName = databaseName },
            commandTimeout: 120,
            cancellationToken: cancellationToken));
    }

    private static async Task<bool> NeedsBootstrapAsync(string connectionString, CancellationToken cancellationToken)
    {
        const string sql = "SELECT CASE WHEN OBJECT_ID(N'[dbo].[tbl_option]', N'U') IS NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            sql,
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    private static async Task ExecuteScriptAsync(string connectionString, string script, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in SplitSqlBatches(script))
        {
            await connection.ExecuteAsync(new CommandDefinition(
                batch,
                commandTimeout: 0,
                cancellationToken: cancellationToken));
        }
    }

    private static IEnumerable<string> GetOrderedScriptPaths(string sqlDirectory)
    {
        return Directory
            .EnumerateFiles(sqlDirectory, "*.sql", SearchOption.TopDirectoryOnly)
            .OrderBy(path => Path.GetFileName(path).Equals("init.sql", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitSqlBatches(string script)
    {
        var builder = new StringBuilder();

        foreach (var line in script.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = builder.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    yield return batch;
                }

                builder.Clear();
                continue;
            }

            builder.AppendLine(line);
        }

        var tail = builder.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(tail))
        {
            yield return tail;
        }
    }

    private static async Task RetryAsync(Func<Task> action, ILogger logger, CancellationToken cancellationToken)
    {
        await RetryAsync(async () =>
        {
            await action();
            return true;
        }, logger, cancellationToken);
    }

    private static async Task<T> RetryAsync<T>(Func<Task<T>> action, ILogger logger, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException && attempt < MaxRetryAttempts)
            {
                logger.LogInformation(ex, "DB bootstrap retry {Attempt}/{MaxRetryAttempts}.", attempt, MaxRetryAttempts);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        return await action();
    }
}