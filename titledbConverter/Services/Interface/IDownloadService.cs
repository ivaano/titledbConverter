using Spectre.Console;
using titledbConverter.Commands;

namespace titledbConverter.Services.Interface;

public interface IDownloadService
{
    Task<Dictionary<string, List<string>>?> GetRegionsAsync(DownloadCommand.Settings settings);

    List<(string name, string url)> BuildDownloadList(Dictionary<string, List<string>> regions);

    Task DownloadWithProgressTask(ProgressTask task, string url, string name, string? path);
    Task Download( string url, string? path, bool verbose);

    Task RunWithThrottlingAsync(IEnumerable<Task> tasks, int maxDegreeOfParallelism);
    void SetBaseUri(string baseUri);    
}