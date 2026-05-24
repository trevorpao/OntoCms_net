using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseOutfitController : Controller
{
    protected async Task<IActionResult> ExecuteOutfitAsync(
        Func<CancellationToken, Task<IActionResult>> action,
        CancellationToken cancellationToken = default)
    {
        await BeforeRouteAsync(cancellationToken);

        try
        {
            return await action(cancellationToken);
        }
        finally
        {
            await AfterRouteAsync(cancellationToken);
        }
    }

    protected virtual Task BeforeRouteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterRouteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected ViewResult ThemeView(string relativePath, object? model = null)
    {
        return View($"~/theme/default/frontend/{relativePath}.cshtml", model);
    }

    protected string ResolveFrontendLanguage(
        string? routeLang,
        string? queryLang,
        IReadOnlyCollection<string>? supportedLanguages = null,
        string defaultLang = "tw",
        string cookieName = "user_lang")
    {
        return ForkLanguageResolver.Resolve(
            routeLang,
            queryLang,
            Request.Cookies[cookieName],
            supportedLanguages,
            defaultLang);
    }

    protected void PersistFrontendLanguageCookie(
        string resolvedLang,
        string? routeLang,
        string? queryLang,
        IReadOnlyCollection<string>? supportedLanguages = null,
        string defaultLang = "tw",
        string cookieName = "user_lang")
    {
        if (!ForkLanguageResolver.ShouldPersistCookie(routeLang, queryLang, supportedLanguages, defaultLang))
        {
            return;
        }

        Response.Cookies.Append(cookieName, resolvedLang, new CookieOptions
        {
            HttpOnly = false,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
        });
    }

    protected static IReadOnlyList<BreadcrumbItem> BuildBreadcrumb(
        IEnumerable<BreadcrumbItem> items,
        string homeLabel = "")
    {
        var breadcrumb = new List<BreadcrumbItem>();
        if (!string.IsNullOrWhiteSpace(homeLabel))
        {
            breadcrumb.Add(new BreadcrumbItem("/", homeLabel));
        }

        breadcrumb.AddRange(items.Where(item => !string.IsNullOrWhiteSpace(item.Title)));
        return breadcrumb;
    }

    protected static string BreadcrumbToHtml(IEnumerable<BreadcrumbItem> items, bool asList = true)
    {
        var segments = items.Select(item =>
        {
            var url = WebUtility.HtmlEncode(item.Slug);
            var text = WebUtility.HtmlEncode(item.Title);
            return asList
                ? $"<li class=\"breadcrumb-item\"><a href=\"{url}\">{text}</a></li>"
                : $"<a href=\"{url}\">{text}</a>";
        });

        return asList ? string.Concat(segments) : string.Join(" > ", segments);
    }

    protected static string FormatDate(DateTimeOffset value, string format)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    protected static string FormatDuration(DateTimeOffset start, DateTimeOffset end)
    {
        var sameDate = start.Date == end.Date;
        return sameDate
            ? $"{start:yyyy-MM-dd(ddd) HH:mm} ~ {end:HH:mm}"
            : $"{start:yyyy-MM-dd(ddd) HH:mm} ~ {end:yyyy-MM-dd(ddd) HH:mm}";
    }

    protected static string FixSlug(string value)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
            Regex.Replace(value ?? string.Empty, "[-_]", " ").Trim());
    }

    protected static string ConvertUrlsToLinks(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Regex.Replace(
            text,
            "(?<![\\\"'])((https)://[^\\s/$.].([\\w./])*\\??[\\w&=\\-]*)",
            match =>
            {
                var url = WebUtility.HtmlEncode(match.Groups[1].Value);
                return $"<a href=\"{url}\">{url}</a>";
            });
    }

    protected static string JoinValues(IEnumerable<string> values, string glue = ",")
    {
        return string.Join(glue, values);
    }

    protected sealed record BreadcrumbItem(string Slug, string Title);
}