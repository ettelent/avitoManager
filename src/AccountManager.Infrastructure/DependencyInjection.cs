using AccountManager.Application.Abstractions;
using AccountManager.Infrastructure.Persistence;
using AccountManager.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountManagerInfrastructure(this IServiceCollection services, string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "account-manager.db");
        services.AddPooledDbContextFactory<AccountManagerDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        services.AddSingleton<IAccountRepository, SqliteAccountRepository>();
        services.AddSingleton<IProfileStorage>(new FileProfileStorage(Path.Combine(dataDirectory, "profiles")));
#pragma warning disable CA1416 // Account Manager is a Windows desktop application; DPAPI is intentional.
        services.AddSingleton<ISecretProtector, WindowsDpapiSecretProtector>();
#pragma warning restore CA1416
        services.AddSingleton<IProxyProvider, ProxyProvider>();
        return services;
    }
}
