using System.Collections;
using titledbConverter.Models;
using titledbConverter.Models.Dto;

namespace titledbConverter.Services.Interface;

public interface IDbService
{
    Task<int> AddTitleAsync(Title title);
    Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles);
    public Task<Dictionary<string, Category>> GetCategoriesAsDict();

    public Task<bool> SaveCategoryLanguages(IEnumerable<CategoryLanguage> categoryLanguages);
    public Task<ICollection<Region>> GetRegionsAsync();
    public Task ImportTitles(IEnumerable<TitleDbTitle> titles);
    public Task ImportTitle(TitleDbTitle title);
    public Task ImportTitlesCategories(IEnumerable<TitleDbTitle> titles);
}