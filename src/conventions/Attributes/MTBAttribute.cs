namespace OntoCms.Conventions.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MTBAttribute : Attribute
{
    public MTBAttribute(string tableBaseName)
    {
        TableBaseName = tableBaseName;
    }

    public string TableBaseName { get; }
}