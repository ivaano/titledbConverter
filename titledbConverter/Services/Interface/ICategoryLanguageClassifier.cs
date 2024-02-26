namespace titledbConverter.Services.Interface;

public interface ICategoryLanguageClassifier
{
    public Task ClassifyCategoryLanguageAsync(string Region, string Language, string Name);
}