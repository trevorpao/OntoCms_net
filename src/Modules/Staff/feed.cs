using Dapper;
using Microsoft.Data.SqlClient;

namespace OntoCms.Modules.Staff;

public sealed class StaffFeed
{
    private readonly IConfiguration configuration;

    public StaffFeed(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<AuthProfile?> GetAuthProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        const string sql = """
SELECT
    s.id AS Id,
    s.status AS Status,
    s.role_id AS RoleId,
    s.account AS Account,
    s.pwd AS PasswordHash,
    r.status AS RoleStatus,
    r.title AS RoleTitle,
    r.priv AS RolePriv
FROM dbo.tbl_staff AS s
INNER JOIN dbo.tbl_role AS r ON r.id = s.role_id
WHERE s.id = @Id;
""";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<AuthProfile>(new CommandDefinition(
            sql,
            new { Id = id },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    public async Task<AuthProfile?> GetAuthProfileByAccountAsync(string account, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        const string sql = """
SELECT
    s.id AS Id,
    s.status AS Status,
    s.role_id AS RoleId,
    s.account AS Account,
    s.pwd AS PasswordHash,
    r.status AS RoleStatus,
    r.title AS RoleTitle,
    r.priv AS RolePriv
FROM dbo.tbl_staff AS s
INNER JOIN dbo.tbl_role AS r ON r.id = s.role_id
WHERE s.account = @Account;
""";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<AuthProfile>(new CommandDefinition(
            sql,
            new { Account = account },
            commandTimeout: 30,
            cancellationToken: cancellationToken));
    }

    public sealed record AuthProfile
    {
        public int Id { get; init; }

        public string Status { get; init; } = string.Empty;

        public int RoleId { get; init; }

        public string Account { get; init; } = string.Empty;

        public string? PasswordHash { get; init; }

        public string RoleStatus { get; init; } = string.Empty;

        public string RoleTitle { get; init; } = string.Empty;

        public int RolePriv { get; init; }
    }
}