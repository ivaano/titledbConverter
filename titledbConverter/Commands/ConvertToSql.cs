using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Data;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;


public sealed class ConvertToSql : AsyncCommand<ConvertToSql.Settings>
{
    private readonly ITitleDbService _titleDbService;
    private readonly IOptions<AppSettings> _configuration;
    
    
    public ConvertToSql(ITitleDbService titleDbService, IOptions<AppSettings> configuration)
    {
        _titleDbService = titleDbService;
        _configuration = configuration;
    }
    
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where titledb files are located")]
        public string? DownloadPath { get; set; }
        
        [CommandOption("-r|--region")]
        [Description("Prefered region to import")]
        public string? Region { get; set; }
        
        [CommandOption("-l|--language")]
        [Description("Prefered language to import")]
        public string? Language { get; set; }
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var stopwatch = Stopwatch.StartNew();

        settings.DownloadPath ??= _configuration.Value.DownloadPath;
        settings.Language ??= _configuration.Value.PreferredLanguage;
        settings.Region ??= _configuration.Value.PreferredRegion;

        await _titleDbService.MergeAllRegionsAsync(settings);

        stopwatch.Stop();
        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms");

        return 0;
    }
}