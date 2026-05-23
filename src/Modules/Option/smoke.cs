using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Option;

public sealed class OptionSmoke : BaseSmoke
{
	protected override IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MethodMap { get; } =
		CreateMethodMap();
}