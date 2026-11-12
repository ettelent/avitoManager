using AccountManager.Application.Abstractions;
using AccountManager.Core.Domain;

namespace AccountManager.Infrastructure;

public sealed class ProxyProvider(IAccountRepository repository, ISecretProtector protector) : IProxyProvider
{
    public async Task SetAsync(Account account, ProxyConfiguration proxy, CancellationToken cancellationToken = default)
    {
        proxy = Normalize(proxy);
        Validate(proxy);
        // Пустое поле пароля в диалоге означает «оставить сохранённый пароль».
        // Новый пароль шифруется DPAPI текущей Windows-учётной записью.
        var protectedPassword = proxy.Password is null
            ? account.ProxyPasswordProtected
            : protector.Protect(proxy.Password);
        account.SetProxy(proxy.Type, proxy.Server, proxy.Username, protectedPassword);
        await repository.SaveAsync(account, cancellationToken);
    }

    public async Task ClearAsync(Account account, CancellationToken cancellationToken = default)
    {
        account.ClearProxy();
        await repository.SaveAsync(account, cancellationToken);
    }

    public ProxyConfiguration? Get(Account account)
    {
        if (account.ProxyType is null || string.IsNullOrWhiteSpace(account.ProxyServer)) return null;
        var server = account.ProxyServer.Trim();
        if (!server.Contains("://", StringComparison.Ordinal))
            server = $"{GetScheme(account.ProxyType.Value)}://{server}";
        else if (account.ProxyType == ProxyType.Https && server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            server = "http://" + server[8..];
        return new ProxyConfiguration(
            account.ProxyType.Value,
            server,
            account.ProxyUsername,
            protector.Unprotect(account.ProxyPasswordProtected));
    }

    private static void Validate(ProxyConfiguration proxy)
    {
        if (!Uri.TryCreate(proxy.Server, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
            throw new ArgumentException("Адрес прокси должен быть в формате http://host:port или socks5://host:port.", nameof(proxy));
        if (uri.Port is < 1 or > 65535) throw new ArgumentException("Порт прокси должен быть от 1 до 65535.", nameof(proxy));
    }

    private static ProxyConfiguration Normalize(ProxyConfiguration proxy)
    {
        var server = proxy.Server.Trim();
        if (!server.Contains("://", StringComparison.Ordinal))
            server = $"{GetScheme(proxy.Type)}://{server}";
        else if (proxy.Type == ProxyType.Https && server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            server = "http://" + server[8..];
        return proxy with { Server = server };
    }

    private static string GetScheme(ProxyType type) => type switch
    {
        ProxyType.Http => "http",
        // HTTPS-сайты открываются через обычный HTTP CONNECT-прокси.
        // Playwright документирует HTTP и SOCKS5 как типы proxy-сервера.
        ProxyType.Https => "http",
        ProxyType.Socks5 => "socks5",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
