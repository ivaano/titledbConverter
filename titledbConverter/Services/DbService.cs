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
    public Task<int> AddTitleAsync(Title title)
    {
        context.Titles.Add(title);
        return context.SaveChangesAsync();
    }
    
    
    
    public async Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles)
    {
        var stopwatch = Stopwatch.StartNew();
        var regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
        var titleEntities = new List<Title>();
        var regionTitles = new List<RegionTitle>();
        var regionTitlesTitleId = new Dictionary<string, List<string>>();

        foreach (var title in titles)
        {
            var mappedTitle = MapTitle(title);
            if (title.Regions is { Count: > 0 })
            {
                mappedTitle.Regions = new List<Region>();
                regionTitlesTitleId.Add(title.Id, title.Regions);
            }

            if (title.Category is { Count: > 0 })
            {
                mappedTitle.Categories = new List<Category>();
            }


            titleEntities.Add(mappedTitle);

            if (titleEntities.Count >= 1000)
            {
                await BulkInsertAndUpdate(titleEntities, regionTitles, regionTitlesTitleId, regionDictionary);
            }
        }

        if (titleEntities.Count > 0)
        {
            await BulkInsertAndUpdate(titleEntities, regionTitles, regionTitlesTitleId, regionDictionary);
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

    private async Task BulkInsertAndUpdate(List<Title> titleEntities, List<RegionTitle> regionTitles,
        Dictionary<string, List<string>> regionTitlesTitleId, Dictionary<string, int> regionDictionary)
    {
        await context.BulkInsertAsync(titleEntities, new BulkConfig() { SetOutputIdentity = true, PreserveInsertOrder = true });

        //regions
        var titleDictionary = titleEntities.ToDictionary(t => t.ApplicationId, t => t.Id);
        foreach (var titleEntity in regionTitlesTitleId.Keys)
        {
            foreach (var region in regionTitlesTitleId[titleEntity])
            {
                if (regionDictionary.TryGetValue(region, out var regionId))
                {
                    regionTitles.Add(new RegionTitle() { RegionId = regionId, TitleId = titleDictionary[titleEntity] });
                }
            }
        }

        await context.BulkInsertAsync(regionTitles, new BulkConfig() { SetOutputIdentity = true, PreserveInsertOrder = true });

        titleEntities.Clear();
        regionTitles.Clear();
        regionTitlesTitleId.Clear();
    }

    public async Task<ICollection<Region>> GetRegionsAsync()
    {
        return await context.Regions.Include(region => region.Languages).ToListAsync();
    }

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
                        /*
                        var region = new Region() { Id = regionId };
                        //post.Tags.Add(context.Tags.Local.Single(x => x.Id == 1));
                        // Check if the region is already being tracked
                        var local = context.Set<Region>()
                            .Local
                            .FirstOrDefault(entry => entry.Id.Equals(region.Id));
                        // If not, attach it
                        if (local == null)
                        {
                            context.Regions.Attach(region);
                            mappedTitle.Regions.Add(region);
                        }
                        else
                        {
                            // Use the tracked instance instead
                            mappedTitle.Regions.Add(local);
                        }
                        */
                    }
                }
            }

            titleEntities.Add(mappedTitle);
            if (batchCount >= 1000)
            {
                AnsiConsole.MarkupLineInterpolated($"[blue]Saving...[/]");

                //await context.SaveChangesAsync();
                context.Titles.AddRange(titleEntities);
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();
                regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);
                titleEntities.Clear();
                batchCount = 0;
            }
            //context.Titles.Add(mappedTitle);

            //AnsiConsole.MarkupLineInterpolated($"[blue]Importing[/][yellow] {title.Id}[/] - [green]{title.Name}[/]");
        }

        if (batchCount > 0)
        {
            context.Titles.AddRange(titleEntities);
            await context.SaveChangesAsync();
        }

        stopwatch.Stop();
        AnsiConsole.MarkupLine($"[springgreen3_1]Imported all in: {stopwatch.Elapsed.TotalMilliseconds} ms[/]");
    }

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
        /*
                if (title.Regions != null)
                {
                    //var dbRegions = context.Regions.ToDictionary(region => region.Id, region => region.Name);
                    foreach (var titleRegion in title.Regions)
                    {
                        //var region = context.Regions.FirstOrDefault(x => x.Name == titleRegion);
                        //var regionId = dbRegions.FirstOrDefault(x => x.Value == titleRegion).Key;

                        if (regionId > 0)
                        {
                            var region = new Region() { Id = regionId };
                            context.Regions.Attach(region);
                            newTitle.Regions.Add(region);
                        }
                        else
                        {
                            throw  new Exception($"Region {titleRegion} not found");
                        }

                    }
                }
                */


        // await AddTitleAsync(newTitle);
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