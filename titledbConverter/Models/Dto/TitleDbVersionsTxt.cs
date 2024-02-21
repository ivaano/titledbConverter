using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace titledbConverter.Models.Dto;

[Delimiter("|")]
[CultureInfo("InvariantCulture")]
public class TitleDbVersionsTxt
{
    [Name("id")]
    public string Id { get; set; }
    [Name("rightsId")]
    public string RightsId { get; set; }
    [Name("version")]
    public string Version { get; set; }
}
