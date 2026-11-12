using System.Collections.ObjectModel;
using AccountManager.Application;
using AccountManager.Application.Abstractions;
using AccountManager.Core.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountManager.Desktop;

public sealed partial class MainViewModel(AccountManagerService accountService, IProxyProvider proxyProvider) : ObservableObject
{
    public ObservableCollection<Account> Accounts { get; } = [];

    [ObservableProperty]
    private Account? selectedAccount;

    [ObservableProperty]
    private string statusMessage = "Готово. Создайте профиль или выберите существующий.";

    public async Task InitializeAsync()
    {
        await accountService.ReconcileStatusesAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var accounts = await accountService.ListAsync();
            Accounts.Clear();
            foreach (var account in accounts) Accounts.Add(account);
            StatusMessage = $"Профилей: {Accounts.Count}.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка загрузки: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        var dialog = new CreateProfileDialog { Owner = System.Windows.Application.Current.MainWindow };
        if (dialog.ShowDialog() != true) return;
        try
        {
            var account = await accountService.CreateAsync(dialog.ProfileName, dialog.Description);
            await RefreshAsync();
            SelectedAccount = Accounts.FirstOrDefault(x => x.Id == account.Id);
            StatusMessage = $"Профиль «{account.Name}» создан.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка создания: {ex.Message}"; }
    }

    [RelayCommand(CanExecute = nameof(CanOperate))]
    private async Task LaunchAsync()
    {
        var account = SelectedAccount;
        if (account is null) return;
        try
        {
            var selectedId = account.Id;
            await accountService.LaunchAsync(account);
            await RefreshAsync();
            SelectedAccount = Accounts.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = $"Профиль «{SelectedAccount?.Name}» запущен. Сессия сохраняется в его папке.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка запуска: {ex.Message}"; }
    }

    [RelayCommand(CanExecute = nameof(CanOperate))]
    private async Task CloseAsync()
    {
        if (SelectedAccount is null) return;
        try
        {
            var selectedId = SelectedAccount.Id;
            await accountService.CloseAsync(SelectedAccount);
            await RefreshAsync();
            SelectedAccount = Accounts.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = $"Профиль «{SelectedAccount?.Name}» закрыт.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка закрытия: {ex.Message}"; }
    }

    [RelayCommand(CanExecute = nameof(CanOperate))]
    private async Task ConfigureProxyAsync()
    {
        if (SelectedAccount is null) return;
        var dialog = new ProxyDialog(SelectedAccount) { Owner = System.Windows.Application.Current.MainWindow };
        if (dialog.ShowDialog() != true) return;
        try
        {
            if (dialog.ClearProxy)
                await proxyProvider.ClearAsync(SelectedAccount);
            else
                await proxyProvider.SetAsync(SelectedAccount, new ProxyConfiguration(
                    dialog.ProxyType, dialog.Server, dialog.Username, dialog.Password));
            await RefreshAsync();
            SelectedAccount = Accounts.FirstOrDefault(x => x.Id == dialog.AccountId);
            StatusMessage = dialog.ClearProxy ? "Прокси отключён." : "Прокси сохранён и будет применён при следующем запуске.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка настройки прокси: {ex.Message}"; }
    }

    partial void OnSelectedAccountChanged(Account? value)
    {
        LaunchCommand.NotifyCanExecuteChanged();
        CloseCommand.NotifyCanExecuteChanged();
        ConfigureProxyCommand.NotifyCanExecuteChanged();
    }

    private bool CanOperate() => SelectedAccount is not null;
}
