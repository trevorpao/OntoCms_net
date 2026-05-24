using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Adv;

[MTB("adv")]
[MULTILANG(true)]
public sealed class AdvFeed : BaseFeedRepository<AdvFeed.WriteModel>
{
    private readonly IConfiguration configuration;

    public AdvFeed(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public override Task<int> SaveAsync(WriteModel payload, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var columns = ApplyAuditFields(
            HandleColumns(payload),
            actorUserId: 0,
            isInsert: payload.Id <= 0);

        return WithTransactionAsync(connectionString, async (connection, transaction, token) =>
        {
            var rowId = await SaveMainRowAsync(connection, columns, payload.Id, transaction, token);
            await SaveMetaAsync(connection, transaction, rowId, columns.Meta, token);
            await SaveLangAsync(connection, transaction, rowId, columns.Lang, actorUserId: 0, token);
            return rowId;
        }, cancellationToken);
    }

    public sealed record WriteModel(
        int Id,
        int PositionId,
        int Counter,
        int Exposure,
        string Status,
        int Weight,
        string? Theme,
        DateTimeOffset? StartDate,
        DateTimeOffset? EndDate,
        string Uri,
        string Cover,
        string Background,
        IReadOnlyDictionary<string, object?>? Meta = null,
        IReadOnlyDictionary<string, LangWriteModel>? Lang = null);

    public sealed record LangWriteModel(
        string Title,
        string? Subtitle,
        string? Content);
}