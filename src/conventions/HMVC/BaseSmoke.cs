namespace OntoCms.Conventions.HMVC;

public abstract class BaseSmoke
{
    protected abstract IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MethodMap { get; }

    public object? Run(
        string surface,
        string contract,
        IReadOnlyDictionary<string, object?>? context = null)
    {
        if (!MethodMap.TryGetValue(surface, out var contracts))
        {
            throw new SmokeContractException(
                "surface_not_found",
                $"Smoke surface '{surface}' not found.");
        }

        if (!contracts.TryGetValue(contract, out var methodName))
        {
            throw new SmokeContractException(
                "contract_not_found",
                $"Smoke contract '{contract}' not found for surface '{surface}'.");
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new SmokeContractException(
                "invalid_smoke_contract",
                "Smoke method mapping is invalid.");
        }

        var method = GetType().GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

        if (method is null)
        {
            throw new SmokeContractException(
                "invalid_smoke_contract",
                "Smoke method mapping is invalid.");
        }

        return method.Invoke(this, [context ?? EmptyContext]);
    }

    protected static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> CreateMethodMap(
        params (string Surface, IReadOnlyDictionary<string, string> Contracts)[] surfaces)
    {
        var methodMap = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (surface, contracts) in surfaces)
        {
            methodMap[surface] = contracts;
        }

        return methodMap;
    }

    protected static IReadOnlyDictionary<string, string> CreateContractMap(
        params (string Contract, string MethodName)[] contracts)
    {
        var contractMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (contract, methodName) in contracts)
        {
            contractMap[contract] = methodName;
        }

        return contractMap;
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyContext =
        new Dictionary<string, object?>();
}