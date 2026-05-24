using Microsoft.AspNetCore.Mvc;
using OntoCms.Conventions.HMVC;
using OntoCms.Modules.Post;

namespace OntoCms.Modules.Option;

[Controller]
public sealed class OptionOutfit : BaseOutfitController
{
	private static readonly string[] HomePageLanguages = ["tw", "en"];

	private readonly OptionFeed optionFeed;
	private readonly PostFeed postFeed;

	public OptionOutfit(OptionFeed optionFeed, PostFeed postFeed)
	{
		this.optionFeed = optionFeed;
		this.postFeed = postFeed;
	}

	[HttpGet("/")]
	[HttpGet("/about")]
	[HttpGet("/{routeLang:regex(^tw|en$)}")]
	[HttpGet("/{routeLang:regex(^tw|en$)}/about")]
	public async Task<IActionResult> Index(
		[FromRoute] string? routeLang,
		[FromQuery(Name = "lang")] string? queryLang,
		CancellationToken cancellationToken)
	{
		return await ExecuteOutfitAsync(async token =>
		{
			var currentLang = ResolveFrontendLanguage(routeLang, queryLang, HomePageLanguages);
			PersistFrontendLanguageCookie(currentLang, routeLang, queryLang, HomePageLanguages);

			var siteTitle = await optionFeed.GetSiteTitleAsync(token);
			var aboutPost = await postFeed.GetPublishedBySlugAsync("about", currentLang, token);

			var model = aboutPost is null
				? new HomePageViewModel(siteTitle, siteTitle, string.Empty, currentLang)
				: new HomePageViewModel(siteTitle, aboutPost.Title, aboutPost.Content, aboutPost.Lang);

			return ThemeView("Home/Index", model);
		}, cancellationToken);
	}
}

public sealed record HomePageViewModel(
	string SiteTitle,
	string PageTitle,
	string Content,
	string CurrentLang)
{
	public string HtmlLang => string.Equals(CurrentLang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "zh-Hant";

	public string DocumentTitle => string.Equals(PageTitle, SiteTitle, StringComparison.Ordinal)
		? SiteTitle
		: $"{PageTitle} | {SiteTitle}";

	public string ChineseAboutPath => "/tw/about";

	public string EnglishAboutPath => "/en/about";
}