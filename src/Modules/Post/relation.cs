using Microsoft.Data.SqlClient;
using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Post;

[MTB("post")]
public sealed class PostRelationRepository : BaseRelationRepository
{
    public Task SaveTagsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int postId,
        IReadOnlyList<string> tagValues,
        CancellationToken cancellationToken = default)
    {
        return ReplaceSaveManyAsync(
            connection,
            transaction,
            "tag",
            postId,
            ParseTagIds(tagValues),
            cancellationToken);
    }

    private static IReadOnlyList<int> ParseTagIds(IReadOnlyList<string> tagValues)
    {
        var parsedIds = new List<int>(tagValues.Count);
        foreach (var tagValue in tagValues)
        {
            if (!int.TryParse(tagValue, out var tagId) || tagId <= 0)
            {
                continue;
            }

            parsedIds.Add(tagId);
        }

        return parsedIds;
    }
}