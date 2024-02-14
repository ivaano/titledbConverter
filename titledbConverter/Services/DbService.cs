using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using titledbConverter.Data;
using titledbConverter.Models;
using titledbConverter.Services.Interface;

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
        context.BulkSaveChanges();
        //await context.BulkInsertAsync(titles, options => options.IncludeGraph = true);
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