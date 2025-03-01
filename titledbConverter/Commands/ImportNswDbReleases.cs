using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;

public class ImportNswDbReleases : AsyncCommand<ImportNswDbReleases.Settings>
{
    private readonly INswReleaseService _nswReleaseService;
    private readonly IOptions<AppSettings> _configuration;
    
    public ImportNswDbReleases(INswReleaseService nswReleaseService, IOptions<AppSettings> configuration)
    {
        _nswReleaseService = nswReleaseService;
        _configuration = configuration;
    }
    
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-f|--file <FILE>")]
        [Description("Specify nswdb.xml file to import releases from")]
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
            var importResult = await _nswReleaseService.ImportReleasesFromXmlAsync(settings.ImportFile);
        }
        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms");

        return 0;
    }
}