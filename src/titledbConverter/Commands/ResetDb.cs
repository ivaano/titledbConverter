using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;

namespace titledbConverter.Commands;

public class ResetDb : AsyncCommand<ResetDb.Settings>
{
    private readonly IDbInitializationService _dbInitService;


    public ResetDb(IDbInitializationService dbInitService)
    {
        _dbInitService = dbInitService;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-d")]
        [Description("Delete current db and create new one")]
        public bool Drop { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await _dbInitService.InitializeAsync(settings.Drop);
        return 0;
    }
}