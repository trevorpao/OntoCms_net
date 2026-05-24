using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseReactionController : ControllerBase
{
    public const int DoneCode = 1;
    public const int MissingColumnsCode = 8004;
    public const int WrongDataCode = 8204;
    public const int UnverifiedCode = 8205;

    protected async Task<IActionResult> ExecuteReactionAsync(
        Func<CancellationToken, Task<IActionResult>> action,
        CancellationToken cancellationToken = default)
    {
        await BeforeReactionAsync(cancellationToken);

        try
        {
            return await action(cancellationToken);
        }
        finally
        {
            await AfterReactionAsync(cancellationToken);
        }
    }

    protected virtual Task BeforeReactionAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterReactionAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task<TPayload> BeforeSaveAsync<TPayload>(
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(payload);
    }

    protected virtual TRow HandleRow<TRow>(TRow row)
    {
        return row;
    }

    protected virtual TRow HandleIteratee<TRow>(TRow row)
    {
        return row;
    }

    protected static string NormalizeQuery(string? query)
    {
        return query?.Trim() ?? string.Empty;
    }

    protected static bool TryGetPositiveId(int id, out int normalizedId)
    {
        normalizedId = id;
        return normalizedId > 0;
    }

    protected string ResolveReactionLanguage(
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

    protected void PersistReactionLanguageCookie(
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

    protected OkObjectResult OkDone(object? data = null, string csrf = "")
    {
        return Ok(Envelope(DoneCode, data, csrf));
    }

    protected OkObjectResult OkMissing(string csrf = "")
    {
        return Ok(Envelope(MissingColumnsCode, Array.Empty<object>(), csrf));
    }

    protected OkObjectResult OkWrongData(object? data = null, string csrf = "")
    {
        return Ok(Envelope(WrongDataCode, data ?? Array.Empty<object>(), csrf));
    }

    protected OkObjectResult OkUnverified(object? data = null, string csrf = "")
    {
        return Ok(Envelope(UnverifiedCode, data ?? Array.Empty<object>(), csrf));
    }

    public static ApiEnvelope Envelope(int code, object? data = null, string csrf = "")
    {
        return new ApiEnvelope(code, data, csrf);
    }

    public sealed record ApiEnvelope(int Code, object? Data, string Csrf);
}