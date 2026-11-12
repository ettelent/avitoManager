namespace AccountManager.Core.Domain;

public enum BrowserStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Faulted
}

public enum ProxyType
{
    Http,
    Https,
    Socks5
}
