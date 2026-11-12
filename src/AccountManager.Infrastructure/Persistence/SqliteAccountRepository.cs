using AccountManager.Application.Abstractions;
using AccountManager.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AccountManager.Infrastructure.Persistence;

public sealed class SqliteAccountRepository(IDbContextFactory<AccountManagerDbContext> contextFactory) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Accounts.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Accounts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Accounts.Update(account);
        await db.SaveChangesAsync(cancellationToken);
    }
}
