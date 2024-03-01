using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
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
    //private ConcurrentBag<Region> _regions;
    
    public ImportTitleService(IDbService dbService, ILogger<ImportTitleService> logger)
    {
        _dbService = dbService;
        _logger = logger;
       // var regions = _dbService.GetRegions();
       // _regions = new ConcurrentBag<Region>(regions);
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

    private async Task<IEnumerable<(string Region, string LanguageCode)>> GetRegionLanguages()
    {
        var regions = await _dbService.GetRegionsAsync();
        return regions.SelectMany(region => region.Languages,
                (region, language) => (Region: region.Name, LanguageCode: language.LanguageCode))
            .ToList();
    }
    

    
    public async Task ImportTitlesFromFileAsync(string file)
    {
        var titles = await ReadTitlesJsonFile(file);
        //await _dbService.ImportTitles(titles);
        await _dbService.BulkInsertTitlesAsync(titles);
   }

    private async Task<IEnumerable<CategoryRegionLanguage>> GetCategoriesFromTsv(string tsvFile)
    {
        using var reader = new StreamReader(tsvFile);
        var config = CsvConfiguration.FromAttributes<CategoryRegionLanguage>();
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CategoryRegionLanguage>();
        return records.ToList();
        /*
         foreach (var category in records)
         {
             Console.WriteLine(category);
         }
         */
    }


    public async Task ImportAllCategories()
    {
        var regionLanguages = await GetRegionLanguages();
        // Read all categories from the dataset
        var datasetFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Datasets");
        
        var categories = _dbService.GetCategoriesAsDict();
        
        var usCategories = await GetCategoriesFromTsv(Path.Combine(datasetFolderPath, "categories.US.en.tsv"));

        var categoriesLanguages = usCategories.Select(category => new CategoryLanguage
            {
                Region = "US",
                Language = "en",
                Name = category.Original
            })
            .ToList();

        await _dbService.SaveCategoryLanguages(categoriesLanguages);
        
        
        foreach (var regionLanguage in regionLanguages)
        {
            var tsvFile = Path.Combine(datasetFolderPath, $"categories.{regionLanguage.Region}.{regionLanguage.LanguageCode}.tsv");
            if (File.Exists(tsvFile))
            {
                var regionCategories = await GetCategoriesFromTsv(Path.Combine(datasetFolderPath, tsvFile));

                categoriesLanguages = regionCategories.Select(category => new CategoryLanguage
                    {
                        Region = regionLanguage.Region,
                        Language = regionLanguage.LanguageCode,
                        Name = category.Original
                    })
                    .ToList();

                await _dbService.SaveCategoryLanguages(categoriesLanguages);

            }
        }
        
    }

    
    public async Task ImportTitlesCategoriesAsync(string file)
    {
        var classifier = new CategoryLanguageClassifier();
        
        
        var titles = await ReadTitlesJsonFile(file);
        var uniqueCategories = new HashSet<string>();
        //var categoryList = new List<KeyValuePair<string, Tuple<string, string>>>();
        //var categoryList = new List<KeyValuePair<string, (string Region, string Language)>>();
        var categoryList = new List<(string Region, string Language, string Name)>();        
        var cats = new Dictionary<string, List<(string Region, string Language, string Name)>>(StringComparer.InvariantCultureIgnoreCase);
        //var cats = new Dictionary<string, List<KeyValuePair<string, Dictionary<string, string>>>>(StringComparer.InvariantCultureIgnoreCase);
        var toTranslate = new List<KeyValuePair<string, string>>();
        //get unique categories
        foreach (var title in titles)
        {
            if (title.Category is not { Count: > 0 }) continue;

            categoryList.AddRange(from category in title.Category where uniqueCategories.Add(category) 
                                  select (title.Region, title.Language, category));

            //categoryList.AddRange(from category in title.Category where uniqueCategories.Add(category)
            //select new (Region: category, Language: title.Region, Name: title.Language));            
            //select new KeyValuePair<string, (string Region, string Language)>(category, (title.Region, title.Language)));  
            //select new KeyValuePair<string, Tuple<string, string>>(category, new Tuple<string, string>(title.Region, title.Language)));
        }




        foreach (var category in categoryList)
        {
            if (category.Region.Equals("US") && category.Language.Equals("en"))
            {
                if (cats.ContainsKey(category.Name)) continue;
                cats.Add(category.Name, [category]);
            }
            else
            {
                toTranslate.Add(new KeyValuePair<string, string>(category.Region, category.Name));
            }
        }


        var taco = 1;
        /*
        foreach (var category in categoryList)
        {
            if (category.Key.Equals("US"))
            {
                if (cats.ContainsKey(category.Key)) continue;
                cats.Add(category.Value, [category]);
            }
            else
            {
                toTranslate.Add(category);
            }
        }
        var translator = new BingTranslator();

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
        
        foreach (var cat in cats)
        {
            AnsiConsole.MarkupLine($"[springgreen3_1]{cat.Key}[/]");
            foreach (var kvp in cat.Value)
            {
                AnsiConsole.MarkupLine($"[springgreen3_1]{kvp.Key} - {kvp.Value}[/]");
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
