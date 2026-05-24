using OntoCms.Conventions.Attributes;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Menu;

[MTB("menu")]
[MULTILANG(true)]
public sealed class MenuFeed : BaseFeedRepository<MenuFeed.WriteModel>
{
    private readonly IConfiguration configuration;

    public MenuFeed(IConfiguration configuration)
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
            await SaveLangAsync(connection, transaction, rowId, columns.Lang, actorUserId: 0, token);
            return rowId;
        }, cancellationToken);
    }

    public sealed record WriteModel(
        int Id,
        string Status,
        string Blank,
        int ParentId,
        string Uri,
        string Theme,
        string? Color,
        string? Icon,
        int Sorter,
        string? Cover,
        IReadOnlyDictionary<string, LangWriteModel>? Lang = null);

    public sealed record LangWriteModel(
        string Title,
        string? Badge,
        string? Info);
}