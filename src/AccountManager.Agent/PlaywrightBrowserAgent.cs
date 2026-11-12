using AccountManager.Application.Abstractions;
using AccountManager.Core.Domain;
using Microsoft.Playwright;

namespace AccountManager.Agent;

public sealed class PlaywrightBrowserAgent : IBrowserAgent
{
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private readonly Dictionary<Guid, IBrowserContext> contexts = [];
    private IPlaywright? playwright;
    private bool disposed;

    public bool IsRunning(Guid accountId)
    {
        IBrowserContext? context;
        lock (contexts)
        {
            contexts.TryGetValue(accountId, out context);
        }

        if (context is null) return false;

        try
        {
            // После ручного закрытия окна Chromium Playwright может ещё хранить
            // объект context, но у него уже не останется страниц.
            return context.Pages.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task LaunchAsync(Guid accountId, string profilePath, ProxyConfiguration? proxy = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(profilePath);
        Directory.CreateDirectory(profilePath);

        IBrowserContext? existing;
        lock (contexts) contexts.TryGetValue(accountId, out existing);
        if (existing is not null && IsRunning(accountId)) return;
        if (existing is not null)
        {
            try { await existing.CloseAsync(); } catch { /* Chromium уже завершился. */ }
            lock (contexts)
            {
                if (contexts.TryGetValue(accountId, out var current) && ReferenceEquals(current, existing))
                    contexts.Remove(accountId);
            }
        }

        await initializationLock.WaitAsync(cancellationToken);
        try
        {
            lock (contexts) contexts.TryGetValue(accountId, out existing);
            if (existing is not null && IsRunning(accountId)) return;
            if (existing is not null)
            {
                try { await existing.CloseAsync(); } catch { /* Chromium уже завершился. */ }
                lock (contexts)
                {
                    if (contexts.TryGetValue(accountId, out var current) && ReferenceEquals(current, existing))
                        contexts.Remove(accountId);
                }
            }

            playwright ??= await Playwright.CreateAsync();
            var context = await playwright.Chromium.LaunchPersistentContextAsync(
                profilePath,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = false,
                    Proxy = proxy is null ? null : new Proxy
                    {
                        Server = proxy.Server,
                        Username = proxy.Username,
                        Password = proxy.Password
                    },
                    // HTTP-прокси работает поверх TCP CONNECT; QUIC/HTTP3 использует UDP
                    // и может вызывать бесконечную загрузку отдельных сайтов.
                    Args = ["--no-first-run", "--no-default-browser-check", "--disable-quic"]
                });

            await context.NewPageAsync();
            lock (contexts) contexts.Add(accountId, context);
        }
        finally
        {
            initializationLock.Release();
        }
    }

    public async Task CloseAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        IBrowserContext? context;
        lock (contexts)
        {
            contexts.Remove(accountId, out context);
        }

        if (context is not null)
            await context.CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;

        IBrowserContext[] active;
        lock (contexts)
        {
            active = contexts.Values.ToArray();
            contexts.Clear();
        }

        foreach (var context in active)
            await context.CloseAsync();

        playwright?.Dispose();
        initializationLock.Dispose();
    }
}
