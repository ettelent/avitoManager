namespace AccountManager.Application.Abstractions;

public interface IProfileStorage
{
    string GetPath(Guid accountId);
    Task EnsureCreatedAsync(Guid accountId, CancellationToken cancellationToken = default);
}
