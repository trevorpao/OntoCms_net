using System.Reflection;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseFeedRepository<TPayload> : IFeedRepository<TPayload>
    where TPayload : class
{
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
        CancellationToken cancellationToken = default)
    {
        if (id > 0)
        {
            return await UpdateMainRowAsync(connection, columns.Main, id, cancellationToken);
        }

        return await InsertMainRowAsync(connection, columns.Main, cancellationToken);
    }

    private async Task<int> InsertMainRowAsync(
        SqlConnection connection,
        IReadOnlyDictionary<string, object?> values,
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
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    private async Task<int> UpdateMainRowAsync(
        SqlConnection connection,
        IReadOnlyDictionary<string, object?> values,
        int id,
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
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    private static string Bracket(string columnName)
    {
        return $"[{columnName.Replace("]", "]]", StringComparison.Ordinal)}]";
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