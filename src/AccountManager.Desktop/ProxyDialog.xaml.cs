using System.Windows;
using AccountManager.Core.Domain;

namespace AccountManager.Desktop;

public partial class ProxyDialog : Window
{
    public ProxyDialog(Account account)
    {
        AccountId = account.Id;
        InitializeComponent();
        TypeBox.ItemsSource = Enum.GetValues<ProxyType>();
        TypeBox.SelectedItem = account.ProxyType ?? ProxyType.Http;
        ServerBox.Text = account.ProxyServer ?? string.Empty;
        UsernameBox.Text = account.ProxyUsername ?? string.Empty;
    }

    public Guid AccountId { get; }
    public ProxyType ProxyType => (ProxyType)TypeBox.SelectedItem!;
    public string Server { get; private set; } = string.Empty;
    public string? Username { get; private set; }
    public string? Password { get; private set; }
    public bool ClearProxy { get; private set; }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ServerBox.Text))
        {
            MessageBox.Show(this, "Укажите адрес прокси.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Server = ServerBox.Text.Trim();
        Username = string.IsNullOrWhiteSpace(UsernameBox.Text) ? null : UsernameBox.Text.Trim();
        Password = string.IsNullOrEmpty(PasswordBox.Password) ? null : PasswordBox.Password;
        DialogResult = true;
    }

    private void ClearClick(object sender, RoutedEventArgs e)
    {
        ClearProxy = true;
        DialogResult = true;
    }
}
