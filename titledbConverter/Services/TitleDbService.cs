﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;
using titledbConverter.Commands;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using titledbConverter.Utils;
using Region = titledbConverter.Models.Region;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Services;

public class TitleDbService(IDbService dbService) : ITitleDbService
{
    private ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>> _concurrentCnmts = default!;
    private ConcurrentDictionary<string, TitleDbVersions> _concurrentVersions = default!;
    private ConcurrentDictionary<string, List<string>> _regionLanguages = default!;
    //private ConcurrentDictionary<string, List<RegionLanguage>> _regionLanguagesDefault = default!;
    private ConcurrentBag<RegionLanguage> _regionLanguagesDefault = default!;
    private Dictionary<string, TitleDbTitle> _regionTitles = default!;
    private ConcurrentDictionary<long, Lazy<Title>> _lazyDict = [];
    private ConcurrentDictionary<long, Lazy<TitleDbTitle>> _titlesDict = [];
    private ConcurrentBag<Title> _titles = [];
    private ConcurrentBag<Region> _regions = [];
    private bool _isCnmtsLoaded = false;
    private bool _isVersionsLoaded = false;
    private bool _isTitlesLoaded = false;
    //private 

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
    
    private async Task<ConcurrentDictionary<string, List<string>>> LoadRegionLanguagesAsync(string fileLocation, string preferredRegion, string preferredLanguage)
    {
        var stopwatch = Stopwatch.StartNew();
        var countryLanguages = await File.ReadAllTextAsync(fileLocation)
            .ContinueWith(fileContent => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(fileContent.Result));
        _regionLanguages = new ConcurrentDictionary<string, List<string>>(countryLanguages);

        _regionLanguagesDefault = new ConcurrentBag<RegionLanguage>(_regionLanguages
            .SelectMany(pair => pair.Value.Select(lang => new RegionLanguage
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
    


    private Title ImportTitle(KeyValuePair<string, TitleDbTitle> game)
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
        
        return title;
    }

    private void ProcessTitle(KeyValuePair<string, TitleDbTitle> game, RegionLanguage regionLanguage)
    {
        if (string.IsNullOrEmpty(game.Value.Id)) return;
        
        if (regionLanguage.Region == regionLanguage.PreferredRegion && regionLanguage.Language == regionLanguage.PreferredLanguage)
        {
            if (_lazyDict.TryGetValue(game.Value.NsuId, out var value))
            {
                var title = value.Value;
                title.TitleName = game.Value.Name;
                title.Region = regionLanguage.Region;
                title.Regions.Add(_regions.First(r => r.Name == regionLanguage.Region));
                //_lazyDict[game.Value.NsuId] = new Lazy<Title>(() => title);
                _lazyDict.AddOrUpdate(game.Value.NsuId, new Lazy<Title>(() => title), (_, _) => new Lazy<Title>(() => title));
            }
            else
            {
                var title = ImportTitle(game);
                title.Region = regionLanguage.Region;
                try
                {
                    var regions = new List<Region> {_regions.First(r => r.Name == regionLanguage.Region)};    
                    title.Regions = regions;
                } catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[bold red]Error: {e.Message} {regionLanguage.Region}[/]");
                }
                
                
                _lazyDict.GetOrAdd(game.Value.NsuId, new Lazy<Title>(() => title));
            }
        }
        else if (regionLanguage.Region != regionLanguage.PreferredRegion && regionLanguage.Language == regionLanguage.PreferredLanguage)
        {
            //Add region to title
            if (_lazyDict.TryGetValue(game.Value.NsuId, out var value))
            {
                var title = value.Value;
                //title.TitleName = game.Value.Name;
                if (string.IsNullOrEmpty(title.Region))
                {
                    title.Region = regionLanguage.Region;
                    title.Regions.Add(_regions.First(r => r.Name == regionLanguage.Region));
                }
                title.Regions.Add(new Region {Name = regionLanguage.Region});
                
            }
            else
            {
                var title = ImportTitle(game);
                title.Region = regionLanguage.Region;
                try
                {
                    var regions = new List<Region> {_regions.First(r => r.Name == regionLanguage.Region)};    
                    title.Regions = regions;
                } catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[bold red]Error: {e.Message} {regionLanguage.Region}[/]");
                }
                _lazyDict.GetOrAdd(game.Value.NsuId, new Lazy<Title>(() => title));
            }
        }
    }
    
    
    private Title ImportTitleDto(KeyValuePair<string, TitleDbTitle> game)
    {
       game.Value.Cnmts = new List<Cnmt>();
       game.Value.Versions = new List<Version>();

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
        
        return title;
    }
    

    private void ProcessTitleDto(KeyValuePair<string, TitleDbTitle> game, RegionLanguage regionLanguage)
    {
        if (string.IsNullOrEmpty(game.Value.Id)) return;

        if (regionLanguage.Region == regionLanguage.PreferredRegion &&
            regionLanguage.Language == regionLanguage.PreferredLanguage)
        {
            if (_titlesDict.TryGetValue(game.Value.NsuId, out var value))
            {
                var title = value.Value;
                if (string.IsNullOrEmpty(title.Region))
                {
                    title.Region = regionLanguage.Region;
                    var region = _regions.First(r => r.Name == regionLanguage.Region);
                    title.Regions?.Add(region.Name);
                }
            }
        }
        else
        {
            var title = ImportTitle(game);
            title.Region = regionLanguage.Region;
            try
            {
                var regions = new List<Region> {_regions.First(r => r.Name == regionLanguage.Region)};    
                title.Regions = regions;
            } catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]Error: {e.Message} {regionLanguage.Region}[/]");
            }
            _lazyDict.GetOrAdd(game.Value.NsuId, new Lazy<Title>(() => title));
        }
    }

    private async Task ImportRegionAsync(RegionLanguage regionLanguage, string downloadPath)
    {
        var regions = dbService.GetRegions();
        _regions = new ConcurrentBag<Region>(regions);
        var regionFile = Path.Join(downloadPath, $"{regionLanguage.Region}.{regionLanguage.Language}.json");
        AnsiConsole.MarkupLineInterpolated($"[bold green]Processing {regionFile}[/]");
        var regionTitles = await GetTitlesJsonFilesAsync(regionFile);
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

        //Parallel.ForEach(regionTitles, options, kvp => ProcessTitle(kvp, regionLanguage));
        Parallel.ForEach(regionTitles, options, kvp => ProcessTitleDto(kvp, regionLanguage));

        //titles.Clear();
        AnsiConsole.MarkupLine($"[lightslateblue]Title Count for {regionFile}: {regionTitles.Count}[/]");
    }

    public async Task ImportAllRegionsAsync(ConvertToSql.Settings settings)
    {
        await Task.WhenAll(
            LoadRegionLanguagesAsync(Path.Join(settings.DownloadPath, "languages.json"), settings.Region, settings.Language),
            LoadCnmtsJsonFilesAsync(Path.Join(settings.DownloadPath, "cnmts.json")),
            LoadVersionsJsonFilesAsync(Path.Join(settings.DownloadPath, "versions.json")));

        var preferedRegion = _regionLanguagesDefault.FirstOrDefault(r => r.Region == settings.Region);
        ImportRegionAsync(preferedRegion, settings.DownloadPath).Wait();
        AnsiConsole.MarkupLine($"[bold green]Lazy Dict Size: {_lazyDict.Values.Count}[/]");
        //await ImportRegionAsync(preferedRegion, settings.DownloadPath);
        //var allOthers = _regionLanguagesDefault.Where(r => r.Region != settings.Region).Where(r => r.Language == settings.Language).Select(r => r.Region).ToList();
        await Task.WhenAll(_regionLanguagesDefault.Where(r => r.Region != settings.Region)
            //.Where(r => r.Language == settings.Language)
            .Select(region => ImportRegionAsync(region, settings.DownloadPath)));
        //await Task.WhenAll(_regionLanguagesDefault.Where(r => r.Language == settings.Language).Select(region => ImportRegionAsync(region, settings.DownloadPath)));
        //await Task.WhenAll(_regionLanguagesDefault.Select(region => ImportRegionAsync(region, settings.DownloadPath)));
        var peta = _lazyDict.Values.Select(x => x.Value).ToList();


        
        //await dbService.BulkInsertTitlesAsync(peta);
        //await dbService.BulkInsertTitlesAsync(_titles.ToList());
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