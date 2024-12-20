using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            c.PropagateExceptions();
            c.AddCommand<DownloadCommand>("download").WithExample("download", "I:\\titledb");
            c.AddCommand<MergeRegions>("merge");
            c.AddCommand<ImportTitles>("import");
            c.AddCommand<ImportCategories>("importcategories");
            c.AddCommand<ImportRatingContents>("importratingcontents");
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
               // services.AddScoped<ITitleDbService, TitleDbService>();
                services.AddScoped<ITitleDbService, TitleDbServiceNotLazy>();
                services.AddScoped<IImportTitleService, ImportTitleService>();
                services.AddScoped<IDbService, DbService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.AddConsole();
            });
}