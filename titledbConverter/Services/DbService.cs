using EFCore.BulkExtensions;
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
    
    public async Task BulkInsertTitlesAsync(List<Title> titles)
    {
        context.Titles.AddRange(titles);
        context.SaveChanges();
        //context.BulkSaveChanges();
        //await context.BulkInsertAsync(titles, options => options.IncludeGraph = true);
    }

    public async Task ImportTitles(IEnumerable<TitleDbTitle> titles)
    {
        var regionDictionary = context.Regions.ToDictionary(region => region.Name, region => region.Id);

        var batchCount = 0;
        var titleEntities = new List<Title>();
        
        foreach (var title in titles)
        {
            batchCount++;
            AnsiConsole.MarkupLineInterpolated($"[blue]Importing[/][yellow] {title.Id}[/] - [green]{title.Name}[/]");

            var mappedTitle = await MapTitle(title);
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
        
    }
    
    private Task<Title> MapTitle(TitleDbTitle title)
    {
        var newTitle = new Title
        {
            NsuId = title.NsuId,
            ApplicationId = title.Id,
            TitleName = title.Name,
            Region = title.Region,
        };
        return Task.FromResult(newTitle);
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
    
    public List<Region> GetRegions()
    {
        return context.Regions.ToList();
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