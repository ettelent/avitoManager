using System.Windows;

namespace AccountManager.Desktop;

public partial class CreateProfileDialog : Window
{
    public CreateProfileDialog() => InitializeComponent();

    public string ProfileName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private void CreateClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(this, "Укажите название профиля.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }

        ProfileName = NameBox.Text.Trim();
        Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) || DescriptionBox.Text == "Описание (необязательно)"
            ? null : DescriptionBox.Text.Trim();
        DialogResult = true;
    }
}
