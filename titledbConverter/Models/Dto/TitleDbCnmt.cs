using System.Text.Json.Serialization;
using titledbConverter.Utils;

namespace titledbConverter.Models.Dto;

public class TitleDbCnmt
{
    [JsonPropertyName("contentEntries")]
    public List<ContentEntry>? ContentEntries { get; set; }
    public List<MetaEntry>? MetaEntries { get; set; }
    
    [JsonPropertyName("otherApplicationId")]
    [JsonConverter(typeof(UppercaseJsonConverter))]
    public string? OtherApplicationId { get; set; }
    
    [JsonPropertyName("requiredApplicationVersion")] 
    public int? RequiredApplicationVersion { get; set; }
    
    [JsonPropertyName("requiredSystemVersion")]
    public int? RequiredSystemVersion { get; set; }
    
    [JsonPropertyName("titleId")]
    [JsonConverter(typeof(UppercaseJsonConverter))]
    public string? TitleId { get; set; }
    
    [JsonPropertyName("titleType")]
    public int TitleType { get; set; }
    
    [JsonPropertyName("version")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Version { get; set; }
    
}

public class ContentEntry
{
    [JsonPropertyName("buildId")]
    public string BuildId { get; set; }
    
    [JsonPropertyName("ncaId")]
    public string NcaId { get; set; }
    
    [JsonPropertyName("type")]
    public int Type { get; set; }
}

public class MetaEntry
{
}