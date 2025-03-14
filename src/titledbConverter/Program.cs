using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Commands;
using titledbConverter.Data;
using titledbConverter.Infrastructure;
using titledbConverter.Services;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var host = CreateHostBuilder(args);
        var app = new CommandApp(new TypeRegistrar(host));
        app.Configure(c =>
        {
            c.SetExceptionHandler((exception, ctx) =>
            {
                if (exception is CommandParseException parseException)
                {
                    if (parseException.Message.Contains("Unknown command"))
                    {
                        AnsiConsole.MarkupLine($"[red]Error: Unknown command '{args.FirstOrDefault()}'.[/]");
                    }
                    if (parseException.Message.Contains("Unexpected option"))
                    {
                        AnsiConsole.MarkupLine($"[red]Error: Unexpected option '{args.FirstOrDefault()}'.[/]");
                    }
                } else if(exception is CommandParseException) {
                    AnsiConsole.MarkupLine($"[red]Error: Check command parameters.[/]");
                }
                else
                {
                    AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
                }
                return -1;
            });
            c.AddCommand<DownloadCommand>("download").WithExample("download", "I:\\titledb");
            c.AddCommand<MergeRegions>("merge");
            c.AddCommand<ImportTitles>("import");
            c.AddCommand<ImportCategories>("importcategories");
            c.AddCommand<ImportRatingContents>("importratingcontents");
            c.AddCommand<ResetDb>("resetdb");
            c.AddCommand<FreshDb>("freshdb").WithDescription("Create a new titledb by downloading,merging and importing everything.");
            c.AddCommand<DbVersion>("dbversion").WithDescription("Get the version of the database.");
            c.AddCommand<Compress>("compress").WithDescription("Compress titledb.db and titles.json files.");
            c.AddCommand<VersionCommand>("version").WithDescription("Displays the application version.");
            c.AddCommand<ImportNswDbReleases>("importnswdbreleases").WithDescription("Import NSW DB Releases.");
        });
        return app.Run(args);
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // add service registrations here
                services.AddHttpClient();
                services.Configure<AppSettings>(hostContext.Configuration.GetSection("DefaultSettings"));
                services.AddDbContext<SqliteDbContext>(options =>
                {
                    options.UseSqlite(hostContext.Configuration.GetConnectionString("SqliteConnection"));
                   // options.EnableSensitiveDataLogging(true);
                });
                //services.AddScoped<ITitleDbService, LegacyTitleDbService>();
                services.AddScoped<ITitleDbService, TitleDbService>();
                services.AddScoped<IImportTitleService, ImportTitleService>();
                services.AddScoped<IDbService, DbService>();
                services.AddScoped<IDownloadService, DownloadService>();
                services.AddScoped<IDbInitializationService, DbInitializationService>();
                services.AddScoped<ICompressionService, CompressionService>();
                services.AddScoped<INswReleaseService, NswReleaseService>();

            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.AddConsole();
            });
}