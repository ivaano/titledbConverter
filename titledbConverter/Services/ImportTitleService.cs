using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using GTranslate;
using GTranslate.Translators;
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
    
    public async Task ImportTitlesCategoriesAsync(string file)
    {
        var titles = await ReadTitlesJsonFile(file);
        var uniqueCategories = new HashSet<string>();
        var categoryList = new List<KeyValuePair<string, string>>();
        var cats = new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.InvariantCultureIgnoreCase);
        var toTranslate = new List<KeyValuePair<string, string>>();
        foreach (var title in titles)
        {
            if (title.Category is not { Count: > 0 }) continue;
            categoryList.AddRange(from category in title.Category where uniqueCategories.Add(category) 
                                  select new KeyValuePair<string, string>(title.Language, category));
        }
        
        foreach (var category in categoryList)
        {
            if (category.Key.Equals("en"))
            {
                if (cats.ContainsKey(category.Value)) continue;
                cats.Add(category.Value, [category]);
            }
            else
            {
                toTranslate.Add(category);
            }
        }
        var translator = new GoogleTranslator();

        foreach (var category in toTranslate)
        {
            var translation = await translator.TranslateAsync(category.Value, "en");
            if (cats.TryGetValue(translation.Translation, out var catList))
            {
                catList.Add(category);
            }
            else
            {
                cats.Add(translation.Translation, [category]);
            }

            
        }
        
        
        
        
/*
        foreach (var title in titles)
        {
            if (title.Category is { Count: > 0 })
            {
                var newCategories = new List<KeyValuePair<string, string>>();
                foreach (var category in title.Category)
                {
                    if (cats.ContainsKey(category)) break;
                    if (title.Language is not null && title.Language.Equals("en"))
                    {
                        var kvp = new KeyValuePair<string, string>(title.Language, category);
                        newCategories.Add(kvp);
                        cats.Add(category, newCategories);
                    }
                    else
                    {
                        var kvp = new KeyValuePair<string, string>(title.Language, "translatedCat");
                        newCategories.Add(kvp);
                        cats.Add(category, newCategories);
                    }
                }
            }
            
        }
*/
        var pepa = 1;
        //await _dbService.ImportTitlesCategories(titles);
    }
}
