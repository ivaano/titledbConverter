using System.ComponentModel;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;

public class DbVersion : AsyncCommand<DbVersion.Settings>
{
    private readonly IOptions<AppSettings> _configuration;
    private readonly IDbService _dbService;
    
    public DbVersion( 
        IOptions<AppSettings> configuration,
        IDbService dbService)
    {
        _configuration = configuration;
        _dbService = dbService;
    }
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-d")]
        [Description("Show title counts on this version")]
        public bool Details { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var version = await _dbService.GetLatestHistoryAsync();
       
        AnsiConsole.MarkupLine($"{version?.VersionNumber}");
        if (!settings.Details) return 0;
        
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        //grid.AddRow("Titles", "Base", "Updates", "DLCs");
        grid.AddRow(
            new Text("Titles", new Style(Color.Red)).Centered(),
            new Text("Base", new Style(Color.Green)).Centered(), 
            new Text("Updates", new Style(Color.Blue)).Centered(), 
            new Text("DLCs", new Style(Color.Blue)).Centered());
        grid.AddRow($"{version?.TitleCount}", $"{version?.BaseCount}", $"{version?.UpdateCount}", $"{version?.DlcCount}");
        /*
        grid.AddRow("Titles:", $"{version?.TitleCount}");
        grid.AddRow("Base:", $"{version?.BaseCount}");
        grid.AddRow("Updates:", $"{version?.UpdateCount}");
        grid.AddRow("DLC:", $"{version?.DlcCount}");
        */
        AnsiConsole.Write(grid);

        return 0;
    }
}