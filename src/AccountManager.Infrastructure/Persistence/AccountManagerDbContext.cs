using AccountManager.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AccountManager.Infrastructure.Persistence;

public sealed class AccountManagerDbContext(DbContextOptions<AccountManagerDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Account>();
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(2000);
        entity.Property(x => x.ProfilePath).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.ProxyType).HasConversion<string>().HasMaxLength(16);
        entity.Property(x => x.ProxyServer).HasMaxLength(500);
        entity.Property(x => x.ProxyUsername).HasMaxLength(200);
        entity.Property(x => x.ProxyPasswordProtected).HasMaxLength(4000);
        entity.HasIndex(x => x.Name).IsUnique();
    }
}
