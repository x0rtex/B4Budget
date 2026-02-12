using B4Budget.Data;
using B4Budget.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B4Budget;

public static class MauiProgram
{
    private const string DbFileName = "b4budget.db";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Services
        builder.Services.AddScoped<IBudgetService, BudgetService>();
        builder.Services.AddScoped<ICalculationService, CalculationService>();

        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        #endif

        // SQLite database path in AppData
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
        builder.Services.AddDbContext<BudgetDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        var app = builder.Build();

        // Ensure database exists on first launch
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
        db.Database.EnsureCreated();

        return app;
    }
}