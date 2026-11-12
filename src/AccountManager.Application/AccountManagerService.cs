using AccountManager.Application.Abstractions;
using AccountManager.Core.Domain;

namespace AccountManager.Application;

public sealed class AccountManagerService(
    IAccountRepository repository,
    IProfileStorage profileStorage,
    IBrowserAgent browserAgent,
    IProxyProvider proxyProvider)
{
    public async Task ReconcileStatusesAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await repository.ListAsync(cancellationToken);
        foreach (var account in accounts)
        {
            if (account.Status == BrowserStatus.Stopped || browserAgent.IsRunning(account.Id)) continue;
            account.MarkStopped();
            await repository.SaveAsync(account, cancellationToken);
        }
    }

    public Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default) =>
        repository.ListAsync(cancellationToken);

    public async Task<Account> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var account = new Account(name, description);
        var path = profileStorage.GetPath(account.Id);
        account.SetProfilePath(path);
        await profileStorage.EnsureCreatedAsync(account.Id, cancellationToken);
        await repository.AddAsync(account, cancellationToken);
        return account;
    }

    public async Task LaunchAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (browserAgent.IsRunning(account.Id)) return;
        account.MarkStarting();
        await repository.SaveAsync(account, cancellationToken);
        try
        {
            await browserAgent.LaunchAsync(account.Id, account.ProfilePath, proxyProvider.Get(account), cancellationToken);
            account.MarkRunning();
            await repository.SaveAsync(account, cancellationToken);
        }
        catch
        {
            account.MarkFaulted();
            await repository.SaveAsync(account, cancellationToken);
            throw;
        }
    }

    public async Task CloseAsync(Account account, CancellationToken cancellationToken = default)
    {
        account.MarkStopping();
        await repository.SaveAsync(account, cancellationToken);
        if (browserAgent.IsRunning(account.Id))
            await browserAgent.CloseAsync(account.Id, cancellationToken);
        account.MarkStopped();
        await repository.SaveAsync(account, cancellationToken);
    }
}
