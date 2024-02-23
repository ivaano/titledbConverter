using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using Spectre.Console;
using titledbConverter.Models;
using Region = titledbConverter.Models.Region;

namespace titledbConverter.Services;

public class ImportTitleService : IImportTitleService
{
    private readonly IDbService _dbService;
    private readonly ILogger<ImportTitleService> _logger;
    private ConcurrentBag<Region> _regions;
    
    public ImportTitleService(IDbService dbService, ILogger<ImportTitleService> logger)
    {
        _dbService = dbService;
        _logger = logger;
        var regions = _dbService.GetRegions();
        _regions = new ConcurrentBag<Region>(regions);
    }

    private async Task<IEnumerable<TitleDbTitle>> ReadTitlesJsonFile(string fileLocation)
    {
        IEnumerable<TitleDbTitle> titles;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            titles = await JsonSerializer.DeserializeAsync<IEnumerable<TitleDbTitle>>(stream) ??
                     throw new InvalidOperationException();
        }
        stopwatch.Stop();
        return titles;
    }
    

    
    public async Task ImportTitlesFromFileAsync(string file)
    {
        var titles = await ReadTitlesJsonFile(file);
        //await _dbService.ImportTitles(titles);
        await _dbService.BulkInsertTitlesAsync(titles);
   }
}
