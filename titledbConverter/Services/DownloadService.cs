using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Spectre.Console;
using titledbConverter.Commands;
using titledbConverter.Services.Interface;
using titledbConverter.Settings;

namespace titledbConverter.Services;

public class DownloadService : IDownloadService
{
    private readonly IOptions<AppSettings> _configuration;
    private readonly HttpClient _httpClient;
    private Uri _baseUri = default!;
    
    public DownloadService(HttpClient httpClient, IOptions<AppSettings> configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }
    public void SetBaseUri(string baseUrl)
    {
        _baseUri = new Uri(baseUrl);
    }
    
    public List<(string name, string url)> BuildDownloadList(Dictionary<string, List<string>> regions)
    {
        var items = new List<(string name, string url)>
        {
            ("nswl.xml", new Uri(_configuration.Value.NswDbReleasesUrl).ToString()),
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
        return items;
    }
    
    public async Task<Dictionary<string, List<string>>?> GetRegionsAsync(DownloadCommand.Settings settings)
    {
        var jsonString = await _httpClient.GetStringAsync(new Uri(_baseUri, "languages.json"));
        var countryLanguages = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
        
        if (countryLanguages is null) throw new InvalidOperationException("Unable to parse languages.json");
        
        AnsiConsole.MarkupLine($"[u]{countryLanguages.Count}[/] regions found.");
        await File.WriteAllBytesAsync(settings.DownloadPath + "/languages.json", Encoding.UTF8.GetBytes(jsonString));
        return countryLanguages;   
    }

    public  async Task DownloadWithProgressTask(ProgressTask task, string url, string name, string? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        try
        {
            using var response =
                await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            task.MaxValue(response.Content.Headers.ContentLength ?? 0);
            task.StartTask();

            var filename = Path.Combine(path, name);
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None,
                8192, true);
            var buffer = new byte[8192];
            while (true)
            {
                var read = await contentStream.ReadAsync(buffer);
                if (read == 0)
                {
                    //AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");
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
    
    public async Task RunWithThrottlingAsync(IEnumerable<Task> tasks, int maxDegreeOfParallelism)
    {
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

        var throttledTasks = tasks.Select(async task =>
        {
            await semaphore.WaitAsync();
            try
            {
                await task;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(throttledTasks);
    }
    
    public async Task Download(string url, string? path, bool verbose)
    {
        ArgumentNullException.ThrowIfNull(path);
        try
        {
            using var response =
                await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();


            var filename = Path.Combine(path, url.Substring(url.LastIndexOf('/') + 1));
            if (verbose)
            {
                AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/]");    
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None,
                8192, true);
            var buffer = new byte[8192];
            while (true)
            {
                var read = await contentStream.ReadAsync(buffer);
                if (read == 0)
                {
                    if (verbose)
                    {
                        AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");    
                    }
                    break;
                }
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
        }
    }
}