namespace titledbConverter.Services.Interface;

public interface ICategoryLanguageClassifier
{
    public Task ClassifyCategoryLanguageAsync(string region, string language, string name);
}