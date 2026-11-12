namespace AccountManager.Core.Domain;

public sealed class Account
{
    private Account() { }

    public Account(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя профиля не может быть пустым.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
        Status = BrowserStatus.Stopped;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ProfilePath { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? LastLaunchedAtUtc { get; private set; }
    public BrowserStatus Status { get; private set; }
    public ProxyType? ProxyType { get; private set; }
    public string? ProxyServer { get; private set; }
    public string? ProxyUsername { get; private set; }
    public string? ProxyPasswordProtected { get; private set; }

    public void SetProfilePath(string path) => ProfilePath = path;
    public void SetProxy(ProxyType type, string server, string? username, string? protectedPassword)
    {
        if (string.IsNullOrWhiteSpace(server)) throw new ArgumentException("Адрес прокси не может быть пустым.", nameof(server));
        ProxyType = type;
        ProxyServer = server.Trim();
        ProxyUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();
        ProxyPasswordProtected = protectedPassword;
    }

    public void ClearProxy()
    {
        ProxyType = null;
        ProxyServer = null;
        ProxyUsername = null;
        ProxyPasswordProtected = null;
    }
    public void MarkStarting() => Status = BrowserStatus.Starting;
    public void MarkRunning()
    {
        Status = BrowserStatus.Running;
        LastLaunchedAtUtc = DateTimeOffset.UtcNow;
    }
    public void MarkStopping() => Status = BrowserStatus.Stopping;
    public void MarkStopped() => Status = BrowserStatus.Stopped;
    public void MarkFaulted() => Status = BrowserStatus.Faulted;
}
