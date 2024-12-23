using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("\t")]
[CultureInfo("InvariantCulture")]
[HasHeaderRecord(true)]
public class CategoryRegionLanguage
{
    [Name("original")]
    public required string Original { get; set; }
    
    [Name("translated")]
    public required string Translated { get; set; }

}
