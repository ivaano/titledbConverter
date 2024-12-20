using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Data;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;


public sealed class MergeRegions : AsyncCommand<MergeRegions.Settings>
{
    private readonly ITitleDbService _titleDbService;
    private readonly IOptions<AppSettings> _configuration;
    
    
    public MergeRegions(ITitleDbService titleDbService, IOptions<AppSettings> configuration)
    {
        _titleDbService = titleDbService;
        _configuration = configuration;
    }
    
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where titledb files are located")]
        public string? DownloadPath { get; set; }
        
        [CommandArgument(1, "[location]")]
        [Description("Specify file where merged regions file should be saved")]
        public string? SaveFilePath { get; set; }
        
        [CommandOption("-r|--region")]
        [Description("Prefered region to import")]
        public string? Region { get; set; }
        
        [CommandOption("-l|--language")]
        [Description("Prefered language to import")]
        public string? Language { get; set; }
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.DownloadPath ??= _configuration.Value.DownloadPath;
        settings.Language ??= _configuration.Value.PreferredLanguage;
        settings.Region ??= _configuration.Value.PreferredRegion;
        
        if (settings.SaveFilePath == null)
        {
            settings.SaveFilePath = Path.Combine(settings.DownloadPath, "titles.json").ToString();
            AnsiConsole.MarkupLineInterpolated($"[bold yellow]Missing save filename using default filename[/] [greenyellow]{settings.SaveFilePath}[/]");
        }
        var stopwatch = Stopwatch.StartNew();

        await _titleDbService.MergeAllRegionsAsync(settings);

        stopwatch.Stop();
        AnsiConsole.MarkupLineInterpolated($"[darkviolet]Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");

        return 0;
    }
}