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
        return await ExecuteReactionAsync(async token =>
        {
            if (!TryGetPositiveId(id, out var optionId))
            {
                return OkMissing();
            }

            var option = await optionFeed.GetAsync(optionId, token);
            if (option is null)
            {
                return OkMissing();
            }

            return OkDone(HandleRow(option));
        }, cancellationToken);
    }
}