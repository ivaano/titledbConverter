using System.ComponentModel;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;

public class FreshDb : AsyncCommand<FreshDb.Settings>
{
    private readonly IDbInitializationService _dbInitService;
    private readonly IOptions<AppSettings> _configuration;
    private readonly IDownloadService _downloadService;
    private readonly ITitleDbService _titleDbService;
    private readonly IImportTitleService _importTitleService;

    public FreshDb(
        IOptions<AppSettings> configuration,
        IDbInitializationService dbInitService,
        IDownloadService downloadService,
        ITitleDbService titleDbService,
        IImportTitleService importTitleService)
    {
        _dbInitService = dbInitService;
        _configuration = configuration;
        _downloadService = downloadService;
        _titleDbService = titleDbService;
        _importTitleService = importTitleService;

    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where to save the files")]
        public string? DownloadPath { get; set; }
        
        [CommandOption("-d")]
        [Description("Delete current db and create new one")]
        public bool Drop { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, FreshDb.Settings settings)
    {
        //Download
        var downloadSettings = new DownloadCommand.Settings
        {
            DownloadPath = settings.DownloadPath,
        };
        _downloadService.SetBaseUri(_configuration.Value.BaseUrl);

        var regions = await _downloadService.GetRegionsAsync(downloadSettings);
        if (regions is null) throw new InvalidOperationException("Unable to parse languages.json");
        var items = _downloadService.BuildDownloadList(regions);
        /*
        var tasks = items.Select(i => _downloadService.Download(i.url, settings.DownloadPath));
        await _downloadService.RunWithThrottlingAsync(tasks, 2);
        await Task.WhenAll(tasks);        
        */
        foreach (var item in items)
        {
            await _downloadService.Download(item.url, settings.DownloadPath, true);
        }
        
        //Merge
        var mergeSettings = new MergeRegions.Settings
        {
            DownloadPath = settings.DownloadPath,
            Language = _configuration.Value.PreferredLanguage,
            Region = _configuration.Value.PreferredRegion,
            SaveFilePath = Path.Combine(settings.DownloadPath, "titles.json")
        };
        await _titleDbService.MergeAllRegionsAsync(mergeSettings);

        //Clear Db
        await _dbInitService.InitializeAsync(settings.Drop);
        
        //ImportCategories
        await _importTitleService.ImportAllCategories();
        await _importTitleService.ImportRatingContents(mergeSettings.SaveFilePath);

        
        //Import Titles
        await _importTitleService.ImportTitlesFromFileAsync(mergeSettings.SaveFilePath);

        return 0;
    }

}