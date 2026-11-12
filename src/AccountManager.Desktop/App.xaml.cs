using System.IO;
using System.Windows;
using AccountManager.Agent;
using AccountManager.Application;
using AccountManager.Infrastructure;
using AccountManager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AccountManager.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AccountManager");
        var collection = new ServiceCollection();
        collection.AddAccountManagerInfrastructure(dataDirectory);
        collection.AddSingleton<PlaywrightBrowserAgent>();
        collection.AddSingleton<AccountManager.Application.Abstractions.IBrowserAgent>(sp => sp.GetRequiredService<PlaywrightBrowserAgent>());
        collection.AddSingleton<AccountManagerService>();
        collection.AddSingleton<MainViewModel>();
        services = collection.BuildServiceProvider();

        var dbFactory = services.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<AccountManager.Infrastructure.Persistence.AccountManagerDbContext>>();
        await using (var db = await dbFactory.CreateDbContextAsync()) await DatabaseInitializer.InitializeAsync(db);
        var viewModel = services.GetRequiredService<MainViewModel>();
        await viewModel.InitializeAsync();
        MainWindow = new MainWindow(viewModel);
        MainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (services is not null)
        {
            var agent = services.GetService<PlaywrightBrowserAgent>();
            if (agent is not null) await agent.DisposeAsync();
            await services.DisposeAsync();
        }
        base.OnExit(e);
    }
}
