using Dapper;
using Microsoft.Data.SqlClient;
using System.Reflection;
using OntoCms.Conventions.Attributes;
using SqlKata;
using SqlKata.Compilers;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseRelationRepository
{
    private static readonly SqlServerCompiler sqlCompiler = new();
    private readonly Lazy<RelationOwnerMetadata> ownerMetadata;

    protected BaseRelationRepository()
    {
        ownerMetadata = new Lazy<RelationOwnerMetadata>(BuildOwnerMetadata);
    }

    protected string TableBaseName => OwnerMetadata.TableBaseName;

    protected string MainTableName => OwnerMetadata.MainTableName;

    protected string OwnerPrimaryKeyColumn => OwnerMetadata.PrimaryKeyColumn;

    protected RelationOwnerMetadata OwnerMetadata => ownerMetadata.Value;

    protected virtual string ResolvePrimaryKeyColumn()
    {
        return "id";
    }

    protected RelationMetadata CreateRelationMetadata(string relationName, bool reverse = false)
    {
        var normalizedRelationName = NormalizeRelationName(relationName);
        var ownerKeyColumn = $"{TableBaseName}_id";
        var relationKeyColumn = $"{normalizedRelationName}_id";

        if (reverse)
        {
            (ownerKeyColumn, relationKeyColumn) = (relationKeyColumn, ownerKeyColumn);
        }

        return new RelationMetadata(
            normalizedRelationName,
            $"tbl_{TableBaseName}_{normalizedRelationName}",
            ownerKeyColumn,
            relationKeyColumn,
            $"{normalizedRelationName}_cnt");
    }

    protected IReadOnlyList<RelationWriteRow> BuildSaveManyRows(
        string relationName,
        int parentId,
        IEnumerable<int> relationIds,
        bool reverse = false,
        bool sortable = false)
    {
        var metadata = CreateRelationMetadata(relationName, reverse);
        var rows = new List<RelationWriteRow>();
        var sorter = 0;

        foreach (var relationId in relationIds)
        {
            if (relationId <= 0)
            {
                continue;
            }

            var values = new Dictionary<string, object?>
            {
                [metadata.OwnerKeyColumn] = parentId,
                [metadata.RelationKeyColumn] = relationId,
            };

            if (sortable)
            {
                values["sorter"] = sorter;
            }

            rows.Add(new RelationWriteRow(values));
            sorter += 1;
        }

        return rows;
    }

    protected async Task ReplaceSaveManyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string relationName,
        int parentId,
        IEnumerable<int> relationIds,
        CancellationToken cancellationToken = default,
        bool reverse = false,
        bool sortable = false)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        var distinctRelationIds = relationIds
            .Where(static relationId => relationId > 0)
            .Distinct()
            .ToArray();

        var metadata = CreateRelationMetadata(relationName, reverse);
        var ownerKeyColumn = reverse ? metadata.RelationKeyColumn : metadata.OwnerKeyColumn;
        var rows = BuildSaveManyRows(relationName, parentId, distinctRelationIds, reverse, sortable);

        var deleteSql = $"DELETE FROM [dbo].[{metadata.RelationTableName}] WHERE {Bracket(ownerKeyColumn)} = @ParentId;";
        await connection.ExecuteAsync(new CommandDefinition(
            deleteSql,
            new { ParentId = parentId },
            transaction,
            commandTimeout: 30,
            cancellationToken: cancellationToken));

        if (rows.Count == 0)
        {
            return;
        }

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var orderedColumns = row.Values.Keys.OrderBy(static column => column, StringComparer.OrdinalIgnoreCase).ToArray();
            var parameters = new DynamicParameters();

            foreach (var column in orderedColumns)
            {
                parameters.Add($"p_{index}_{column}", row.Values[column]);
            }

            var insertSql = $"INSERT INTO [dbo].[{metadata.RelationTableName}] ({string.Join(", ", orderedColumns.Select(Bracket))}) VALUES ({string.Join(", ", orderedColumns.Select(column => $"@p_{index}_{column}"))});";
            await connection.ExecuteAsync(new CommandDefinition(
                insertSql,
                parameters,
                transaction,
                commandTimeout: 30,
                cancellationToken: cancellationToken));
        }
    }

    protected async Task<IReadOnlyList<int>> FindOwnerIdsByRelationAsync(
        SqlConnection connection,
        string relationName,
        IEnumerable<int> relationIds,
        CancellationToken cancellationToken = default,
        bool reverse = false)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var distinctRelationIds = relationIds
            .Where(static relationId => relationId > 0)
            .Distinct()
            .ToArray();

        if (distinctRelationIds.Length == 0)
        {
            return Array.Empty<int>();
        }

        var metadata = CreateRelationMetadata(relationName, reverse);
        var ownerKeyColumn = reverse ? metadata.RelationKeyColumn : metadata.OwnerKeyColumn;
        var relationKeyColumn = reverse ? metadata.OwnerKeyColumn : metadata.RelationKeyColumn;

        var query = NewReadQuery($"[dbo].[{metadata.RelationTableName}]")
            .Select(ownerKeyColumn)
            .WhereIn(relationKeyColumn, distinctRelationIds)
            .GroupBy(ownerKeyColumn)
            .HavingRaw($"COUNT(DISTINCT {Bracket(relationKeyColumn)}) = ?", distinctRelationIds.Length)
            .OrderBy(ownerKeyColumn);

        var rows = await ReadManyAsync<int>(connection, query, cancellationToken);

        return rows.ToArray();
    }

    protected async Task<IReadOnlyList<int>> FindRelationIdsByOwnerAsync(
        SqlConnection connection,
        string relationName,
        int ownerId,
        CancellationToken cancellationToken = default,
        bool reverse = false)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (ownerId <= 0)
        {
            return Array.Empty<int>();
        }

        var metadata = CreateRelationMetadata(relationName, reverse);
        var ownerKeyColumn = reverse ? metadata.RelationKeyColumn : metadata.OwnerKeyColumn;
        var relationKeyColumn = reverse ? metadata.OwnerKeyColumn : metadata.RelationKeyColumn;

        var query = NewReadQuery($"[dbo].[{metadata.RelationTableName}]")
            .Select(relationKeyColumn)
            .Where(ownerKeyColumn, ownerId)
            .OrderBy(relationKeyColumn);

        var rows = await ReadManyAsync<int>(connection, query, cancellationToken);

        return rows.ToArray();
    }

    protected static Query NewReadQuery(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        return new Query(tableName);
    }

    protected static CommandDefinition CompileReadCommand(
        Query query,
        object? overrides = null,
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
            commandTimeout: commandTimeout,
            cancellationToken: cancellationToken);
    }

    protected static async Task<IReadOnlyList<TResult>> ReadManyAsync<TResult>(
        SqlConnection connection,
        Query query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(query);

        var rows = await connection.QueryAsync<TResult>(
            CompileReadCommand(query, cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    private RelationOwnerMetadata BuildOwnerMetadata()
    {
        var repositoryType = GetType();
        var mtb = repositoryType.GetCustomAttribute<MTBAttribute>();
        if (mtb is null || string.IsNullOrWhiteSpace(mtb.TableBaseName))
        {
            throw new InvalidOperationException($"{repositoryType.Name} must declare [MTB(\"table_base\")].");
        }

        var tableBaseName = mtb.TableBaseName.Trim();

        return new RelationOwnerMetadata(
            tableBaseName,
            $"tbl_{tableBaseName}",
            ResolvePrimaryKeyColumn());
    }

    private static string NormalizeRelationName(string relationName)
    {
        if (string.IsNullOrWhiteSpace(relationName))
        {
            throw new ArgumentException("Relation name is required.", nameof(relationName));
        }

        return relationName.Trim().ToLowerInvariant();
    }

    private static string Bracket(string value)
    {
        return $"[{value}]";
    }

    protected sealed record RelationOwnerMetadata(
        string TableBaseName,
        string MainTableName,
        string PrimaryKeyColumn);

    protected sealed record RelationMetadata(
        string RelationName,
        string RelationTableName,
        string OwnerKeyColumn,
        string RelationKeyColumn,
        string RelationCountColumn);

    protected sealed record RelationWriteRow(IReadOnlyDictionary<string, object?> Values);
}