using AccountManager.Core.Domain;

namespace AccountManager.Application.Abstractions;

public interface IBrowserAgent : IAsyncDisposable
{
    Task LaunchAsync(Guid accountId, string profilePath, ProxyConfiguration? proxy = null, CancellationToken cancellationToken = default);
    Task CloseAsync(Guid accountId, CancellationToken cancellationToken = default);
    bool IsRunning(Guid accountId);
}

public sealed record ProxyConfiguration(ProxyType Type, string Server, string? Username, string? Password);
