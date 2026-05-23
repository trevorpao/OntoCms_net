using Microsoft.AspNetCore.Mvc;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Option;

[Controller]
public sealed class OptionOutfit : BaseOutfitController
{
	private readonly OptionFeed optionFeed;

	public OptionOutfit(OptionFeed optionFeed)
	{
		this.optionFeed = optionFeed;
	}

	[HttpGet("/")]
	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		return await ExecuteOutfitAsync(async token =>
		{
			var siteTitle = await optionFeed.GetSiteTitleAsync(token);
			return ThemeView("Home/Index", siteTitle);
		}, cancellationToken);
	}
}