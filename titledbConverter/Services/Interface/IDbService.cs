using System.Collections;
using FluentResults;
using titledbConverter.Models;
using titledbConverter.Models.Dto;

namespace titledbConverter.Services.Interface;

public interface IDbService
{
    Task<int> AddTitleAsync(Title title);
    Task BulkInsertTitlesAsync(IEnumerable<TitleDbTitle> titles);
    public Task<Result<Dictionary<string, Category>>> GetCategoriesAsDict();
    public Task<Result<Dictionary<string, CategoryLanguage>>> GetCategoriesLanguagesAsDict();
    public Task<Result<int>> SaveCategories(IEnumerable<CategoryLanguage> categoryLanguages);
    public Task<Result<int>> SaveRatingContents(IEnumerable<RatingContent> ratingContents);
    public Task<Result<int>> SaveCategoryLanguages(IEnumerable<CategoryLanguage> categoryLanguages);
    public Task<ICollection<Region>> GetRegionsAsync();
    public Task<bool> AddDbHistory();
    public Task<History?> GetLatestHistoryAsync();
}