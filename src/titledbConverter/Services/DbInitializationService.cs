using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using titledbConverter.Data;
using titledbConverter.Services.Interface;

namespace titledbConverter.Services;

public class DbInitializationService : IDbInitializationService
{
    private readonly SqliteDbContext _context;
    private readonly ILogger<DbInitializationService> _logger;

    public DbInitializationService(
        SqliteDbContext context,
        ILogger<DbInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(bool dropDatabase)
    {
        try
        {
            if (dropDatabase)
            {
                AnsiConsole.MarkupLineInterpolated($"[bold yellow]Delete Database[/]");

                await _context.Database.EnsureDeletedAsync();
            }

            // Check and apply any pending migrations
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[bold yellow]Applying pending migrations...[/]");
                await _context.Database.MigrateAsync();
            }
            AnsiConsole.MarkupLineInterpolated($"[bold green]Database initialization completed successfully[/]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}