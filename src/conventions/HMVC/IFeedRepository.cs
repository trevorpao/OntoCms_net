namespace OntoCms.Conventions.HMVC;

public interface IFeedRepository<in TPayload>
    where TPayload : class
{
    Task<int> SaveAsync(TPayload payload, CancellationToken cancellationToken = default);
}