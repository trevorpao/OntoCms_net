using System.Reflection;
using OntoCms.Conventions.Attributes;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseRelationRepository
{
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