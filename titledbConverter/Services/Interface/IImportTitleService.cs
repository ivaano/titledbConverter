namespace titledbConverter.Services.Interface;

public interface IImportTitleService
{
    public Task ImportTitlesFromFileAsync(string file);
    public Task ImportTitlesCategoriesAsync(string file);
    public Task ImportAllCategories();
}