using titledbConverter.Models;

namespace titledbConverter.Services.Interface;

public interface IDbService
{
    Task<int> AddTitleAsync(Title title);
    Task BulkInsertTitlesAsync(List<Title> titles);
    public List<Region> GetRegions();
}