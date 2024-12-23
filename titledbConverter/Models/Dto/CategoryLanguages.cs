using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("\t")]
[CultureInfo("InvariantCulture")]
public class CategoryLanguages
{
    [Name("original")]
    public required string Category { get; set; }
    
    [Name("translated")] 
    public required string Translated { get; set; }
}