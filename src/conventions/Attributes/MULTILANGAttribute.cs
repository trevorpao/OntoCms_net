namespace OntoCms.Conventions.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MULTILANGAttribute : Attribute
{
    public MULTILANGAttribute(bool enabled)
    {
        Enabled = enabled;
    }

    public bool Enabled { get; }
}