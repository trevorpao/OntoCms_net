using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Option;

public sealed class OptionKit : BaseKit
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> ruleGroups =
        CreateRuleGroups();

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Rule(string group = DefaultRuleGroupName)
    {
        return ResolveRuleGroup(group, ruleGroups);
    }
}