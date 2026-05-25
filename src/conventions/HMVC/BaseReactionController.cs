using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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

    protected async Task<IActionResult> ReactGetAsync<TRow>(
        int id,
        Func<int, CancellationToken, Task<TRow?>> loadRowAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loadRowAsync);

        return await ExecuteReactionAsync(async token =>
        {
            if (!TryGetPositiveId(id, out var normalizedId))
            {
                return OkMissing();
            }

            var row = await loadRowAsync(normalizedId, token);
            if (row is null)
            {
                return OkMissing();
            }

            return OkDone(HandleRow(row));
        }, cancellationToken);
    }

    protected Task<IActionResult> ReactGetAsync<TRow>(
        int id,
        IReactionGetFeed<TRow> feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactGetAsync(id, feed.GetAsync, cancellationToken);
    }

    protected async Task<IActionResult> ReactDeleteAsync(
        int id,
        Func<int, CancellationToken, Task<bool>> deleteAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deleteAsync);

        return await ExecuteReactionAsync(async token =>
        {
            if (!TryGetPositiveId(id, out var normalizedId))
            {
                return OkMissing();
            }

            var deleted = await deleteAsync(normalizedId, token);
            if (!deleted)
            {
                return OkMissing();
            }

            return OkDone(new { id = normalizedId });
        }, cancellationToken);
    }

    protected Task<IActionResult> ReactDeleteAsync(
        int id,
        IReactionDeleteFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactDeleteAsync(id, feed.DeleteAsync, cancellationToken);
    }

    protected async Task<IActionResult> ReactSaveAsync<TPayload>(
        TPayload payload,
        Func<TPayload, CancellationToken, Task<int>> saveAsync,
        CancellationToken cancellationToken = default)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(saveAsync);

        return await ExecuteReactionAsync(async token =>
        {
            var normalizedPayload = await BeforeSaveAsync(payload, token);
            var rowId = await saveAsync(normalizedPayload, token);
            return OkDone(new { id = rowId });
        }, cancellationToken);
    }

    protected async Task<IActionResult> ReactSaveAsync<TPayload>(
        TPayload payload,
        Func<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> resolveRuleGroup,
        Func<TPayload, CancellationToken, Task<int>> saveAsync,
        CancellationToken cancellationToken = default,
        string ruleGroup = BaseKit.SaveRuleGroupName)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(resolveRuleGroup);
        ArgumentNullException.ThrowIfNull(saveAsync);

        return await ExecuteReactionAsync(async token =>
        {
            var normalizedPayload = await BeforeSaveAsync(payload, token);
            var validationFailure = ValidateRuleGroup(normalizedPayload, resolveRuleGroup(ruleGroup));
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var rowId = await saveAsync(normalizedPayload, token);
            return OkDone(new { id = rowId });
        }, cancellationToken);
    }

    protected Task<IActionResult> ReactSaveAsync<TPayload>(
        TPayload payload,
        IFeedRepository<TPayload> feed,
        CancellationToken cancellationToken = default)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactSaveAsync(payload, feed.SaveAsync, cancellationToken);
    }

    protected Task<IActionResult> ReactSaveAsync<TPayload>(
        TPayload payload,
        Func<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> resolveRuleGroup,
        IFeedRepository<TPayload> feed,
        CancellationToken cancellationToken = default,
        string ruleGroup = BaseKit.SaveRuleGroupName)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactSaveAsync(payload, resolveRuleGroup, feed.SaveAsync, cancellationToken, ruleGroup);
    }

    protected async Task<IActionResult> ReactListAsync<TRow>(
        string? query,
        int? page,
        int? limit,
        Func<string, int, int, CancellationToken, Task<FeedPageResult<TRow>>> loadRowsAsync,
        CancellationToken cancellationToken = default,
        int defaultLimit = 24,
        int minLimit = 12,
        int? maxLimit = null)
    {
        ArgumentNullException.ThrowIfNull(loadRowsAsync);

        return await ExecuteReactionAsync(async token =>
        {
            var normalizedQuery = NormalizeQuery(query);
            var pageIndex = NormalizePageIndex(page);
            var normalizedLimit = NormalizeReactionLimit(limit, defaultLimit, minLimit, maxLimit);
            var pageResult = await loadRowsAsync(normalizedQuery, pageIndex, normalizedLimit, token);
            var subset = pageResult.Subset.Select(HandleIteratee).ToArray();

            return OkDone(new
            {
                subset,
                limit = pageResult.Limit,
                pos = pageResult.Pos,
                total = pageResult.Total,
                count = pageResult.Count,
                sql = string.Empty,
            });
        }, cancellationToken);
    }

    protected Task<IActionResult> ReactListAsync<TRow>(
        string? query,
        int? page,
        int? limit,
        IReactionListFeed<TRow> feed,
        CancellationToken cancellationToken = default,
        int defaultLimit = 24,
        int minLimit = 12,
        int? maxLimit = null)
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactListAsync(
            query,
            page,
            limit,
            feed.LimitRowsAsync,
            cancellationToken,
            defaultLimit,
            minLimit,
            maxLimit);
    }

    protected async Task<IActionResult> ReactGetOptionsAsync<TOption>(
        string? query,
        Func<string, CancellationToken, Task<IReadOnlyList<TOption>>> loadOptionsAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loadOptionsAsync);

        return await ExecuteReactionAsync(async token =>
        {
            var normalizedQuery = NormalizeQuery(query);
            var rows = await loadOptionsAsync(normalizedQuery, token);
            return OkDone(rows);
        }, cancellationToken);
    }

    protected Task<IActionResult> ReactGetOptionsAsync<TOption>(
        string? query,
        IReactionOptionsFeed<TOption> feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        return ReactGetOptionsAsync(query, feed.GetOptionsAsync, cancellationToken);
    }

    protected static string NormalizeQuery(string? query)
    {
        return query?.Trim() ?? string.Empty;
    }

    protected static int NormalizePageIndex(int? page)
    {
        if (!page.HasValue || page.Value <= 1)
        {
            return 0;
        }

        return page.Value - 1;
    }

    protected static int NormalizeReactionLimit(
        int? limit,
        int defaultLimit = 24,
        int minLimit = 12,
        int? maxLimit = null)
    {
        if (defaultLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultLimit), "Default limit must be positive.");
        }

        if (minLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minLimit), "Minimum limit must be positive.");
        }

        var ceiling = Math.Max(maxLimit ?? defaultLimit, minLimit);
        if (!limit.HasValue || limit.Value <= 0)
        {
            return ceiling;
        }

        return Math.Clamp(limit.Value, minLimit, ceiling);
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

    protected OkObjectResult? ValidateRuleGroup<TPayload>(
        TPayload payload,
        IReadOnlyDictionary<string, IReadOnlyList<string>> rules)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(rules);

        if (rules.Count == 0)
        {
            return null;
        }

        var payloadFields = ExtractPayloadFields(payload);
        var missingFields = new List<string>();
        var wrongDataFields = new List<string>();
        var unsupportedRules = new List<string>();

        foreach (var ruleEntry in rules)
        {
            payloadFields.TryGetValue(ruleEntry.Key, out var value);
            foreach (var rule in ruleEntry.Value)
            {
                switch (rule.Trim())
                {
                    case "required":
                        if (IsMissingRequiredValue(value))
                        {
                            missingFields.Add(ruleEntry.Key);
                        }

                        break;

                    case "integer":
                        if (!IsMissingRequiredValue(value) && !IsIntegerValue(value))
                        {
                            wrongDataFields.Add(ruleEntry.Key);
                        }

                        break;

                    default:
                        unsupportedRules.Add($"{ruleEntry.Key}:{rule}");
                        break;
                }
            }
        }

        if (unsupportedRules.Count > 0)
        {
            return OkUnverified(new { unsupported_rules = unsupportedRules.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() });
        }

        if (missingFields.Count > 0)
        {
            return OkMissingData(missingFields);
        }

        if (wrongDataFields.Count > 0)
        {
            return OkWrongData(new { fields = wrongDataFields.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() });
        }

        return null;
    }

    protected OkObjectResult OkMissingData(IReadOnlyCollection<string> fields, string csrf = "")
    {
        return Ok(Envelope(MissingColumnsCode, new { fields = fields }, csrf));
    }

    private static IReadOnlyDictionary<string, object?> ExtractPayloadFields<TPayload>(TPayload payload)
        where TPayload : class
    {
        return payload
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.CanRead)
            .ToDictionary(
                property => property.Name,
                property => property.GetValue(payload),
                StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsMissingRequiredValue(object? value)
    {
        return value switch
        {
            null => true,
            string stringValue => string.IsNullOrWhiteSpace(stringValue),
            _ => false,
        };
    }

    private static bool IsIntegerValue(object? value)
    {
        return value switch
        {
            sbyte or byte or short or ushort or int or uint or long or ulong => true,
            string stringValue => int.TryParse(stringValue.Trim(), out _),
            _ => false,
        };
    }

    public static ApiEnvelope Envelope(int code, object? data = null, string csrf = "")
    {
        return new ApiEnvelope(code, data, csrf);
    }

    public sealed record ApiEnvelope(int Code, object? Data, string Csrf);
}