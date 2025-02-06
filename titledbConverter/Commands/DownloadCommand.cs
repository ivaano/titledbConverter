using System.ComponentModel;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;
using ValidationResult = Spectre.Console.ValidationResult;

namespace titledbConverter.Commands;

public sealed class DownloadCommand : AsyncCommand<DownloadCommand.Settings>
{
    private readonly IOptions<AppSettings> _configuration;
    private readonly IDownloadService _downloadService;
    
    public DownloadCommand( 
        IOptions<AppSettings> configuration,
        IDownloadService downloadService)
    {
        _configuration = configuration;
        _downloadService = downloadService;
    }

    public sealed class Settings : CommandSettings
    {
       
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where to save the files")]
        public string? DownloadPath { get; set; }
        
        [CommandOption("-u|--url")]
        [Description("Specify the base url to download the files")]
        public string? BaseUrl { get; set; }
    }
    
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DownloadPath))
        {
            settings.DownloadPath ??= _configuration.Value.DownloadPath;
            AnsiConsole.MarkupLine($"Using default download location {settings.DownloadPath}");
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            settings.BaseUrl ??= _configuration.Value.BaseUrl;
            _downloadService.SetBaseUri(settings.BaseUrl);
            AnsiConsole.MarkupLine($"Using default config base url {settings.BaseUrl}");

        }
        
        return !Directory.Exists((settings.DownloadPath)) ? 
            ValidationResult.Error($"Path not found - {settings.DownloadPath}") : base.Validate(context, settings);
    }

    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var regions = await _downloadService.GetRegionsAsync(settings);
        if (regions is null) throw new InvalidOperationException("Unable to parse languages.json");
        var items = _downloadService.BuildDownloadList(regions);
  
        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var semaphore = new SemaphoreSlim(5); 

                var throttledTasks = items.Select(async item =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var progressTask = ctx.AddTask(item.name);
                        await _downloadService.DownloadWithProgressTask(progressTask, item.url, settings.DownloadPath);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(throttledTasks);
            });

        
        return 0;
    }
}
