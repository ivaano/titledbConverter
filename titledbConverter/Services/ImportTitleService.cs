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
    
    public ImportTitleService(IDbService dbService, ILogger<ImportTitleService> logger)
    {
        _dbService = dbService;
        _logger = logger;
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
        await _dbService.BulkInsertTitlesAsync(titles);
   }

    private async Task<IEnumerable<CategoryRegionLanguage>> GetCategoriesFromTsv(string tsvFile)
    {
        using var reader = new StreamReader(tsvFile);
        var config = CsvConfiguration.FromAttributes<CategoryRegionLanguage>();
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CategoryRegionLanguage>();
        return records.ToList();
    }

    private static List<CategoryLanguage> CreateCategoryLanguages(IEnumerable<string> categoryNames)
    {
        return categoryNames.Select(category => new CategoryLanguage
        {
            Region = "US",
            Language = "en",
            Name = category
        }).ToList();
    }

    public async Task ImportAllCategories()
    {
        var regionLanguages = await GetRegionLanguages();
        // Read all categories from the dataset
        var datasetFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Datasets");
        
        var categories = await _dbService.GetCategoriesAsDict();
        
        var usCategories = (await GetCategoriesFromTsv(Path.Combine(datasetFolderPath, "categories.US.en.tsv"))).ToList();
        var usCategoriesDict = usCategories.ToDictionary(category => category.Original, category => category);

        var categoryNames = categories.IsFailed ? 
            usCategories.Select(c => c.Original) :
            categories.Value.Keys.Except(usCategoriesDict.Keys);

        var missingCategories = CreateCategoryLanguages(categoryNames);

        if (missingCategories.Count > 0)
        {
            var saveResult = await _dbService.SaveCategories(missingCategories);
            if (saveResult.IsSuccess)
            {
                categories = await _dbService.GetCategoriesAsDict();
            }
        } 
        
        var categoriesLanguages = await _dbService.GetCategoriesLanguagesAsDict();
        
        foreach (var regionLanguage in regionLanguages)
        {
            var tsvFile = Path.Combine(datasetFolderPath, $"categories.{regionLanguage.Region}.{regionLanguage.LanguageCode}.tsv");
            if (!File.Exists(tsvFile)) continue;
            var regionCategories = (await GetCategoriesFromTsv(Path.Combine(datasetFolderPath, tsvFile))).ToList();

            if (categoriesLanguages.IsSuccess)
            {
                var existingCategories = categoriesLanguages.Value.Keys.Intersect(regionCategories.Select(c => $"{regionLanguage.Region}-{regionLanguage.LanguageCode}.{c.Original}"));
                var existingCategoriesList = existingCategories.ToList();
                if (existingCategoriesList.Count == 0)
                {
                    var categoryLanguages = regionCategories.Select(category => new CategoryLanguage
                        {
                            Region = regionLanguage.Region,
                            Language = regionLanguage.LanguageCode,
                            Name = category.Original,
                            CategoryId = categories.Value[category.Translated].Id
                        })
                        .ToList();
                    await _dbService.SaveCategoryLanguages(categoryLanguages);

                }
                else
                {
                    //todo handle existing categories
                    regionCategories = regionCategories.Where(c => !existingCategoriesList.Contains($"{regionLanguage.Region}-{regionLanguage.LanguageCode}.{c.Translated}")).ToList();
                }
            }
            else
            {
                var categoryLanguages = regionCategories.Select(category => new CategoryLanguage
                    {
                        Region = regionLanguage.Region,
                        Language = regionLanguage.LanguageCode,
                        Name = category.Original,
                        CategoryId = categories.Value[category.Translated].Id
                    })
                    .ToList();
                await _dbService.SaveCategoryLanguages(categoryLanguages);
            }
        }
    }

    public async Task ImportRatingContents(string file)
    {
        var titles = await ReadTitlesJsonFile(file);
        var uniqueRatingContents = titles
            .Where(t => t.RatingContent != null) 
            .SelectMany(t => t.RatingContent)
            .Where(content => content != null)
            .Distinct();
        var ratingContents = uniqueRatingContents.Select(content => new RatingContent { Name = content });

        await _dbService.SaveRatingContents(ratingContents);
    }
}
