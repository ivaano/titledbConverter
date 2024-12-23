using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Spectre.Console;
using titledbConverter.Commands;
using titledbConverter.Models.Dto;
using titledbConverter.Models.Enums;
using titledbConverter.Services.Interface;
using titledbConverter.Utils;

namespace titledbConverter.Services;

public class TitleDbService : ITitleDbService
{
    private ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>> _concurrentCnmts = null!;
    private ConcurrentDictionary<string, TitleDbNca> _concurrentNcas = null!;
    private ConcurrentDictionary<string, TitleDbVersion> _concurrentVersions = null!;
    private ConcurrentDictionary<string, List<string>> _regionLanguages = null!;
    private ConcurrentBag<RegionLanguageMap> _regionLanguagesDefault = null!;
    private readonly ConcurrentDictionary<string, TitleDbTitle> _titlesDict = new();
    private bool _isCnmtsLoaded;
    private bool _isVersionsLoaded;
    private readonly ReaderWriterLockSlim _readLock = new();
    private readonly ReaderWriterLockSlim _writeLock = new();

    
    private void AddTitleToDict(string key, TitleDbTitle value)
    {
        _writeLock.EnterWriteLock();
        try
        {
            _titlesDict.TryAdd(key, value);
        }
        finally
        {
            _writeLock.ExitWriteLock();
        }
    }

