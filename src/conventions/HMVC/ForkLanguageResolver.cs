namespace OntoCms.Conventions.HMVC;

public static class ForkLanguageResolver
{
    public static readonly string[] DefaultLanguages = ["tw", "en"];

    public static string Resolve(
        string? routeLang,
        string? queryLang,
        string? cookieLang,
        IReadOnlyCollection<string>? supportedLanguages = null,
        string defaultLang = "tw")
    {
        var normalizedSupported = BuildSupportedLanguages(supportedLanguages, defaultLang);

        foreach (var candidate in new[]
        {
            Normalize(routeLang),
            Normalize(queryLang),
            Normalize(cookieLang),
        })
        {
            if (!string.IsNullOrWhiteSpace(candidate) && normalizedSupported.Contains(candidate))
            {
                return candidate;
            }
        }

        return Normalize(defaultLang) ?? "tw";
    }

    public static bool ShouldPersistCookie(
        string? routeLang,
        string? queryLang,
        IReadOnlyCollection<string>? supportedLanguages = null,
        string defaultLang = "tw")
    {
        var normalizedSupported = BuildSupportedLanguages(supportedLanguages, defaultLang);
        var routeCandidate = Normalize(routeLang);
        var queryCandidate = Normalize(queryLang);

        return (!string.IsNullOrWhiteSpace(routeCandidate) && normalizedSupported.Contains(routeCandidate))
            || (!string.IsNullOrWhiteSpace(queryCandidate) && normalizedSupported.Contains(queryCandidate));
    }

    public static string? Normalize(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return null;
        }

        var normalized = lang.Trim().ToLowerInvariant();
        return normalized switch
        {
            "en-us" or "en-gb" => "en",
            "zh" or "zh-tw" or "zh-hant" => "tw",
            _ => normalized,
        };
    }

    private static HashSet<string> BuildSupportedLanguages(IReadOnlyCollection<string>? supportedLanguages, string defaultLang)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var lang in supportedLanguages ?? DefaultLanguages)
        {
            var normalized = Normalize(lang);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                result.Add(normalized);
            }
        }

        var fallback = Normalize(defaultLang) ?? "tw";
        result.Add(fallback);
        return result;
    }
}