namespace titledbConverter.Services.Interface;

public interface IImportTitleService
{
    public Task ImportTitlesFromFileAsync(string file);
}