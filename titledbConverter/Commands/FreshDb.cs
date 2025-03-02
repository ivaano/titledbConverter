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
    private readonly ICompressionService _compressionService;
    private readonly IDbService _dbService;
    private readonly INswReleaseService _nswReleaseService;


    public FreshDb(
        IOptions<AppSettings> configuration,
        IDbInitializationService dbInitService,
        IDownloadService downloadService,
        ITitleDbService titleDbService,
        IImportTitleService importTitleService,
        ICompressionService compressionService,
        IDbService dbService,
        INswReleaseService nswReleaseService)
    {
        _dbInitService = dbInitService;
        _configuration = configuration;
        _downloadService = downloadService;
        _titleDbService = titleDbService;
        _importTitleService = importTitleService;
        _compressionService = compressionService;
        _dbService = dbService;
        _nswReleaseService = nswReleaseService;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<location>")]
        [Description("Specify folder where to save the files")]
        public string DownloadPath { get; set; } = null!;
        
        [CommandOption("-d")]
        [Description("Delete current db and create new one")]
        public bool Drop { get; set; }
        
        [CommandOption("-c|--compress <COMPRESSOUTPUT>")]
        [Description("Compress titledb.db and titles.json to this location")]
        public string? Compress { get; set; } 
    }
    
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DownloadPath))
        {
            settings.DownloadPath ??= _configuration.Value.DownloadPath;
            AnsiConsole.MarkupLine($"Using default download location {settings.DownloadPath}");
        }
        
        if (!string.IsNullOrWhiteSpace(settings.Compress))
        {
            var directoryInfo = new DirectoryInfo(settings.Compress);
            if (!directoryInfo.Exists)
            {
                return ValidationResult.Error($"Invalid compress folder location {settings.Compress}");
            }
        }
        
        return ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, FreshDb.Settings settings)
    {
        var titlesJson = Path.Combine(settings.DownloadPath, "titles.json");
        var dbPath = Path.Combine(settings.DownloadPath, "titledb.db");
        var nswPath = Path.Combine(settings.DownloadPath, "nswdb.xml");
        //Download
        var downloadSettings = new DownloadCommand.Settings
        {
            DownloadPath = settings.DownloadPath,
        };
        _downloadService.SetBaseUri(_configuration.Value.BaseUrl);

        var regions = await _downloadService.GetRegionsAsync(downloadSettings);
        if (regions is null) throw new InvalidOperationException("Unable to parse languages.json");
        var items = _downloadService.BuildDownloadList(regions);
        
        var tasks = items.Select(i => _downloadService.Download(i.url, i.name, settings.DownloadPath, false));
        var enumerableTasks = tasks.ToList();
        await _downloadService.RunWithThrottlingAsync(enumerableTasks, 3);
        await Task.WhenAll(enumerableTasks);        
        
        //Merge
        var mergeSettings = new MergeRegions.Settings
        {
            DownloadPath = settings.DownloadPath,
            Language = _configuration.Value.PreferredLanguage,
            Region = _configuration.Value.PreferredRegion,
            SaveFilePath = titlesJson
        };
        await _titleDbService.MergeAllRegionsAsync(mergeSettings);

        //Clear Db
        await _dbInitService.InitializeAsync(settings.Drop);
        
        //ImportCategories
        await _importTitleService.ImportAllCategories();
        await _importTitleService.ImportRatingContents(mergeSettings.SaveFilePath);

        
        //Import Titles
        await _importTitleService.ImportTitlesFromFileAsync(mergeSettings.SaveFilePath);

        await _dbService.AddDbHistory();
        
        if (!string.IsNullOrWhiteSpace(settings.Compress))
        {
            await _compressionService.CompressFileAsync(titlesJson, Path.Combine(settings.Compress, "titles.json.gz"));
        }
        
        //Process nswdb
        var importResult = await _nswReleaseService.ImportReleasesFromXmlAsync(nswPath);
        AnsiConsole.MarkupLineInterpolated($"[cyan3]{importResult} titles inserted from Nswdb.xml[/]");
        
        return 0;
    }

}