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
        
        
        [CommandOption("-d <FILE>")]
        [Description("Specify a directory with multiple xml files to import releases nswdb file will take precedence.")]
        public string? ImportDirectory { get; set; }
        
        
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
            var importResult = await _nswReleaseService.ImportReleasesFromXmlAsync(settings.ImportFile, true);
            AnsiConsole.MarkupLineInterpolated($"[cyan3]{importResult} titles inserted[/]");
        }
        
        if (!string.IsNullOrEmpty(settings.ImportDirectory))
        {
            if (Directory.Exists(settings.ImportDirectory))
            {
                if ((File.GetAttributes(settings.ImportDirectory) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    var importResult = await _nswReleaseService.ImportReleasesFromDirectoryAsync(settings.ImportDirectory);
                    AnsiConsole.MarkupLineInterpolated($"[cyan3]{importResult} titles from {settings.ImportDirectory} inserted[/]");

                }
                else
                {
                    AnsiConsole.WriteLine($"Error: {settings.ImportDirectory} is not a directory.");
                    return 1;
                }
            }
            else
            {
                AnsiConsole.WriteLine($"Directory does not exist: {settings.ImportDirectory}");
                return 1;
            }
        }
        
        AnsiConsole.MarkupLineInterpolated($"[darkviolet]Elapsed time: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return 0;
    }
}