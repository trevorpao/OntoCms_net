using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Dapper;
using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using SqlKata;
using SqlKata.Compilers;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseFeedRepository<TPayload> : IFeedRepository<TPayload>
    where TPayload : class
{
    private static readonly SqlServerCompiler sqlCompiler = new();
    private readonly Lazy<FeedMetadata> metadata;

    protected BaseFeedRepository()
    {
        metadata = new Lazy<FeedMetadata>(BuildMetadata);
    }

    protected string TableBaseName => Metadata.TableBaseName;

    protected bool SupportsMultilang => Metadata.SupportsMultilang;

    protected string PrimaryKeyColumn => Metadata.PrimaryKeyColumn;

    protected string MainTableName => Metadata.MainTableName;

    protected string LangTableName => Metadata.LangTableName;

    protected string MetaTableName => Metadata.MetaTableName;

    protected FeedMetadata Metadata => metadata.Value;

    public abstract Task<int> SaveAsync(TPayload payload, CancellationToken cancellationToken = default);

    protected ColumnSplitResult HandleColumns(TPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var main = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var meta = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var lang = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var tags = new List<string>();

        foreach (var property in payload.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead)
            {
                continue;
            }

            var columnName = ResolvePayloadColumnName(property);
            if (!FilterColumn(columnName))
            {
                continue;
            }

            var value = property.GetValue(payload);
            switch (columnName)
            {
                case "id":
                    break;

                case "meta":
                    CopyNamedValues(value, meta);
                    break;

                case "lang":
                    CopyNamedValues(value, lang);
                    break;

                case "tags":
                    CopyTags(value, tags);
                    break;

                default:
                    main[columnName] = NormalizeColumnValue(columnName, value);
                    break;
            }
        }

        return new ColumnSplitResult(main, meta, lang, tags);
    }

    protected virtual string ResolvePrimaryKeyColumn()
    {
        return "id";
    }

    protected virtual bool FilterColumn(string columnName)
    {
        return !string.IsNullOrWhiteSpace(columnName);
    }

    protected virtual string ResolvePayloadColumnName(PropertyInfo property)
    {
        return ToSnakeCase(property.Name);
    }

    protected virtual object? NormalizeColumnValue(string columnName, object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string stringValue)
        {
            return stringValue.Trim();
        }

        if (value is DateTimeOffset or DateTime or Guid)
        {
            return value;
        }

        if (value is System.Collections.IEnumerable && value is not string)
        {
            return JsonSerializer.Serialize(value);
        }

        return value;
    }

    protected ColumnSplitResult ApplyAuditFields(
        ColumnSplitResult columns,
        int actorUserId,
        DateTimeOffset? currentTime = null,
        bool isInsert = false)
    {
        var timestamp = currentTime ?? DateTimeOffset.UtcNow;
        var main = new Dictionary<string, object?>(columns.Main, StringComparer.OrdinalIgnoreCase)
        {
            ["last_ts"] = timestamp,
            ["last_user"] = actorUserId,
        };

        if (isInsert)
        {
            main["insert_ts"] = timestamp;
            main["insert_user"] = actorUserId;
        }

        return columns with { Main = main };
    }

    protected async Task<int> SaveMainRowAsync(
        SqlConnection connection,
        ColumnSplitResult columns,
        int id,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        if (id > 0)
        {
            return await UpdateMainRowAsync(connection, columns.Main, id, transaction, cancellationToken);
        }

        return await InsertMainRowAsync(connection, columns.Main, transaction, cancellationToken);
    }

    protected async Task<bool> DeleteMainRowAsync(
        SqlConnection connection,
        int id,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Delete requires a positive id.");
        }

        var sql = $"DELETE FROM [dbo].[{MainTableName}] WHERE [{PrimaryKeyColumn}] = @Id;";
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id },
            transaction,
            commandTimeout: 30,
            cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    protected async Task SaveMetaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int parentId,
        IReadOnlyDictionary<string, object?> values,
        CancellationToken cancellationToken = default)
    {
        var deleteSql = $"DELETE FROM [dbo].[{MetaTableName}] WHERE [parent_id] = @ParentId;";
        await connection.ExecuteAsync(new CommandDefinition(
            deleteSql,
            new { ParentId = parentId },
            transaction,
            commandTimeout: 30,
            cancellationToken: cancellationToken));

        if (values.Count == 0)
        {
            return;
        }

        var insertSql = $"""
INSERT INTO [dbo].[{MetaTableName}] ([parent_id], [k], [v], [last_ts])
VALUES (@ParentId, @Key, @Value, @Timestamp);
""";

        var timestamp = DateTimeOffset.UtcNow;
        foreach (var item in values)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                insertSql,
                new
                {
                    ParentId = parentId,
                    Key = item.Key,
                    Value = NormalizeMetaValue(item.Value),
                    Timestamp = timestamp,
                },
                transaction,
                commandTimeout: 30,
                cancellationToken: cancellationToken));
        }
    }

    protected async Task SaveLangAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int parentId,
        IReadOnlyDictionary<string, object?> values,
        int actorUserId = 0,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsMultilang)
        {
            if (values.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException($"{GetType().Name} does not support _lang persistence.");
        }

        if (values.Count == 0)
        {
            return;
        }

        var activeRows = values
            .Where(static item => !string.IsNullOrWhiteSpace(item.Key) && item.Value is not null)
            .Select(item => new
            {
                Lang = item.Key.Trim(),
                Values = ExtractNamedValues(item.Value),
            })
            .Where(static item => item.Values.Count > 0)
            .ToArray();

        if (activeRows.Length == 0)
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow;

        for (var index = 0; index < activeRows.Length; index++)
        {
            var row = activeRows[index];
            var rowValues = new Dictionary<string, object?>(row.Values, StringComparer.OrdinalIgnoreCase)
            {
                ["lang"] = row.Lang,
                ["parent_id"] = parentId,
                ["last_ts"] = timestamp,
                ["last_user"] = actorUserId,
            };

            rowValues.TryAdd("insert_ts", timestamp);
            rowValues.TryAdd("insert_user", actorUserId);

            var orderedColumns = rowValues.Keys.OrderBy(static column => column, StringComparer.OrdinalIgnoreCase).ToArray();
            var updateColumns = orderedColumns
                .Where(static column => !column.Equals("lang", StringComparison.OrdinalIgnoreCase)
                    && !column.Equals("parent_id", StringComparison.OrdinalIgnoreCase)
                    && !column.Equals("insert_ts", StringComparison.OrdinalIgnoreCase)
                    && !column.Equals("insert_user", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var parameters = new DynamicParameters();
            var sourceColumns = new List<string>(orderedColumns.Length);
            foreach (var column in orderedColumns)
            {
                var parameterName = $"p_{index}_{column}";
                parameters.Add(parameterName, rowValues[column]);
                sourceColumns.Add($"@{parameterName} AS {Bracket(column)}");
            }

            var sql = $"""
MERGE [dbo].[{LangTableName}] AS target
USING (SELECT {string.Join(", ", sourceColumns)}) AS source
ON target.[parent_id] = source.[parent_id]
AND target.[lang] = source.[lang]
WHEN MATCHED THEN
    UPDATE SET {string.Join(", ", updateColumns.Select(column => $"target.{Bracket(column)} = source.{Bracket(column)}"))}
WHEN NOT MATCHED THEN
    INSERT ({string.Join(", ", orderedColumns.Select(Bracket))})
    VALUES ({string.Join(", ", orderedColumns.Select(column => $"source.{Bracket(column)}"))});
""";

            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                parameters,
                transaction,
                commandTimeout: 30,
                cancellationToken: cancellationToken));
        }
    }

    protected async Task<TResult> WithTransactionAsync<TResult>(
        string connectionString,
        Func<SqlConnection, SqlTransaction, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(connection, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    protected static Query NewQuery(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        return new Query(tableName);
    }

    protected static CommandDefinition CompileCommand(
        Query query,
        object? overrides = null,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default,
        int? commandTimeout = 30)
    {
        ArgumentNullException.ThrowIfNull(query);

        var compiled = sqlCompiler.Compile(query);
        var parameters = new DynamicParameters();

        foreach (var binding in compiled.NamedBindings)
        {
            parameters.Add(binding.Key, binding.Value);
        }

        if (overrides is not null)
        {
            parameters.AddDynamicParams(overrides);
        }

        return new CommandDefinition(
            compiled.Sql,
            parameters,
            transaction,
            commandTimeout,
            cancellationToken: cancellationToken);
    }

    protected virtual int ReadLotsLimit()
    {
        return 500;
    }

    protected virtual int ReadPageLimit()
    {
        return 12;
    }

    protected async Task<IReadOnlyList<TResult>> LotsAsync<TResult>(
        SqlConnection connection,
        Query query,
        CancellationToken cancellationToken = default,
        int? limit = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(query);

        var normalizedLimit = NormalizeReadLimit(limit, ReadLotsLimit());
        var effectiveQuery = query.Clone();

        if (normalizedLimit > 0 && !effectiveQuery.HasLimit())
        {
            effectiveQuery.Limit(normalizedLimit);
        }

        var rows = await connection.QueryAsync<TResult>(
            CompileCommand(effectiveQuery, cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    protected async Task<TResult?> OneAsync<TResult>(
        SqlConnection connection,
        Query query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(query);

        var effectiveQuery = query.Clone();

        if (!effectiveQuery.HasLimit())
        {
            effectiveQuery.Limit(1);
        }

        return await connection.QueryFirstOrDefaultAsync<TResult>(
            CompileCommand(effectiveQuery, cancellationToken: cancellationToken));
    }

    protected async Task<FeedPageResult<TResult>> LimitRowsAsync<TResult>(
        SqlConnection connection,
        Query query,
        int page = 0,
        int limit = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(query);

        var normalizedLimit = NormalizeReadLimit(limit, ReadPageLimit());
        var totalQuery = BuildCountQuery(query);
        var total = await connection.ExecuteScalarAsync<int>(
            CompileCommand(totalQuery, cancellationToken: cancellationToken));

        if (total <= 0)
        {
            return new FeedPageResult<TResult>(Array.Empty<TResult>(), 0, normalizedLimit, 0, 0);
        }

        var count = (int)Math.Ceiling(total / (double)normalizedLimit);
        var pos = Math.Max(0, Math.Min(page, count - 1));

        var subsetQuery = query.Clone()
            .ClearComponent("limit")
            .ClearComponent("offset")
            .ForPage(pos + 1, normalizedLimit);

        var subset = await connection.QueryAsync<TResult>(
            CompileCommand(subsetQuery, cancellationToken: cancellationToken));

        return new FeedPageResult<TResult>(subset.ToArray(), total, normalizedLimit, count, pos);
    }

    protected static Query BuildCountQuery(Query query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var countSource = query.Clone()
            .ClearComponent("order")
            .ClearComponent("limit")
            .ClearComponent("offset")
            .As("count_src");

        return new Query()
            .From(countSource)
            .SelectRaw("COUNT(1) AS [count]");
    }

    private static int NormalizeReadLimit(int? requestedLimit, int defaultLimit)
    {
        if (requestedLimit.HasValue && requestedLimit.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedLimit), "Limit must be zero or positive.");
        }

        if (defaultLimit <= 0)
        {
            throw new InvalidOperationException("Default read limit must be positive.");
        }

        return requestedLimit.GetValueOrDefault() > 0
            ? requestedLimit.Value
            : defaultLimit;
    }

    private async Task<int> InsertMainRowAsync(
        SqlConnection connection,
        IReadOnlyDictionary<string, object?> values,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Insert requires at least one main-table column.");
        }

        var orderedColumns = values.Keys.OrderBy(static column => column, StringComparer.OrdinalIgnoreCase).ToArray();
        var parameters = new DynamicParameters();
        var parameterNames = new List<string>(orderedColumns.Length);

        foreach (var column in orderedColumns)
        {
            var parameterName = $"p_{column}";
            parameterNames.Add($"@{parameterName}");
            parameters.Add(parameterName, values[column]);
        }

        var sql = $"""
INSERT INTO [dbo].[{MainTableName}] ({string.Join(", ", orderedColumns.Select(Bracket))})
OUTPUT INSERTED.[{PrimaryKeyColumn}]
VALUES ({string.Join(", ", parameterNames)});
""";

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            parameters,
            transaction,
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    private async Task<int> UpdateMainRowAsync(
        SqlConnection connection,
        IReadOnlyDictionary<string, object?> values,
        int id,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Update requires at least one main-table column.");
        }

        var orderedColumns = values.Keys.OrderBy(static column => column, StringComparer.OrdinalIgnoreCase).ToArray();
        var parameters = new DynamicParameters();
        var assignments = new List<string>(orderedColumns.Length);

        foreach (var column in orderedColumns)
        {
            var parameterName = $"p_{column}";
            assignments.Add($"{Bracket(column)} = @{parameterName}");
            parameters.Add(parameterName, values[column]);
        }

        parameters.Add("id", id);

        var sql = $"""
UPDATE [dbo].[{MainTableName}]
SET {string.Join(", ", assignments)}
WHERE [{PrimaryKeyColumn}] = @id;

SELECT [{PrimaryKeyColumn}]
FROM [dbo].[{MainTableName}]
WHERE [{PrimaryKeyColumn}] = @id;
""";

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            parameters,
            transaction,
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    private static string Bracket(string columnName)
    {
        return $"[{columnName.Replace("]", "]]", StringComparison.Ordinal)}]";
    }

    private static string? NormalizeMetaValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string stringValue)
        {
            return stringValue.Trim();
        }

        if (value is System.Collections.IEnumerable && value is not string)
        {
            return JsonSerializer.Serialize(value);
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    private Dictionary<string, object?> ExtractNamedValues(object? value)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (value is null)
        {
            return result;
        }

        if (value is IReadOnlyDictionary<string, object?> readOnlyValues)
        {
            foreach (var item in readOnlyValues)
            {
                result[item.Key] = NormalizeColumnValue(item.Key, item.Value);
            }

            return result;
        }

        if (value is IDictionary<string, object?> values)
        {
            foreach (var item in values)
            {
                result[item.Key] = NormalizeColumnValue(item.Key, item.Value);
            }

            return result;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                if (item.Key is string key)
                {
                    result[key] = NormalizeColumnValue(key, item.Value);
                }
            }

            return result;
        }

        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead)
            {
                continue;
            }

            var key = ResolvePayloadColumnName(property);
            result[key] = NormalizeColumnValue(key, property.GetValue(value));
        }

        return result;
    }

    private static void CopyNamedValues(object? value, IDictionary<string, object?> target)
    {
        if (value is null)
        {
            return;
        }

        if (value is IReadOnlyDictionary<string, object?> readOnlyValues)
        {
            foreach (var item in readOnlyValues)
            {
                target[item.Key] = item.Value;
            }

            return;
        }

        if (value is IDictionary<string, object?> values)
        {
            foreach (var item in values)
            {
                target[item.Key] = item.Value;
            }

            return;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                if (item.Key is string key)
                {
                    target[key] = item.Value;
                }
            }
        }
    }

    private static void CopyTags(object? value, ICollection<string> target)
    {
        switch (value)
        {
            case null:
                return;

            case string csv:
                foreach (var tag in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    target.Add(tag);
                }

                return;

            case IEnumerable<string> tags:
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        target.Add(tag.Trim());
                    }
                }

                return;
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            if (char.IsUpper(current))
            {
                if (index > 0)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private FeedMetadata BuildMetadata()
    {
        var repositoryType = GetType();
        var mtb = repositoryType.GetCustomAttribute<MTBAttribute>();
        if (mtb is null || string.IsNullOrWhiteSpace(mtb.TableBaseName))
        {
            throw new InvalidOperationException($"{repositoryType.Name} must declare [MTB(\"table_base\")].");
        }

        var tableBaseName = mtb.TableBaseName.Trim();
        var multilang = repositoryType.GetCustomAttribute<MULTILANGAttribute>();

        return new FeedMetadata(
            tableBaseName,
            multilang?.Enabled ?? false,
            ResolvePrimaryKeyColumn());
    }

    protected sealed record FeedMetadata(
        string TableBaseName,
        bool SupportsMultilang,
        string PrimaryKeyColumn)
    {
        public string MainTableName => $"tbl_{TableBaseName}";

        public string LangTableName => $"{MainTableName}_lang";

        public string MetaTableName => $"{MainTableName}_meta";
    }

    protected sealed record ColumnSplitResult(
        IReadOnlyDictionary<string, object?> Main,
        IReadOnlyDictionary<string, object?> Meta,
        IReadOnlyDictionary<string, object?> Lang,
        IReadOnlyList<string> Tags);
}

public sealed record FeedPageResult<TResult>(
    IReadOnlyList<TResult> Subset,
    int Total,
    int Limit,
    int Count,
    int Pos);