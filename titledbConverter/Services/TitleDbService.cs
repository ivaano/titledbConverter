using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Services;

public class TitleDbService(IDbService dbService) : ITitleDbService
{
    private ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>> _concurrentCnmts = default!;
    private ConcurrentDictionary<string, TitleDbVersions> _concurrentVersions = default!;
    private Dictionary<string, TitleDbTitle> _regionTitles = default!;
    private ConcurrentDictionary<long, Lazy<Title>> _lazyDict = [];
    private ConcurrentBag<Title> _titles = [];
    private bool _isCnmtsLoaded = false;
    private bool _isVersionsLoaded = false;
    private bool _isTitlesLoaded = false;

    private async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>>> LoadCnmtsJsonFilesAsync(
        string fileLocation)
    {
        if (_isCnmtsLoaded) return _concurrentCnmts;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            var cnmts =
                await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, TitleDbCnmt>>>(stream);
            _concurrentCnmts = new ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>>(
                cnmts.ToDictionary(
                    kvp => kvp.Key.ToUpper(),
                    kvp => new ConcurrentDictionary<string, TitleDbCnmt>(kvp.Value)
                )
            );
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        _isCnmtsLoaded = true;
        return _concurrentCnmts;
    }

    private async Task<ConcurrentDictionary<string, TitleDbVersions>> LoadVersionsJsonFilesAsync(string fileLocation)
    {
        if (_isVersionsLoaded) return _concurrentVersions;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            var versions = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbVersions>>(stream);
            _concurrentVersions = new ConcurrentDictionary<string, TitleDbVersions>(
                versions.ToDictionary(
                    kvp => kvp.Key.ToUpper(),
                    kvp => kvp.Value
                )
            );
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        _isVersionsLoaded = true;
        return _concurrentVersions;
    }

    private async Task<Dictionary<string, TitleDbTitle>> LoadTitlesJsonFilesAsync(string fileLocation)
    {
        if (_isTitlesLoaded) return _regionTitles;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            _regionTitles = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbTitle>>(stream) ??
                      throw new InvalidOperationException();
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        _isTitlesLoaded = true;
        return _regionTitles;
    }
    
    private async Task<Dictionary<string, TitleDbTitle>> GetTitlesJsonFilesAsync(string fileLocation)
    {
        Dictionary<string, TitleDbTitle> titles;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            titles = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbTitle>>(stream) ??
                     throw new InvalidOperationException();
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return titles;
    }
    
    public async Task ImportAllRegionsAsync(string regionFolder)
    {
        //var files = Directory.GetFiles(regionFolder, "*.json");
        await Task.WhenAll(
            //LoadTitlesJsonFilesAsync(regionFile),
            LoadCnmtsJsonFilesAsync(Path.Join(regionFolder, "cnmts.json")),
            LoadVersionsJsonFilesAsync(Path.Join(regionFolder, "versions.json")));
        
        var jsonString = await File.ReadAllTextAsync(Path.Join(regionFolder, "languages.json"));
        var countryLanguages = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
        
        var files = new List<string>();
        foreach (var (key, value) in countryLanguages)
        {
            files.AddRange(value.Select(lang => Path.Join(regionFolder, $"{key}.{lang}.json")));
        }
        
        //string[] files = [Path.Join(regionFolder, "US.en.json"), Path.Join(regionFolder, "MX.en.json")];
        await Task.WhenAll(files.Select(ImportRegionAsync));
        var peta = _lazyDict.Values.Select(x => x.Value).ToList();
        await dbService.BulkInsertTitlesAsync(peta);
        //await dbService.BulkInsertTitlesAsync(_titles.ToList());
    }

    private void ProcessTitle(KeyValuePair<string, TitleDbTitle> game)
    {
        var title = new Title
        {
            NsuId = game.Value.NsuId,
            ApplicationId = game.Value.Id,
            TitleName = game.Value.Name,
            Cnmts = new List<Cnmt>(),
            Versions = new List<Version>()
        };

        if (!string.IsNullOrEmpty(game.Value.Id))
        {
            //DLC
            if (_concurrentCnmts.ContainsKey(game.Value.Id))
            {
                var concurrentCnmt = _concurrentCnmts[game.Value.Id];

                foreach (var (key, cnmt) in concurrentCnmt)
                {
                    var titleCnmt = new Cnmt
                    {
                        OtherApplicationId = cnmt.OtherApplicationId,
                        RequiredApplicationVersion = cnmt.RequiredApplicationVersion,
                        TitleType = cnmt.TitleType,
                        Version = cnmt.Version
                    };
                    title.Cnmts.Add(titleCnmt);
                    
                    //cnmt updates
                    if (cnmt.TitleType == 128 && !string.IsNullOrWhiteSpace(cnmt.OtherApplicationId) && _concurrentCnmts.TryGetValue(cnmt.OtherApplicationId, out var concurrentCnmt2))
                    {
                        foreach (var (key2, cnmt2) in concurrentCnmt2)
                        {
                            var titleCnmt2 = new Cnmt
                            {
                                OtherApplicationId = cnmt.TitleId,
                                RequiredApplicationVersion = cnmt2.RequiredApplicationVersion,
                                TitleType = cnmt2.TitleType,
                                Version = cnmt2.Version
                            };
                            title.Cnmts.Add(titleCnmt2);
                        }
                    }
                    
                }
            }

            //Update versions
            if (_concurrentVersions.TryGetValue(game.Value.Id, out var vers))
            {
                foreach (var titleVersion in vers.Select(version => new Version
                         {
                             VersionNumber = int.Parse(version.Key),
                             VersionDate = version.Value,
                             Title = title
                         }))
                {
                    title.Versions.Add(titleVersion);
                }
            }
        }
        /*
        if (_lazyDict.ContainsKey(game.Value.NsuId))
        {
            _lazyDict[game.Value.NsuId] = new Lazy<Title>(() => title);
        }
        else
        {
            _lazyDict.TryAdd(game.Value.NsuId, new Lazy<Title>(() => title));
        }
        */
        _lazyDict.GetOrAdd(game.Value.NsuId, new Lazy<Title>(() => title));
        //_titles.Add(title);
    }

    public async Task ImportRegionAsync(string regionFile)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold green]Processing {regionFile}[/]");
        var regionTitles = await GetTitlesJsonFilesAsync(regionFile);
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

        Parallel.ForEach(regionTitles, options, ProcessTitle);

        //titles.Clear();
        AnsiConsole.MarkupLine($"[lightslateblue]Title Count for {regionFile}: {regionTitles.Count}[/]");
    }


    public Task ImportCnmtsAsync(string cnmtsFile)
    {
        var cnmts = LoadCnmtsJsonFilesAsync(cnmtsFile);
        return Task.CompletedTask;
    }

    public Task ImportVersionsAsync(string versionsFile)
    {
        throw new NotImplementedException();
    }
}