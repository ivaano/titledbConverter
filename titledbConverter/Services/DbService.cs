using System.Diagnostics;
using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using titledbConverter.Data;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using Region = titledbConverter.Models.Region;

namespace titledbConverter.Services;

public class DbService(SqliteDbContext context, ILogger<DbService> logger) : IDbService, IDisposable
{
    private Dictionary<string, int> _languageTable = default!;
    private bool _dataFetched = false;
    
    public Task<int> AddTitleAsync(Title title)
    {
        context.Titles.Add(title);
        return context.SaveChangesAsync();
    }

    private static List<string> CategoryLanguageMapper(TitleDbTitle title)
    {
        var categoryLanguages = (from category in title.Category where !string.IsNullOrEmpty(title.Region) && !string.IsNullOrEmpty(title.Language) select $"{title.Region}-{title.Language}.{category}").ToList();

        return categoryLanguages;
    }
    
    private async Task FetchData()
    {
        if (_dataFetched) return;
        _languageTable = await context.Languages.ToDictionaryAsync(language => language.LanguageCode, language => language.Id);
        _dataFetched = true;
    }
   
    
    public async Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles)
    {
        var stopwatch = Stopwatch.StartNew();
        //db data
        var regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
        var categoryLanguageDictionary = context.CategoryLanguages.ToDictionary(cl => $"{cl.Region}-{cl.Language}.{cl.Name}", cl => cl.CategoryId);
        
        var titleEntities = new List<Title>();
        var titlesWithRegions = new Dictionary<string, List<string>>();
        var titlesWithCategories = new Dictionary<string, List<string>>();
        var titlesWithLanguages = new Dictionary<string, List<string>>();
        
        foreach (var title in titles)
        {
            var mappedTitle = MapTitle(title);
            if (title.Regions is { Count: > 0 })
            {
                mappedTitle.Regions = new List<Region>();
                titlesWithRegions.Add(title.Id, title.Regions);
            }

            if (title.Category is { Count: > 0 })
            {
                mappedTitle.Categories = new List<Category>();
                titlesWithCategories.Add(title.Id, CategoryLanguageMapper(title));
            }
            
            if (title.Languages is { Count: > 0 })
            {
                mappedTitle.Languages = new List<Language>();
                titlesWithLanguages.Add(title.Id, title.Languages);
            }


            titleEntities.Add(mappedTitle);

            if (titleEntities.Count >= 1000)
            {
                await BulkInsertAndUpdate(titleEntities,  titlesWithRegions, regionDictionary, categoryLanguageDictionary, titlesWithCategories, titlesWithLanguages);
            }
        }

        if (titleEntities.Count > 0)
        {
            await BulkInsertAndUpdate(titleEntities, titlesWithRegions, regionDictionary, categoryLanguageDictionary, titlesWithCategories, titlesWithLanguages);
        }

        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Imported all in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
    }
    
    public async Task<Result<Dictionary<string, Category>>> GetCategoriesAsDict()
    {
        var categories = await context.Categories.ToListAsync();
        return categories.Count == 0 ? Result.Fail<Dictionary<string, Category>>("No categories found") : 
            Result.Ok(categories.ToDictionary(category => category.Name, category => category));
    }

    public async Task<Result<Dictionary<string, CategoryLanguage>>> GetCategoriesLanguagesAsDict()
    { 
        var categories = await context.CategoryLanguages.Include(cl => cl.Category).ToListAsync();
        return categories.Count == 0 ? Result.Fail<Dictionary<string, CategoryLanguage>>("No categories found") : 
            Result.Ok(categories.ToDictionary(cl => 
                    $"{cl.Region}-{cl.Language}.{cl.Name}", 
                cl => cl));
    }


    public async Task<Result<int>> SaveCategories(IEnumerable<CategoryLanguage> categoryLanguages)
    {
        var categoryNames = context.Categories.ToDictionary(category => category.Name, category => category);
        var existingCategoryNames = context.CategoryLanguages.ToList();
        foreach (var categoryLanguage in categoryLanguages)
        {
            var exists = existingCategoryNames
                .Where(x => x.Name == categoryLanguage.Name)
                .Where(x => x.Region == categoryLanguage.Region)
                .FirstOrDefault(x => x.Language == categoryLanguage.Language);

            if (exists is not null) continue;
            
            if(categoryNames.TryGetValue(categoryLanguage.Name, out var categoryName))
            {
                categoryLanguage.Category = categoryName;
               
                context.CategoryLanguages.Add(categoryLanguage);
            }
            else
            {
                var newCategoryName = new Category {Name = categoryLanguage.Name};
                categoryLanguage.Category = newCategoryName;
                context.CategoryLanguages.Add(categoryLanguage);
            }
        }

        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Ok(result) : Result.Fail("No categories saved");
    }

    public async Task<Result<int>> SaveCategoryLanguages(IEnumerable<CategoryLanguage> categoryLanguages)
    {
        context.CategoryLanguages.AddRange(categoryLanguages);
        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Ok(result) : Result.Fail("No categories saved");
    }

    
    private async Task BulkInsertAndUpdate(List<Title> titleEntities, 
        Dictionary<string, List<string>> regionTitlesTitleId, Dictionary<string, int> regionDictionary, 
        Dictionary<string, int> categoryLanguageDictionary,  Dictionary<string, List<string>> titleCategories, 
        Dictionary<string, List<string>> titlesWithLanguages)
    {
        await FetchData();
        var regionTitles = new List<RegionTitle>();
        var categoryTitles = new List<TitleCategory>();
        var titleLanguages = new List<TitleLanguage>();
        
        // Bulk insert titles
        await context.BulkInsertAsync(titleEntities, new BulkConfig() { SetOutputIdentity = true, PreserveInsertOrder = true });
        var titleDictionary = titleEntities.ToDictionary(t => t.ApplicationId, t => t.Id);

        // Bulk insert region titles
        regionTitles.AddRange(regionTitlesTitleId.Keys.SelectMany(titleEntity => 
            regionTitlesTitleId[titleEntity].Where(region => regionDictionary.TryGetValue(region, out var regionId))
                .Select(region => new RegionTitle() { RegionId = regionDictionary[region], TitleId = titleDictionary[titleEntity] })));
        await context.BulkInsertAsync(regionTitles, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });

        // Bulk insert category titles
        categoryTitles.AddRange(titleCategories.SelectMany(titleCategory => 
            titleCategory.Value.Where(category => categoryLanguageDictionary.TryGetValue(category, out var categoryId))
                .Select(category => new TitleCategory { CategoryId = categoryLanguageDictionary[category], TitleId = titleDictionary[titleCategory.Key] })));
        await context.BulkInsertAsync(categoryTitles, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true }); 
        
        // Bulk insert title languages
        titleLanguages.AddRange(titlesWithLanguages.SelectMany(titleLanguage => 
            titleLanguage.Value.Where(language => _languageTable.TryGetValue(language, out var languageId))
                .Select(language => new TitleLanguage { LanguageId = _languageTable[language], TitleId = titleDictionary[titleLanguage.Key] })));
        await context.BulkInsertAsync(titleLanguages, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });
        
        
        titleEntities.Clear();
        regionTitles.Clear();
        regionTitlesTitleId.Clear();
        categoryTitles.Clear();
        titleCategories.Clear();
        titlesWithLanguages.Clear();
    }
    
    public async Task<ICollection<Region>> GetRegionsAsync()
    {
        return await context.Regions.Include(region => region.Languages).ToListAsync();
    }

    public async Task ImportTitles(IEnumerable<TitleDbTitle> titles)
    {
        var stopwatch = Stopwatch.StartNew();
        var regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
        var titleEntities = new List<Title>();

        foreach (var title in titles)
        {
            var mappedTitle = MapTitle(title);
            if (title.Regions is { Count: > 0 })
            {
                mappedTitle.Regions = title.Regions
                    .Where(regionDictionary.ContainsKey)
                    .Select(region => context.Regions.Local.Single(x => x.Id == regionDictionary[region]))
                    .ToList();
            }

            titleEntities.Add(mappedTitle);
            if (titleEntities.Count >= 1000)
            {
                await SaveAndClear(titleEntities, regionDictionary);
            }
        }

        if (titleEntities.Count > 0)
        {
            await SaveAndClear(titleEntities, regionDictionary);
        }

        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Imported all in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
    }

    private async Task SaveAndClear(List<Title> titleEntities, Dictionary<string, int> regionDictionary)
    {
        context.Titles.AddRange(titleEntities);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
        titleEntities.Clear();
    }
    
    
