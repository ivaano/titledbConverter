using titledbConverter.Models;
using titledbConverter.Models.Dto;

namespace titledbConverter.Services.Interface;

public interface IDbService
{
    Task<int> AddTitleAsync(Title title);
    Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles);
    public List<Region> GetRegions();
    public Task ImportTitles(IEnumerable<TitleDbTitle> titles);
    public Task ImportTitle(TitleDbTitle title);
}