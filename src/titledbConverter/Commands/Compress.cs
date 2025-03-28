using System.ComponentModel;
using Microsoft.Extensions.Options;
using Spectre.Console.Cli;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Commands;

public class Compress : AsyncCommand<Compress.Settings>
{
    private readonly IOptions<AppSettings> _configuration;
    private readonly ICompressionService _compressionService;


    public Compress( 
        IOptions<AppSettings> configuration,
        ICompressionService compressionService)
    {
        _configuration = configuration;
        _compressionService = compressionService;
    }
    
    public sealed class Settings : CommandSettings
    {

        [CommandArgument(0, "<inputFile>")]
        [Description("Specify file to compress")]
        public string InputFile { get; set; } = null!;
        
        [CommandArgument(1, "<outputFile>")]
        [Description("Specify target file")]
        public string OutputFile { get; set; } = null!;
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _compressionService.CompressFileAsync(settings.InputFile, settings.OutputFile);
        return Task.FromResult(0);
    }
}