/*
    public async Task ImportTitles(IEnumerable<TitleDbTitle> titles)
    {
        var stopwatch = Stopwatch.StartNew();

        var regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);

        var batchCount = 0;
        var titleEntities = new List<Title>();

        foreach (var title in titles)
        {
            batchCount++;
            AnsiConsole.MarkupLineInterpolated($"[blue]Importing[/][yellow] {title.Id}[/] - [green]{title.Name}[/]");

            var mappedTitle = MapTitle(title);
            if (title.Regions is { Count: > 0 })
            {
                mappedTitle.Regions = new List<Region>();
                foreach (var titleRegion in title.Regions)
                {
                    if (regionDictionary.TryGetValue(titleRegion, out var regionId))
                    {
                        mappedTitle.Regions.Add(context.Regions.Local.Single(x => x.Id == regionId));
                    }
                }
            }

            titleEntities.Add(mappedTitle);
            if (batchCount >= 1000)
            {
                AnsiConsole.MarkupLineInterpolated($"[blue]Saving...[/]");

                context.Titles.AddRange(titleEntities);
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();
                regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
                titleEntities.Clear();
                batchCount = 0;
            }
        }

        if (batchCount > 0)
        {
            context.Titles.AddRange(titleEntities);
            await context.SaveChangesAsync();
        }

        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Imported all in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
    }
*/
    private static Title MapTitle(TitleDbTitle title)
    {
        var newTitle = new Title
        {
            NsuId = title.NsuId,
            ApplicationId = title.Id,
            TitleName = title.Name,
            Region = title.Region,
        };
        return newTitle;
    }

    public async Task ImportTitle(TitleDbTitle title)
    {
        var newTitle = new Title
        {
            NsuId = title.NsuId,
            ApplicationId = title.Id,
            TitleName = title.Name,
            Region = title.Region,
        };

    }

    public Task ImportTitlesCategories(IEnumerable<TitleDbTitle> titles)
    {
        throw new NotImplementedException();
    }



    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            context.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}