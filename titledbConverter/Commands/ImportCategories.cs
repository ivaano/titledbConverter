using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;

public class ImportCategories : AsyncCommand<ImportCategories.Settings>
{
    private readonly IImportTitleService _importTitleService;
    private readonly IOptions<AppSettings> _configuration;
    
    
    public ImportCategories(IImportTitleService importTitleService, IOptions<AppSettings> configuration)
    {
        _importTitleService = importTitleService;
        _configuration = configuration;
    }
    
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-f|--file <FILE>")]
        [Description("Specify titles.json file to import game categories from")]
        public string? ImportFile { get; set; }
        
        public override ValidationResult Validate()
        {
            return File.Exists(ImportFile)
                ? ValidationResult.Success()
                : ValidationResult.Error("Cannot access specified file.");
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var stopwatch = Stopwatch.StartNew();
        stopwatch.Stop();
        if (settings.ImportFile is not null)
        {
           await _importTitleService.ImportAllCategories();
        }
        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms");
        return 0;
    }
}