using AccountManager.Core.Domain;
using Xunit;

namespace AccountManager.UnitTests;

public sealed class AccountTests
{
    [Fact]
    public void New_account_has_unique_id_and_stopped_status()
    {
        var account = new Account("Профиль 1");
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(BrowserStatus.Stopped, account.Status);
    }

    [Fact]
    public void Account_lifecycle_updates_status_and_launch_time()
    {
        var account = new Account("Профиль 1");
        account.MarkStarting();
        account.MarkRunning();
        Assert.Equal(BrowserStatus.Running, account.Status);
        Assert.NotNull(account.LastLaunchedAtUtc);
        account.MarkStopping();
        account.MarkStopped();
        Assert.Equal(BrowserStatus.Stopped, account.Status);
    }

    [Fact]
    public void Empty_name_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new Account("  "));
    }
}
