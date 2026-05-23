using System.Text;

namespace OntoCms.Conventions.HMVC;

public abstract class BaseKit
{
    public const string DefaultRuleGroupName = "default";
    public const string DeleteRuleGroupName = "del";
    public const string UploadRuleGroupName = "upload";
    public const string UploadFileRuleGroupName = "uploadFile";

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> baseRuleGroups =
        new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>(StringComparer.OrdinalIgnoreCase)
        {
            [DefaultRuleGroupName] = EmptyRuleGroup,
            [DeleteRuleGroupName] = CreateRuleGroup(("pid", ["required", "integer"])),
            [UploadRuleGroupName] = CreateRuleGroup(("photo", ["required", "uploaded_file", "max:5M", "mimes:jpeg,png"])),
            [UploadFileRuleGroupName] = CreateRuleGroup(("file", ["required", "uploaded_file", "max:10M", "mimes:pdf,zip,xls,xlsx"])),
        };

    protected static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyRuleGroup =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

    protected static IReadOnlyDictionary<string, IReadOnlyList<string>> ResolveRuleGroup(
        string? group,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>? entityRuleGroups = null)
    {
        var groupName = string.IsNullOrWhiteSpace(group) ? DefaultRuleGroupName : group.Trim();

        if (entityRuleGroups is not null && entityRuleGroups.TryGetValue(groupName, out var entityRuleGroup))
        {
            return entityRuleGroup;
        }

        if (baseRuleGroups.TryGetValue(groupName, out var baseRuleGroup))
        {
            return baseRuleGroup;
        }

        return EmptyRuleGroup;
    }

    protected static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> CreateRuleGroups(
        params (string Group, IReadOnlyDictionary<string, IReadOnlyList<string>> Rules)[] groups)
    {
        var dictionary = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (group, rules) in groups)
        {
            dictionary[group] = rules;
        }

        return dictionary;
    }

    protected static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateRuleGroup(
        params (string Field, IReadOnlyList<string> Rules)[] rules)
    {
        var dictionary = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (field, entries) in rules)
        {
            dictionary[field] = entries;
        }

        return dictionary;
    }

    public static int GetDisplayWidth(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        var width = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            width += rune.Utf8SequenceLength > 1 ? 2 : 1;
        }

        return width;
    }
}