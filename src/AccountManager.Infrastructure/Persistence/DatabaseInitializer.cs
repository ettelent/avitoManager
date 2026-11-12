using Microsoft.EntityFrameworkCore;

namespace AccountManager.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AccountManagerDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(Accounts);";
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken)) columns.Add(reader.GetString(1));
        }

        var additions = new Dictionary<string, string>
        {
            ["ProxyType"] = "TEXT NULL",
            ["ProxyServer"] = "TEXT NULL",
            ["ProxyUsername"] = "TEXT NULL",
            ["ProxyPasswordProtected"] = "TEXT NULL"
        };
        foreach (var addition in additions)
        {
            if (columns.Contains(addition.Key)) continue;
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE Accounts ADD COLUMN {addition.Key} {addition.Value};";
            await alter.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
