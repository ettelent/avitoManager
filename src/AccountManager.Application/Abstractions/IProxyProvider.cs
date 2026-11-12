using AccountManager.Core.Domain;

namespace AccountManager.Application.Abstractions;

// Этап 1: контракт оставлен для подключения менеджера прокси без протекания его деталей в UI.
public interface IProxyProvider
{
    Task SetAsync(Account account, ProxyConfiguration proxy, CancellationToken cancellationToken = default);
    Task ClearAsync(Account account, CancellationToken cancellationToken = default);
    ProxyConfiguration? Get(Account account);
}
