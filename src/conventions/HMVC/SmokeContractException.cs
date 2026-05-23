namespace OntoCms.Conventions.HMVC;

public sealed class SmokeContractException : InvalidOperationException
{
    public SmokeContractException(string errorKey, string message)
        : base(message)
    {
        ErrorKey = errorKey;
    }

    public string ErrorKey { get; }
}