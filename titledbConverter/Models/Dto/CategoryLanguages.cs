using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("\t")]
[CultureInfo("InvariantCulture")]
public class CategoryLanguages
{
    [Name("original")]
    public string Category { get; set; }
    [Name("translated")] 
    public string Translated { get; set; }
}