using System.Collections.Immutable;
using CsvHelper;
using CsvHelper.Configuration;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;

namespace titledbConverter.Services;

public class CategoryLanguageClassifier : ICategoryLanguageClassifier
{

    private readonly ImmutableHashSet<string> _knownCategories;
    
    public CategoryLanguageClassifier()
    {
        var englishCategories = LoadLanguageMap("US", "en");
        _knownCategories = (englishCategories ?? throw new InvalidOperationException("Unable to get Us.en categories.")).Select(c => c.Category).ToImmutableHashSet();
    }
    
    private IEnumerable<CategoryLanguages>? LoadLanguageMap(string region, string language)
    {
        var filePath = Path.Join(Directory.GetCurrentDirectory(), "Datasets", $"categories.{region}.{language}.tsv");
        using var reader = new StreamReader(filePath);
        var config = CsvConfiguration.FromAttributes<CategoryLanguages>();
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<CategoryLanguages>();
        return records.ToList();
    }
    
    public async Task ClassifyCategoryLanguageAsync(string region, string language, string name)
    {
       var map = LoadLanguageMap(region, language);
       var caco = map?.FirstOrDefault(c => c.Category == name);
       var pe = 1;
    }
}