    private TitleDbTitle GetTitleFromDict(string id)
    {
        _readLock.EnterReadLock();
        try
        {
            return _titlesDict.TryGetValue(id, out var value) ? value : new TitleDbTitle { Id = id };
        }
        finally
        {
            _readLock.ExitReadLock();
        }
    }
    
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
                (cnmts ?? throw new InvalidOperationException("Invalid cnmts file.")).ToDictionary(
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
    
    private async Task<ConcurrentDictionary<string, List<string>>> LoadRegionLanguagesAsync(string fileLocation, 
        string preferredRegion, string preferredLanguage)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var countryLanguages = await File.ReadAllTextAsync(fileLocation)
            .ContinueWith(fileContent => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(fileContent.Result));

        _regionLanguages = new ConcurrentDictionary<string, List<string>>(
            countryLanguages ?? throw new InvalidOperationException("Invalid languages.json file"));

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
    
    private async Task<ConcurrentDictionary<string, TitleDbVersion>> LoadVersionsJsonFilesAsync(string fileLocation)
    {
        if (_isVersionsLoaded) return _concurrentVersions;
        var stopwatch = Stopwatch.StartNew();
        await using (var stream = File.OpenRead(fileLocation))
        {
            var versions = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbVersion>>(stream);
            _concurrentVersions = new ConcurrentDictionary<string, TitleDbVersion>(
                (versions ?? throw new InvalidOperationException("Invalid versions.json file")).ToDictionary(
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

    private async Task<Dictionary<string, TitleDbNca>> LoadNcasAsync(string fileLocation)
    {
        var stopwatch = Stopwatch.StartNew();
        Dictionary<string, TitleDbNca> ncas;
        await using (var stream = File.OpenRead(fileLocation))
        {
            ncas = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbNca>>(stream) ??
                     throw new InvalidOperationException();
            
            foreach (var keyValuePair in ncas)
            {
                // Set the NcaId property of each TitleDbNca object to the dictionary key
                keyValuePair.Value.NcaId = keyValuePair.Key;
            }
            _concurrentNcas = new ConcurrentDictionary<string, TitleDbNca>(ncas);
        }
        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Loaded {fileLocation} in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
        return ncas;
    }
  
    private static async Task<SortedDictionary<string, TitleDbTitle>> LoadTitlesFromJsonFileAsync(string fileLocation)
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

    private async Task SaveTitlesToJsonFile(string fileName)
    {
        await using var createStream = File.Create(fileName);
        var sortedTitlesDict = new SortedDictionary<string, TitleDbTitle>(_titlesDict);
        await JsonSerializer.SerializeAsync(createStream, sortedTitlesDict.Select(x => x.Value), new JsonSerializerOptions { WriteIndented = true });
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
    
    private List<TitleDbVersions> GetTitleVersions(string titleId)
    {
        if (!_concurrentVersions.TryGetValue(titleId, out var vers)) return [];

        return vers.Select(version => new TitleDbVersions
                       {
                           VersionNumber = int.Parse(version.Key),
                           VersionDate = version.Value
                       }).ToList();
    }

    private void AddNewTitle(string titleId, TitleDbTitle title, RegionLanguageMap regionLanguage)
    {
        //region
        title.Region = regionLanguage.Region;
        title.Regions ??= [];
        title.Regions.Add(regionLanguage.Region);
        title.Language = regionLanguage.Language;
        
        //sort and remove dupes from languages
        title.Languages = title.Languages?.Distinct().OrderBy(language => language).ToList();

        //type
        if (title.IsBase == false && title is { IsDlc: false, IsUpdate: false })
        {
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
        }
        
        //add missing OtherApplicationId for DLC
        if (title is { IsDlc: true, OtherApplicationId: null }) 
        {
            var prefix = title.Id[..12];
            var parentTitle = _titlesDict.FirstOrDefault(
                x => x.Key.StartsWith(prefix) && x.Value.IsBase);
            
            if (!string.IsNullOrEmpty(parentTitle.Key))
            {
                title.OtherApplicationId = parentTitle.Key;
            }
        }
        
        //versions
        if (title.IsUpdate is false)
        {
            title.Versions = GetTitleVersions(title.Id);
            title.Version = title.Versions
                .Select(v => v.VersionNumber)
                .DefaultIfEmpty()
                .Max()
                .ToString();
        }

        //cnmts
        var contentCnmt = _concurrentCnmts
            .TryGetValue(titleId, out var cnmt) ? cnmt.Values.ToList() : null;
        title.Cnmts ??= [];
        if (contentCnmt is not null)
        {
            var cnmts = contentCnmt;
            var ncaIds = cnmts
                .Where(x => x.ContentEntries is not null)
                .SelectMany(k => k.ContentEntries ?? Enumerable.Empty<ContentEntry>())
                .Select(n => n.NcaId).ToList();

            title.Ncas = ncaIds
                .Select(id => _concurrentNcas.TryGetValue(id!, out var nca) ? nca : null)
                .OfType<TitleDbNca>()
                .ToList();
            
            title.Cnmts.AddRange(cnmts);
            
            //DLC updates
            if (title.IsDlc)
            {
                title.Version = contentCnmt
                    .Select(v => v.Version)
                    .DefaultIfEmpty()
                    .Max()
                    .ToString();
            }
        }

        //additional ids
        if (title.Ids is not null && title.Ids.Count > 1)
        {
            title.Ids
                .Where(id => id != title.Id)
                .Where(id => !_titlesDict.ContainsKey(id))
                .ToList()
                .ForEach(id => {
                    var additionalTitleId = title with { Id = id, Cnmts = null };
                    AddTitleToDict(id, additionalTitleId);
                });
        }
        
        AddTitleToDict(titleId, title);
    }

    private void UpdateTitleRegion(string titleId, RegionLanguageMap regionLanguage)
    {
        var title = GetTitleFromDict(titleId); 
        _writeLock.EnterWriteLock();

        try
        {
            if (string.IsNullOrEmpty(title.Region))
            {
                title.Region = regionLanguage.Region;
                title.Regions ??= [];
                title.Regions.Add(regionLanguage.Region);
            }
            else if (title.Region != regionLanguage.Region)
            {
                if (title.Regions is not null)
                {
                    var exists = title.Regions.Find(s => s == regionLanguage.Region);
                    if (exists is not null) return;
                    title.Regions.Add(regionLanguage.Region);
                }
                else
                {
                    title.Regions = [regionLanguage.Region];
                }
            }
        }
        finally
        {
            _writeLock.ExitWriteLock();
        }
    }

    //Additional Editions have same id, but different nsuId
    // jq 'map(select(.id != null)) | group_by(.id) | map(select(map(.nsuId) | unique | length > 1)) | flatten | map({id, nsuId, name})' US.en.json
    //an example would be 01003BC0000A0000, so first one will be the main
    //edition and the others will be additional editions
    //Further processing is needed for titles with no id
    // jq '.[] | select(.id == null) | {nsuId, id, name}' US.en.json
    //perhaps a fuzzy search and then do some kind of validation against the algolia index
    //for now only the first case is handled
    private void ProcessAdditionalEditions(
        SortedDictionary<string, TitleDbTitle> regionTitles, 
        SortedDictionary<string, TitleDbTitle> uniqueRegionTitles)
    {
        
        var additionalEditions = regionTitles
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Id)) 
            .GroupBy(kvp => kvp.Value.Id)                      
            .Where(group => group.Count() > 1)                 
            .ToDictionary(
                group => group.Key, 
                group => group.Skip(1).ToList());
        
        foreach (var kvp in additionalEditions)
        {
            foreach (var title in kvp.Value)
            {
                var screenshots = (title.Value.Screenshots ?? Enumerable.Empty<string>())
                    .Except(uniqueRegionTitles[kvp.Key].Screenshots ?? Enumerable.Empty<string>());
                var newEdition = new TitleDbEdition
                {
                    Id = title.Value.Id,
                    NsuId = title.Value.NsuId,
                    Name = title.Value.Name,
                    BannerUrl = title.Value.BannerUrl,
                    Description = title.Value.Description,
                    Screenshots = screenshots.ToList(),
                    ReleaseDate = title.Value.ReleaseDate,
                    Size = title.Value.Size
                };
                uniqueRegionTitles[kvp.Key].Editions ??= [];
                uniqueRegionTitles[kvp.Key].Editions?.Add(newEdition);
            }
        }
    }
    
    private void ProcessUpdates(string filePath, RegionLanguageMap regionLanguage)
    {
        using var reader = new StreamReader(filePath);
        var config = CsvConfiguration.FromAttributes<TitleDbVersionsTxt>();
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<TitleDbVersionsTxt>();
        var recList = records.ToList();
        
        
        var otherApplicationIdMapTitleId = _concurrentCnmts.Values
            .SelectMany(cnmt => cnmt.Values)
            .Where(cnmt => cnmt.TitleType == 129)
            .Where(cnmt => cnmt.OtherApplicationId is not null)
            .GroupBy(cnmt => cnmt.TitleId)
            .ToDictionary(
                g => g.First().TitleId!, 
                g => g.First().OtherApplicationId);
       
        foreach (var otherApplication in recList)
        {
            var titleType = GetTitleType(otherApplication.Id);
            if (titleType != TitleType.Update) continue;
            if (!otherApplicationIdMapTitleId.TryGetValue(otherApplication.Id, out var titleId)) continue;
            
            var baseTitle = GetTitleFromDict(titleId!);
            var title = new TitleDbTitle
            {
                Id = otherApplication.Id,
                Name = baseTitle.Name,
                Developer = baseTitle.Developer,
                Publisher = baseTitle.Publisher,
                Description = baseTitle.Description,
                BannerUrl = baseTitle.BannerUrl,
                OtherApplicationId = baseTitle.Id,
                RightsId = otherApplication.RightsId,
                Version = otherApplication.Version,
                IsUpdate = true,
                Versions = GetTitleVersions(baseTitle.Id),
            };
            AddNewTitle(otherApplication.Id, title, regionLanguage);
        }
    }
    
    private Task CountUpdatesAndDlcs()
    {
        var baseTitles = _titlesDict.Values.
            Where(x => x.IsBase).Select(x => x).ToList();
        var dlcTitleDbTitles = _titlesDict.Values.
            Where(x => x.IsDlc).Select(x => x).ToList();

        foreach (var title in baseTitles)
        {
            if (title.Versions is not null)
            {
                _titlesDict[title.Id].PatchCount = title.Versions.Count;
            }

            if (title.Cnmts is not null)
            {
                _titlesDict[title.Id].DlcCount = dlcTitleDbTitles.Count(x => x.OtherApplicationId == title.Id && x.IsDlc);
            }
        }

        return Task.CompletedTask;
    }
    
    private async Task MergeRegionsAsync(RegionLanguageMap regionLanguage, string downloadPath)
    {
        var regionFile = Path.Join(downloadPath, $"{regionLanguage.Region}.{regionLanguage.Language}.json");
        AnsiConsole.MarkupLineInterpolated($"[bold green]Processing {regionFile}[/]");

        var regionTitles = await LoadTitlesFromJsonFileAsync(regionFile);
        var uniqueRegionTitles = new SortedDictionary<string, TitleDbTitle>(
            regionTitles
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value.Id)) 
                .GroupBy(kvp => kvp.Value.Id)                    
                .ToDictionary(
                    group => group.Key,                         
                    group => group.First().Value                 
                )
        );
        ProcessAdditionalEditions(regionTitles, uniqueRegionTitles);
        
        var regionTitleKeys = uniqueRegionTitles
            .Select(x => x.Value.Id);
        var titleKeys = _titlesDict.Select(x => x.Key);
        var titleKeysHashSet = new HashSet<string>(titleKeys);
        var regionTitleKeysList = regionTitleKeys.ToList();
        var newKeys = regionTitleKeysList.Except(titleKeysHashSet);
        var newKeysHashSet = new HashSet<string>(newKeys);
        
        var newDictionary = newKeysHashSet
            .ToDictionary(
                x => x, 
                x => uniqueRegionTitles[x]);
        newDictionary.ToList().ForEach(kv => AddNewTitle(kv.Key, kv.Value, regionLanguage));
        AnsiConsole.MarkupLineInterpolated($"[dodgerblue2]Adding {newDictionary.Count} titles from {regionLanguage.Region}-{regionLanguage.Language} region[/]");

        var existingKeys = regionTitleKeysList.Intersect(titleKeysHashSet);
        var updateHashSet = new HashSet<string>(existingKeys);

        updateHashSet.ToList().ForEach(k => UpdateTitleRegion(k, regionLanguage));
        AnsiConsole.MarkupLineInterpolated($"[deepskyblue1]Updating {updateHashSet.Count} titles from {regionLanguage.Region}-{regionLanguage.Language} region[/]");
    }
    
    public async Task MergeAllRegionsAsync(MergeRegions.Settings settings)
    {
        await Task.WhenAll(
            LoadRegionLanguagesAsync(Path.Join(settings.DownloadPath, "languages.json"), settings.Region!, settings.Language!),
            LoadCnmtsJsonFilesAsync(Path.Join(settings.DownloadPath, "cnmts.json")),
            LoadNcasAsync(Path.Join(settings.DownloadPath, "ncas.json")),
            LoadVersionsJsonFilesAsync(Path.Join(settings.DownloadPath, "versions.json")));
        
        var preferedRegion = _regionLanguagesDefault
            .FirstOrDefault(
                r => r.Region == settings.Region && r.Language == settings.Language);
        
        MergeRegionsAsync(
            preferedRegion ?? throw new InvalidOperationException("Invalid Preferred Region"), 
            settings.DownloadPath!).GetAwaiter().GetResult();

        var sortedRegions = _regionLanguagesDefault
            .Where(r => r.Region != settings.Region)
            .GroupBy(r => r.Region)
            .Select(g => {
                var preferred = g.FirstOrDefault(r => r.PreferredLanguage == r.Language);
                return preferred ?? g.First();
            })
            .OrderBy(r => r.Language == r.PreferredLanguage ? 0 : 1)
            .ThenBy(r => r.Region)
            //.Take(2)
            .ToList();

        foreach (var region in sortedRegions)
        {
            await MergeRegionsAsync(region, settings.DownloadPath!);
        }
        
        /*
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 2 };
        await Parallel.ForEachAsync(
            sortedRegions,
            parallelOptions,
            async (region, _) => 
            {
                await MergeRegionsAsync(region, settings.DownloadPath);
            }
        );
        */
        
        //Sort Regions A..Z
        _titlesDict.Values
            .Where(t => t.Regions is not null)
            .ToList()
            .ForEach(t => t.Regions!.Sort((a, b) => 
                string.Compare(a, b, StringComparison.OrdinalIgnoreCase)));
        
        //updates are found in versions.txt
        ProcessUpdates(Path.Join(settings.DownloadPath, "versions.txt"), preferedRegion);

        await CountUpdatesAndDlcs();

        var baseGames = _titlesDict.Values.Count(x => x.IsBase);
        var dlcGames = _titlesDict.Values.Count(x => x.IsDlc);
        var updateGames = _titlesDict.Values.Count(x => x.IsUpdate);

        AnsiConsole.MarkupLine($"[bold green]Titles Count: {_titlesDict.Values.Count}[/]");
        AnsiConsole.MarkupLine($"[bold green]Base Titles: {baseGames}[/]");
        AnsiConsole.MarkupLine($"[bold green]DLC Titles: {dlcGames}[/]");
        AnsiConsole.MarkupLine($"[bold green]Update Titles: {updateGames}[/]");
        AnsiConsole.MarkupLine($"Save to: {settings.SaveFilePath}");
        await SaveTitlesToJsonFile(settings.SaveFilePath!);
    }

}