using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("\t")]
[CultureInfo("InvariantCulture")]
[HasHeaderRecord(true)]
public class CategoryRegionLanguage
{
    [Name("original")]
    public string Original { get; set; }
    [Name("translated")]
    public string Translated { get; set; }

}
