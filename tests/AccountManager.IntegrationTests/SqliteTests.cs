using AccountManager.Core.Domain;
using AccountManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountManager.IntegrationTests;

public sealed class SqliteTests
{
    [Fact]
    public async Task Account_can_be_saved_to_sqlite_and_read_back()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AccountManagerDbContext>().UseSqlite(connection).Options;
        await using (var db = new AccountManagerDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Accounts.Add(new Account("SQLite test"));
            await db.SaveChangesAsync();
        }
        await using (var db = new AccountManagerDbContext(options))
            Assert.Single(await db.Accounts.ToListAsync());
    }
}
