using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using titledbConverter.Settings;
using ValidationResult = Spectre.Console.ValidationResult;

namespace titledbConverter.Commands;

public sealed class DownloadCommand : AsyncCommand<DownloadCommand.Settings>
{
    private readonly IOptions<AppSettings> _configuration;
    private readonly HttpClient _httpClient;
    private Uri _baseUri = default!;
    
    public DownloadCommand(HttpClient httpClient, IOptions<AppSettings> configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public sealed class Settings : CommandSettings
    {
       
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where to save the files")]
        public string? DownloadPath { get; set; }
        
        [CommandOption("-u|--url")]
        [Description("Specify the base url to download the files")]
        public string? BaseUrl { get; set; }
    }
    
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DownloadPath))
        {
            settings.DownloadPath ??= _configuration.Value.DownloadPath;
            AnsiConsole.MarkupLine($"Using default download location {settings.DownloadPath}");
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            settings.BaseUrl ??= _configuration.Value.BaseUrl;
            _baseUri = new Uri(settings.BaseUrl);
            AnsiConsole.MarkupLine($"Using default config base url {settings.BaseUrl}");

        }
        
        return !Directory.Exists((settings.DownloadPath)) ? 
            ValidationResult.Error($"Path not found - {settings.DownloadPath}") : base.Validate(context, settings);

    }

    private async Task<Dictionary<string, List<string>>?> GetRegions(Settings settings)
    {
        var jsonString = await _httpClient.GetStringAsync(new Uri(_baseUri, "languages.json"));
        var countryLanguages = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
        
        if (countryLanguages is null) throw new InvalidOperationException("Unable to parse languages.json");
        
        AnsiConsole.MarkupLine($"[u]{countryLanguages.Count}[/] regions found.");
        await File.WriteAllBytesAsync(settings.DownloadPath + "/languages.json", Encoding.UTF8.GetBytes(jsonString));
        return countryLanguages;        
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var regions = await GetRegions(settings);
        if (regions is null) throw new InvalidOperationException("Unable to parse languages.json");
        
        var items = new List<(string name, string url)>
        {
            ("cnmts.json", new Uri(_baseUri, "cnmts.json").ToString()),
            ("versions.json", new Uri(_baseUri, "versions.json").ToString()),
            ("ncas.json", new Uri(_baseUri, "ncas.json").ToString()),
            ("versions.txt", new Uri(_baseUri, "versions.txt").ToString()),
        };
        
        foreach (var (key, value) in regions)
        {
            foreach (var lang in value)
            {
                var name = $"{key}.{lang}.json";
                var url = new Uri(_baseUri, name);
                items.Add((name, url.ToString()));
            }
        }

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(), 
                new ProgressBarColumn(), 
                new PercentageColumn(),
                new RemainingTimeColumn(), 
                new SpinnerColumn())
            .StartAsync(ctx => Task.WhenAll(items.Select(item => Download(ctx.AddTask(item.name), item.url, settings.DownloadPath))));

        return 0;
    }


    async Task Download(ProgressTask task, string url, string? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        try
        {
            using var response =
                await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            task.MaxValue(response.Content.Headers.ContentLength ?? 0);
            task.StartTask();

            var filename = Path.Combine(path, url.Substring(url.LastIndexOf('/') + 1));
            AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/] ({task.MaxValue} bytes)");

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None,
                8192, true);
            var buffer = new byte[8192];
            while (true)
            {
                var read = await contentStream.ReadAsync(buffer);
                if (read == 0)
                {
                    AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");
                    break;
                }

                task.Increment(read);
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
        }
    }
}
