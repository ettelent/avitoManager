using AccountManager.Application.Abstractions;

namespace AccountManager.Infrastructure.Persistence;

public sealed class FileProfileStorage(string rootPath) : IProfileStorage
{
    public string GetPath(Guid accountId) => Path.Combine(rootPath, accountId.ToString("N"));

    public Task EnsureCreatedAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(GetPath(accountId));
        return Task.CompletedTask;
    }
}
