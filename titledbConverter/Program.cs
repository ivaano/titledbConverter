using System.Diagnostics.CodeAnalysis;
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
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConvertToSql))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConvertToSql.Settings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DownloadCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DownloadCommand.Settings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    public static int Main(string[] args)
    {
        var host = CreateHostBuilder(args);
        var app = new CommandApp(new TypeRegistrar(host));
        app.Configure(c =>
        {
            c.PropagateExceptions();
            c.AddCommand<DownloadCommand>("download").WithExample("download", "I:\\titledb");
            c.AddCommand<ConvertToSql>("convert");
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
                });
                services.AddScoped<ITitleDbService, TitleDbService>();
                services.AddScoped<IDbService, DbService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.AddConsole();
            });
}