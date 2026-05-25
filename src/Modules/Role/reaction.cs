using Microsoft.AspNetCore.Mvc;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Role;

[ApiController]
[Route("api/role")]
public sealed class RoleReaction : BaseReactionController
{
    private readonly RoleFeed roleFeed;

    public RoleReaction(RoleFeed roleFeed)
    {
        this.roleFeed = roleFeed;
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int id, CancellationToken cancellationToken)
    {
        return await ReactGetAsync(id, roleFeed, cancellationToken);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        return await ReactListAsync(query, page, limit, roleFeed, cancellationToken, defaultLimit: 12, minLimit: 3, maxLimit: 24);
    }

    [HttpGet("get_opts")]
    public async Task<IActionResult> GetOptions([FromQuery] string? query, CancellationToken cancellationToken)
    {
        return await ReactGetOptionsAsync(query, roleFeed, cancellationToken);
    }

    [HttpGet("get_auth_opts")]
    public async Task<IActionResult> GetAuthorityOptions(CancellationToken cancellationToken)
    {
        return await ExecuteReactionAsync(
            _ => Task.FromResult<IActionResult>(OkDone(RoleFeed.GetAuthorityOptions())),
            cancellationToken);
    }

    protected override TRow HandleRow<TRow>(TRow row)
    {
        if (row is RoleFeed.RoleRecord role)
        {
            var handledRole = role with
            {
                Priv = string.Join(',', RoleFeed.ExpandAuthorityValues(role.PrivValue)),
                Auth = RoleFeed.ExpandAuthorityValues(role.PrivValue),
                Authorities = RoleFeed.ExpandAuthorityNames(role.PrivValue),
            };

            return (TRow)(object)handledRole;
        }

        return base.HandleRow(row);
    }

    protected override TRow HandleIteratee<TRow>(TRow row)
    {
        if (row is RoleFeed.RoleRecord role)
        {
            var handledRole = role with
            {
                Priv = string.Join("<br/>", RoleFeed.ExpandAuthorityTitles(role.PrivValue)),
                Authorities = RoleFeed.ExpandAuthorityNames(role.PrivValue),
            };

            return (TRow)(object)handledRole;
        }

        return base.HandleIteratee(row);
    }
}