using Microsoft.AspNetCore.Mvc;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Option;

[ApiController]
[Route("api/option")]
public sealed class OptionReaction : BaseReactionController
{
    private readonly OptionFeed optionFeed;

    public OptionReaction(OptionFeed optionFeed)
    {
        this.optionFeed = optionFeed;
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int id, CancellationToken cancellationToken)
    {
        return await ReactGetAsync(id, optionFeed, cancellationToken);
    }

    [HttpPost("del")]
    public async Task<IActionResult> Delete([FromForm] int id, CancellationToken cancellationToken)
    {
        return await ReactDeleteAsync(id, optionFeed, cancellationToken);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        return await ReactListAsync(query, page, limit, optionFeed, cancellationToken, defaultLimit: 200, minLimit: 12, maxLimit: 200);
    }

    [HttpGet("get_opts")]
    public async Task<IActionResult> GetOptions([FromQuery] string? query, CancellationToken cancellationToken)
    {
        return await ReactGetOptionsAsync(query, optionFeed, cancellationToken);
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromForm] OptionFeed.WriteModel payload, CancellationToken cancellationToken)
    {
        return await ReactSaveAsync(payload, OptionKit.Rule, optionFeed, cancellationToken);
    }
}