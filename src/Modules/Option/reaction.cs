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

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromForm] OptionFeed.WriteModel payload, CancellationToken cancellationToken)
    {
        return await ReactSaveAsync(payload, optionFeed, cancellationToken);
    }
}