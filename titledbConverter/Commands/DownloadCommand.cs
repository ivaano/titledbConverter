using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace titledbConverter.Commands;

public sealed class DownloadCommand(HttpClient httpClient) : AsyncCommand<DownloadCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
       
        [CommandArgument(0, "[location]")]
        [Description("Specify folder where to save the files")]
        public string? DownloadPath { get; init; }
    }
    
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DownloadPath))
        {
            return ValidationResult.Error("Download path cannot be empty");
        }
        
        return !Directory.Exists((settings.DownloadPath)) ? 
            ValidationResult.Error($"Path not found - {settings.DownloadPath}") : base.Validate(context, settings);
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var items = new (string name, string url)[]
        {
            ("US", "https://github.com/blawar/titledb/raw/master/US.en.json"),
            ("cnmts", "https://github.com/blawar/titledb/raw/master/cnmts.json"),
            ("versions", "https://raw.githubusercontent.com/blawar/titledb/master/versions.json"),
        };

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
                await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
