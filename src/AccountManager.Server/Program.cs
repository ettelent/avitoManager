using AccountManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var dataDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "AccountManager");
builder.Services.AddAccountManagerInfrastructure(dataDirectory);

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok", stage = 1 }));
app.Run();
