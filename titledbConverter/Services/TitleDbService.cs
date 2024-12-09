using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Spectre.Console;
using titledbConverter.Commands;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Models.Enums;
using titledbConverter.Services.Interface;
using titledbConverter.Utils;
using Region = titledbConverter.Models.Region;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Services;

public class TitleDbService(IDbService dbService) : ITitleDbService
{
    private ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>> _concurrentCnmts = default!;
    private ConcurrentDictionary<string, TitleDbVersion> _concurrentVersions = default!;
    private ConcurrentDictionary<string, List<string>> _regionLanguages = default!;
    private ConcurrentBag<RegionLanguageMap> _regionLanguagesDefault = default!;
    private ConcurrentDictionary<string, Lazy<TitleDbTitle>> _titlesDict = [];
    private ConcurrentBag<Region> _regions = [];
    private bool _isCnmtsLoaded = false;
    private bool _isVersionsLoaded = false;

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

    private async Task<ConcurrentDictionary<string, TitleDbVersion>> LoadVersionsJsonFilesAsync(string fileLocation)
    {
        if (_isVersionsLoaded) return _concurrentVersions;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            var versions = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbVersion>>(stream);
            _concurrentVersions = new ConcurrentDictionary<string, TitleDbVersion>(
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

    public async Task<Dictionary<string, List<string>>?> GetRegionLanguages(string fileLocation)
    {
        var countryLanguages = await File.ReadAllTextAsync(fileLocation)
            .ContinueWith(fileContent => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(fileContent.Result));
        return countryLanguages;
    }
    
    private async Task<ConcurrentDictionary<string, List<string>>> LoadRegionLanguagesAsync(string fileLocation, string preferredRegion, string preferredLanguage)
    {
        var stopwatch = Stopwatch.StartNew();
        /*
        var countryLanguages = await File.ReadAllTextAsync(fileLocation)
            .ContinueWith(fileContent => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(fileContent.Result));
       */
        var countryLanguages = await GetRegionLanguages(fileLocation);
        _regionLanguages = new ConcurrentDictionary<string, List<string>>(countryLanguages);

        _regionLanguagesDefault = new ConcurrentBag<RegionLanguageMap>(_regionLanguages
            .SelectMany(pair => pair.Value.Select(lang => new RegionLanguageMap
            {
                Region = pair.Key,
                Language = lang,
                PreferredRegion = preferredRegion,
                PreferredLanguage = preferredLanguage
            })));

        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return _regionLanguages;
    }

    private async Task<Dictionary<string, TitleDbNca>> LoadNcasAsync(string fileLocation)
    {
        var stopwatch = Stopwatch.StartNew();
        Dictionary<string, TitleDbNca> ncas;
        await using (var stream = File.OpenRead(fileLocation))
        {
            ncas = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbNca>>(stream) ??
                     throw new InvalidOperationException();
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return ncas;
    }
  
    private static async Task<SortedDictionary<string, TitleDbTitle>> GetTitlesJsonFilesAsync(string fileLocation)
    {
        SortedDictionary<string, TitleDbTitle> titles;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            titles = await JsonSerializer.DeserializeAsync<SortedDictionary<string, TitleDbTitle>>(stream) ??
                     throw new InvalidOperationException();
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return titles;
    }

    private static TitleType GetTitleType(string titleId)
    {
        var titleIdNum = Convert.ToInt64(titleId, 16);
        var isDlc = ((ulong)titleIdNum & 0xFFFFFFFFFFFFE000) != ((ulong)titleIdNum & 0xFFFFFFFFFFFFF000);
        var idExt = titleIdNum & 0x0000000000000FFF;

        if (isDlc)
        {
            return TitleType.AddOnContent;
        }
        
        return idExt == 0 ? TitleType.Base : TitleType.Update;
    }
    
    private static TitleDbTitle InitTitleDto(TitleDbTitle title)
    {
        title.Cnmts ??= [];
        title.Versions ??= [];
        var type = GetTitleType(title.Id);
        
        switch (type)
        {
            case TitleType.Base:
                title.IsBase = true;
                break;
            case TitleType.AddOnContent:
                title.IsDlc = true;
                break;
            case TitleType.Update:
                title.IsUpdate = true;
                break;
        }
        return title;
    }

    private TitleDbTitle ImportTitleDto(KeyValuePair<string, TitleDbTitle> game)
    {
       var title = InitTitleDto(game.Value);
       if (string.IsNullOrEmpty(title.Id)) return title;

       //DLC
        if (_concurrentCnmts.ContainsKey(title.Id))
        {
            var concurrentCnmt = _concurrentCnmts[game.Value.Id];

            foreach (var (key, cnmt) in concurrentCnmt)
            {

                title.Cnmts.Add(cnmt);
                    
                //cnmt updates
                if (cnmt.TitleType == 128 && !string.IsNullOrWhiteSpace(cnmt.OtherApplicationId) &&
                    _concurrentCnmts.TryGetValue(cnmt.OtherApplicationId, out var concurrentCnmt2))
                {
                    foreach (var (key2, cnmt2) in concurrentCnmt2)
                    {
                        cnmt2.OtherApplicationId = cnmt.TitleId;
                        title.Cnmts.Add(cnmt2);
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
                         //Title = title
                     }))
            {
                title.Versions.Add(titleVersion);
            }
        }

        return title;
    }

    private void ProcessTitleAdditionalIds(TitleDbTitle title)
    {
        if (title.Ids is not null && title.Ids.Count > 1)
        {
            foreach (var id in title.Ids)
            {
                if (_titlesDict.TryGetValue(id, out var value)) continue;

                var titleValue = new TitleDbTitle
                {
                   Id = id,
                   Ids = title.Ids,
                   BannerUrl = title.BannerUrl,
                   Category = title.Category,
                   Description = title.Description,
                   Developer = title.Developer,
                   FrontBoxArt = title.FrontBoxArt,
                   IconUrl = title.IconUrl,
                   Intro = title.Intro,
                   IsDemo = title.IsDemo,
                   Key = title.Key,
                   Languages = title.Languages,
                   Name = title.Name,
                   NsuId = title.NsuId,
                   NumberOfPlayers = title.NumberOfPlayers,
                   Publisher = title.Publisher,
                   Rating = title.Rating,
                   RatingContent = title.RatingContent,
                   Region = title.Region,
                   Language = title.Language,
                   ReleaseDate = title.ReleaseDate,
                   RightsId = title.RightsId,
                   Screenshots = title.Screenshots,
                   Size = title.Size,
                   Version = title.Version,
                   Versions = title.Versions,
                   Cnmts = title.Cnmts,
                   Ncas = title.Ncas
                };
                _titlesDict.GetOrAdd(id, new Lazy<TitleDbTitle>(() => titleValue));
            }
        }
    }

    private void AddOrUpdateTitle(KeyValuePair<string, TitleDbTitle> game, RegionLanguageMap regionLanguage)
    {
        if (_titlesDict.TryGetValue(game.Value.Id, out var value))
        {
            var title = value.Value;
            if (string.IsNullOrEmpty(title.Region))
            {
                title.Region = regionLanguage.Region;
                var region = _regions.First(r => r.Name == regionLanguage.Region);
                title.Regions?.Add(region.Name);
            }

            if (title.Region != regionLanguage.Region)
            {
                if (title.Regions != null)
                {
                    var exists = title.Regions.Find(s => s == regionLanguage.Region);
                    if (exists is not null) return;
                    title.Regions.Add(regionLanguage.Region);
                }
                else
                {
                    title.Regions = new List<string> { regionLanguage.Region };
                }
            }
        }
        else
        {
            var title = ImportTitleDto(game);
            title.Region = regionLanguage.Region;
            title.Language = regionLanguage.Language;

            try
            {
                var regions = new List<string> { _regions.First(r => r.Name == regionLanguage.Region).Name };  
                if (title.Regions is not null)
                {
                    title.Regions.AddRange(regions);
                }
                else
                {
                    title.Regions = regions;
                }
            } catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]Error: {e.Message} {regionLanguage.Region}[/]");
            }
            _titlesDict.GetOrAdd(game.Value.Id, new Lazy<TitleDbTitle>(() => title));
            ProcessTitleAdditionalIds(title);
        }
    }
    
    private void ProcessTitleDto(KeyValuePair<string, TitleDbTitle> game, RegionLanguageMap regionLanguage)
    {
        if (!string.IsNullOrEmpty(game.Value.Id) )
        {
            AddOrUpdateTitle(game, regionLanguage);
        }
    }

    private async Task ImportRegionAsync(RegionLanguageMap regionLanguage, string downloadPath)
    {

        var regionFile = Path.Join(downloadPath, $"{regionLanguage.Region}.{regionLanguage.Language}.json");
        var regionTitles = await GetTitlesJsonFilesAsync(regionFile);
        AnsiConsole.MarkupLineInterpolated($"[bold green]Processing {regionFile}[/]");
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

        //Parallel.ForEach(regionTitles, options, kvp => ProcessTitle(kvp, regionLanguage));
        Parallel.ForEach(regionTitles, options, kvp => ProcessTitleDto(kvp, regionLanguage));

        //titles.Clear();
        AnsiConsole.MarkupLine($"[lightslateblue]Title Count for {regionFile}: {regionTitles.Count}[/]");
    }

    private async Task TitleComparer(ConvertToSql.Settings settings)
    {
        //load nut titles
        var nutTitles = await File.ReadAllTextAsync(Path.Join(settings.DownloadPath, "titles.json"))
            .ContinueWith(fileContent => JsonSerializer.Deserialize<Dictionary<string, NutTitle>>(fileContent.Result));
        var notFound = 0;
        /*
        foreach (var nutTitle in nutTitles)
        {
            if (!_titlesDict.ContainsKey(nutTitle.Key))
            {
                AnsiConsole.MarkupLine($"[bold red]Notfound {nutTitle.Key}[/]");
                notFound++;
            }
        }
        */
        foreach (var title in _titlesDict)
        {
            if (!nutTitles.ContainsKey(title.Key))
            {
                AnsiConsole.MarkupLine($"[bold red]Notfound {title.Key}[/]");
                notFound++;
            }
        }
        AnsiConsole.MarkupLine($"[bold red]Notfound: {notFound}[/]");
    }
    
    private void ImportNcas(Dictionary<string, TitleDbNca> ncas)
    {
        foreach (var nca in ncas)
        {
            nca.Value.NcaId = nca.Key;
            if (_titlesDict.TryGetValue(nca.Value.TitleId, out var game))
            {
                var title = InitTitleDto(game.Value);

                if (title.Ncas != null)
                {
                    title.Ncas.Add(nca.Value);
                }
                else
                {
                    title.Ncas = new List<TitleDbNca> { nca.Value };
                }
                _titlesDict.TryUpdate(nca.Value.TitleId,  new Lazy<TitleDbTitle>(() => title), game);            
            }
            else
            {
                var titleNcas = new List<TitleDbNca> { nca.Value };
                var title = InitTitleDto(new TitleDbTitle
                {
                    Id = nca.Value.TitleId,
                    Ncas = titleNcas
                });
                _titlesDict.GetOrAdd(nca.Value.TitleId, new Lazy<TitleDbTitle>(() => title));
            }
        }
    }
    
    private void ImportVersionsTxt(string filePath)
    {
        using var reader = new StreamReader(filePath);
        var config = CsvConfiguration.FromAttributes<TitleDbVersionsTxt>();

        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<TitleDbVersionsTxt>();
        foreach (var r in records)
        {
            var title = InitTitleDto(new TitleDbTitle
            {
                Id = r.Id,
                RightsId = r.RightsId,
                Version = r.Version
            });
            _titlesDict.GetOrAdd(r.Id, new Lazy<TitleDbTitle>(() => title));

        }
    }

    private async Task SaveJsonTitles(string fileName)
    {
        await using var createStream = File.Create(fileName);
        var sortedTitlesDict = new SortedDictionary<string, Lazy<TitleDbTitle>>(_titlesDict);
        await JsonSerializer.SerializeAsync(createStream, sortedTitlesDict.Values.Select(x => x.Value), new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task CleanRegions()
    {
        foreach (var title in _titlesDict.Values)
        {
            if (title.Value.Regions is not null)
            {
                title.Value.Regions = title.Value.Regions.Distinct().Where(region => region != null).ToList();
                title.Value.Regions.Sort();
            }
            
            if (title.Value.Languages is not null)
            {
                title.Value.Languages = title.Value.Languages.Distinct().Where(lang => lang != null).ToList();
                title.Value.Languages.Sort();
            }
        }
    }

    private async Task EnrichUpdates(IEnumerable<TitleDbTitle> updates)
    {
        foreach (var update in updates)
        {
            if (!_concurrentCnmts.TryGetValue(update.Id, out var value))
                continue;
            foreach (var cnmt in value.Values)
            {
   
                var baseGame = cnmt.OtherApplicationId != null && _titlesDict.TryGetValue(cnmt.OtherApplicationId, out var baseTitle) ? baseTitle.Value : null;
                if (baseGame is null) continue;
                _titlesDict[update.Id].Value.Name = baseGame.Name;
                _titlesDict[update.Id].Value.Developer = baseGame.Developer;
                _titlesDict[update.Id].Value.Publisher = baseGame.Publisher;
                _titlesDict[update.Id].Value.Description = baseGame.Description;
                _titlesDict[update.Id].Value.BannerUrl = baseGame.BannerUrl;
                _titlesDict[update.Id].Value.OtherApplicationId = baseGame.Id;
            }
        }
    }

    public async Task MergeAllRegionsAsync(ConvertToSql.Settings settings)
    {
        var regions = await dbService.GetRegionsAsync();
        _regions = new ConcurrentBag<Region>(regions);
        
        await Task.WhenAll(
            LoadRegionLanguagesAsync(Path.Join(settings.DownloadPath, "languages.json"), settings.Region, settings.Language),
            LoadCnmtsJsonFilesAsync(Path.Join(settings.DownloadPath, "cnmts.json")),
            LoadVersionsJsonFilesAsync(Path.Join(settings.DownloadPath, "versions.json")));
        
        var preferedRegion = _regionLanguagesDefault.FirstOrDefault(r => r.Region == settings.Region && r.Language == settings.Language);
        ImportRegionAsync(preferedRegion, settings.DownloadPath).Wait();
        AnsiConsole.MarkupLine($"[bold green]Lazy Dict Size: {_titlesDict.Values.Count}[/]");
        
        await Task.WhenAll(_regionLanguagesDefault.Where(r => r.Region != settings.Region)
            //.Where(r => r.Language == settings.Language)
            .Select(region => ImportRegionAsync(region, settings.DownloadPath)));
        
        //var peta = _titlesDict.Values.Select(x => x.Value).ToList();

        AnsiConsole.MarkupLine($"[bold green]Lazy Dict Size: {_titlesDict.Values.Count}[/]");

        var ncas = await LoadNcasAsync(Path.Join(settings.DownloadPath, "ncas.json"));
        ImportNcas(ncas);
        ImportVersionsTxt(Path.Join(settings.DownloadPath, "versions.txt"));

        var baseGames = _titlesDict.Values.Where(x => x.Value.IsBase).Select(x => x.Value).ToList();
        var dlcGames = _titlesDict.Values.Where(x => x.Value.IsDlc).Select(x => x.Value).ToList();
        var updateGames = _titlesDict.Values.Where(x => x.Value.IsUpdate).Select(x => x.Value).ToList();
        await EnrichUpdates(updateGames);
        await CleanRegions();
        AnsiConsole.MarkupLine($"[bold green]Titles Count Count: {_titlesDict.Values.Count}[/]");
        AnsiConsole.MarkupLine($"[bold green]Base Titles: {baseGames.Count}[/]");
        AnsiConsole.MarkupLine($"[bold green]DLC Titles: {dlcGames.Count}[/]");
        AnsiConsole.MarkupLine($"[bold green]Update Titles: {updateGames.Count}[/]");
        AnsiConsole.MarkupLine($"Save to: {Path.Join(settings.DownloadPath, "titles-ivan.json")}");
        await SaveJsonTitles(Path.Join(settings.DownloadPath, "titles-ivan.json"));
    }

}