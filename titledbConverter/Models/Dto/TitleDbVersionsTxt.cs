using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("|")]
[CultureInfo("InvariantCulture")]
public class TitleDbVersionsTxt
{
    [Name("id")]
    public required string Id { get; set; }
    
    [Name("rightsId")]
    public required string RightsId { get; set; }
    
    [Name("version")]
    public required string Version { get; set; }
}
