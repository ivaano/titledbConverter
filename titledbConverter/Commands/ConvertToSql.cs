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
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var stopwatch = Stopwatch.StartNew();

        settings.DownloadPath ??= _configuration.Value.DownloadPath;
        //var regionFile = Path.Join(settings.DownloadPath, "US.en.json");
        //var regionFile = Path.Join(settings.DownloadPath, "ivan.json");
        //await _titleDbService.ImportRegionAsync(regionFile);
        await _titleDbService.ImportAllRegionsAsync(settings.DownloadPath);
        /*
        var cnmtFile = Path.Join(settings.DownloadPath, "cnmts.json");
        await _titleDbService.ImportCnmtsAsync(cnmtFile);
        
        var versionsFile = Path.Join(settings.DownloadPath, "versions.json");
        await _titleDbService.ImportCnmtsAsync(versionsFile);
        */
        stopwatch.Stop();
        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms");

        return 0;
    }
}