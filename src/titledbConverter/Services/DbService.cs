using System.Collections.Concurrent;
using System.Diagnostics;
using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using titledbConverter.Data;
using titledbConverter.Enums;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using Region = titledbConverter.Models.Region;
using Version = titledbConverter.Models.Version;


namespace titledbConverter.Services;

public class DbService(SqliteDbContext context) : IDbService, IDisposable
{
    private Dictionary<string, int> _languageTable = null!;
    private Dictionary<string, int> _ratingContentTable = null!;
    private readonly ConcurrentDictionary<string, int> _titlesApplicationIdMap = new();
    private bool _dataFetched;
    
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
        _ratingContentTable = await context.RatingContents.ToDictionaryAsync(rc => rc.Name, rc => rc.Id);
        _dataFetched = true;
    }
   
    
    public async Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles)
    {
        var stopwatch = Stopwatch.StartNew();
        //db data
        await ClearTables();
        var regionDictionary = context.Regions.ToDictionary(
            region => region.Name, 
            region => region.Id);
        var categoryLanguageDictionary = context.CategoryLanguages.
            ToDictionary(cl => $"{cl.Region}-{cl.Language}.{cl.Name}", cl => cl.CategoryId);
        
        var titleEntities = new List<Title>();
        var titlesWithRegions = new Dictionary<string, List<string>>();
        var titlesWithCategories = new Dictionary<string, List<string>>();
        var titlesWithLanguages = new Dictionary<string, List<string>>();
        var titlesWithCnmts = new Dictionary<string, List<TitleDbCnmt>>();
        var titlesVersions = new Dictionary<string, List<Version>>();
        var titlesScreenshots = new Dictionary<string, List<string>>();
        var titlesWithRatingContent = new Dictionary<string, List<string>>();
        var titlesWithEditions = new Dictionary<string, List<TitleDbEdition>>();
        
        foreach (var title in titles)
        {
            var mappedTitle = MapTitle(title);

            if (title.Editions is { Count: > 0 })
            {
                titlesWithEditions.Add(title.Id, title.Editions);
            }

            if (title.Screenshots is { Count: > 0 })
            {
                titlesScreenshots.Add(title.Id, title.Screenshots);
            }
            
            if (title.RatingContent is { Count: > 0 })
            {
                titlesWithRatingContent.Add(title.Id, title.RatingContent);
            }
            
            if (title.Versions is { Count: > 0 })
            {
                var titleVersions = title.Versions.Select(version => new Version
                {
                    VersionNumber = version.VersionNumber,
                    VersionDate = version.VersionDate
                }).ToList();
                titlesVersions.Add(title.Id, titleVersions);
            }
            
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

            if (title.Cnmts is { Count: > 0 })
            {
                titlesWithCnmts.Add(title.Id, title.Cnmts);
            }

            titleEntities.Add(mappedTitle);

            if (titleEntities.Count >= 1000)
            {
                await BulkTitlesInserts(titleEntities,  titlesWithRegions, regionDictionary, 
                    categoryLanguageDictionary, titlesWithCategories, titlesWithLanguages, titlesWithCnmts, 
                    titlesVersions, titlesScreenshots, titlesWithRatingContent, titlesWithEditions);
            }
        }

        if (titleEntities.Count > 0)
        {
            await BulkTitlesInserts(titleEntities, titlesWithRegions, regionDictionary, 
                categoryLanguageDictionary, titlesWithCategories, titlesWithLanguages, titlesWithCnmts, 
                titlesVersions, titlesScreenshots, titlesWithRatingContent, titlesWithEditions);
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
    
    public async Task<Result<int>> SaveRatingContents(IEnumerable<RatingContent> ratingContents)
    {
        var currentRatingContents = context.RatingContents.ToList();
        
        var newRatingContents = ratingContents
            .Where(rc => currentRatingContents.All(c => c.Name != rc.Name)) 
            .OrderBy(rc => rc.Name)
            .ToList();

        if (newRatingContents.Count == 0) return Result.Ok(0);
        
        context.RatingContents.AddRange(newRatingContents);
        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Ok(result) : Result.Fail("No rating contents saved");

    }
    
    private async Task ClearTables()
    {
        //raw queries 10 sec faster than ef
        await context.Database.ExecuteSqlAsync($"DELETE FROM TitleRegion");
        await context.Database.ExecuteSqlAsync($"DELETE FROM TitleCategory");
        await context.Database.ExecuteSqlAsync($"DELETE FROM TitleLanguages");
        await context.Database.ExecuteSqlAsync($"DELETE FROM TitleRatingContents");
        await context.Database.ExecuteSqlAsync($"DELETE FROM Cnmts");
        await context.Database.ExecuteSqlAsync($"DELETE FROM Editions");
        await context.Database.ExecuteSqlAsync($"DELETE FROM Versions");
        await context.Database.ExecuteSqlAsync($"DELETE FROM ScreenShots");
        await context.Database.ExecuteSqlAsync($"DELETE FROM Titles");
        await context.Database.ExecuteSqlAsync($"DELETE FROM NswReleaseTitles");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Cnmts'");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Edition'");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Versions'");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'ScreenShots'");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Titles'");
        await context.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'NswReleaseTitles'");
    }

    private static DateTime? ParseIntDate(int? dateToParse)
    {
        if (dateToParse.ToString() is not { Length: 8 }) return null;
        var year = dateToParse / 10000;
        var month = (dateToParse / 100) % 100;
        var day = dateToParse % 100;
        if (year is null || month is null || day is null) return null;
        return new DateTime((int)year, (int)month, (int)day);
    }
    
    private async Task BulkTitlesInserts(List<Title> titleEntities, 
        Dictionary<string, List<string>> regionTitlesTitleId, Dictionary<string, int> regionDictionary, 
        Dictionary<string, int> categoryLanguageDictionary,  Dictionary<string, List<string>> titleCategories, 
        Dictionary<string, List<string>> titlesWithLanguages, Dictionary<string, List<TitleDbCnmt>> titlesWithCnmts,
        Dictionary<string, List<Version>> titlesWithVersions, Dictionary<string, List<string>> titlesWithScreenshots,
        Dictionary<string, List<string>> titlesWithRatingContent, Dictionary<string, List<TitleDbEdition>> titlesWithEditions)
    {
        
        await FetchData();
        var regionTitles = new List<TitleRegion>();
        var categoryTitles = new List<TitleCategory>();
        var titleLanguages = new List<TitleLanguage>();
        var titleCnmts = new List<Cnmt>();
        var titleVersions = new List<Version>();
        var titleScreenshots = new List<Screenshot>();
        var titleRatingContents = new List<TitleRatingContent>();
        var titleEditions = new List<Edition>();
        
        // Bulk insert titles
        await context.BulkInsertAsync(titleEntities, new BulkConfig() { SetOutputIdentity = true, PreserveInsertOrder = true });
        var titleDictionary = titleEntities.ToDictionary(t => t.ApplicationId, t => t.Id);
        
        foreach (var kvp in titleDictionary)
        {
            _titlesApplicationIdMap.TryAdd(kvp.Key, kvp.Value);
        }
        
        // Bulk insert versions
        titleVersions.AddRange(titlesWithVersions
            .Where(kvp => _titlesApplicationIdMap.TryGetValue(kvp.Key, out _))
            .SelectMany(kvp => kvp.Value.Select(version => { version.TitleId = _titlesApplicationIdMap[kvp.Key]; return version; })));        
        await context.BulkInsertAsync(titleVersions, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });
        
        // Bulk insert rating contents
        titleRatingContents.AddRange(titlesWithRatingContent.
            SelectMany(x => x.Value
                .Where(y => _ratingContentTable.TryGetValue(y, out _))
                .Select(z => new TitleRatingContent { RatingContentId = _ratingContentTable[z], TitleId = _titlesApplicationIdMap[x.Key] })));
        await context.BulkInsertAsync(titleRatingContents, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });        
        
        // Bulk insert screenshots
        titleScreenshots.AddRange(titlesWithScreenshots
            .Where(kvp => _titlesApplicationIdMap.TryGetValue(kvp.Key, out _))
            .SelectMany(kvp => kvp.Value.Select(s => new Screenshot { TitleId = _titlesApplicationIdMap[kvp.Key], Url = s })));
        await context.BulkInsertAsync(titleScreenshots, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });

        // Bulk insert Editions
        foreach (var (key, titleDbEdition) in titlesWithEditions)
        {
            var exist = _titlesApplicationIdMap.TryGetValue(key, out var titleId);
            if (exist)
            {
                titleEditions.AddRange(titleDbEdition.Select(e => new Edition()
                {
                    TitleId = titleId,
                    ApplicationId = e.Id,
                    BannerUrl = e.BannerUrl,
                    Description = e.Description,
                    NsuId = e.NsuId,
                    ReleaseDate = ParseIntDate(e.ReleaseDate),
                    Size = e.Size,
                    TitleName = e.Name
                }) );
            }
        }
        await context.BulkInsertAsync(titleEditions, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });

        
        // Bulk insert cnmts
        foreach (var (key, value) in titlesWithCnmts)
        {
            var exist = _titlesApplicationIdMap.TryGetValue(key, out var titleId);
            if (exist)
            {
                titleCnmts.AddRange(value.Select(cnmt => new Cnmt()
                {
                    TitleId = titleId,
                    OtherApplicationId = cnmt.OtherApplicationId,
                    RequiredApplicationVersion = cnmt.RequiredApplicationVersion,
                    TitleType = cnmt.TitleType,
                    Version = cnmt.Version
                }));
            }
        }
        await context.BulkInsertAsync(titleCnmts, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });

        // Bulk insert region titles
        regionTitles.AddRange(regionTitlesTitleId.Keys.SelectMany(titleEntity => 
            regionTitlesTitleId[titleEntity].Where(region => regionDictionary.TryGetValue(region, out _))
                .Select(region => new TitleRegion() { RegionId = regionDictionary[region], TitleId = _titlesApplicationIdMap[titleEntity] })));
        await context.BulkInsertAsync(regionTitles, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });

        // Bulk insert category titles
        categoryTitles.AddRange(titleCategories.SelectMany(titleCategory => 
            titleCategory.Value.Where(category => categoryLanguageDictionary.TryGetValue(category, out _))
                .Select(category => new TitleCategory { CategoryId = categoryLanguageDictionary[category], TitleId = _titlesApplicationIdMap[titleCategory.Key] })));
        await context.BulkInsertAsync(categoryTitles, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true }); 
        
        // Bulk insert title languages
        titleLanguages.AddRange(titlesWithLanguages.SelectMany(titleLanguage => 
            titleLanguage.Value.Where(language => _languageTable.TryGetValue(language, out _))
                .Select(language => new TitleLanguage { LanguageId = _languageTable[language], TitleId = _titlesApplicationIdMap[titleLanguage.Key] })));
        await context.BulkInsertAsync(titleLanguages, new BulkConfig() { SetOutputIdentity = false, PreserveInsertOrder = true });
        
        titleEntities.Clear();
        regionTitles.Clear();
        regionTitlesTitleId.Clear();
        categoryTitles.Clear();
        titleCategories.Clear();
        titlesWithLanguages.Clear();
        titlesWithCnmts.Clear();
        titlesWithVersions.Clear();
        titlesWithScreenshots.Clear();
        titlesWithRatingContent.Clear();
        titlesWithEditions.Clear();
    }
    
    public async Task<ICollection<Region>> GetRegionsAsync()
    {
        return await context.Regions.Include(region => region.Languages).ToListAsync();
    }

    public async Task<bool> AddDbHistory()
    {
        var history = new History
        {
            VersionNumber = Guid.NewGuid().ToString("n"),
            TimeStamp = DateTime.Now,
            TitleCount = context.Titles.Count(),
            BaseCount = context.Titles.Count(t => t.ContentType == TitleContentType.Base),
            UpdateCount = context.Titles.Count(t => t.ContentType == TitleContentType.Update),
            DlcCount = context.Titles.Count(t => t.ContentType == TitleContentType.DLC),
        };
        
        context.History.Add(history);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<History?> GetLatestHistoryAsync()
    {
        return await context.History.OrderByDescending(t => t.TimeStamp).FirstOrDefaultAsync();
    }

    private static Title MapTitle(TitleDbTitle title)
    {
        var verSuccess = uint.TryParse(title.Version, out var latestVersion) ? latestVersion : 0;
        var newTitle = new Title
        {
            
            NsuId = title.NsuId,
            ApplicationId = title.Id,
            TitleName = title.Name,
            Region = title.Region,
            IconUrl = title.IconUrl,
            Intro = title.Intro,
            IsDemo = title.IsDemo,
            BannerUrl = title.BannerUrl,
            Developer = title.Developer,
            Publisher = title.Publisher,
            LatestVersion = latestVersion,
            Description = title.Description,
            Rating = title.Rating,
            NumberOfPlayers = title.NumberOfPlayers,
            Size = title.Size,
            OtherApplicationId = title.OtherApplicationId,
            ContentType = TitleContentType.Unknown,
            UpdatesCount = title.PatchCount,
            DlcCount = title.DlcCount
        };
        
        if (title.ReleaseDate is not null && title.ReleaseDate.ToString() is { Length: 8 })
        {
            var year = title.ReleaseDate / 10000;
            var month = (title.ReleaseDate / 100) % 100;
            var day = title.ReleaseDate % 100;
            newTitle.ReleaseDate = new DateTime((int)year, (int)month, (int)day);    
        }
        
        if (title.IsBase)
        {
            newTitle.ContentType = TitleContentType.Base;
        }
        
        if (title.IsUpdate)
        {
            newTitle.ContentType = TitleContentType.Update;
        }
        
        if (title.IsDlc)
        {
            newTitle.ContentType = TitleContentType.DLC;
        }        
        return newTitle;
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