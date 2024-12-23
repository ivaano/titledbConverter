using System.Text.Json.Serialization;

namespace titledbConverter.Models.Dto;

public class TitleDbEdition
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("nsuId")]
    public long NsuId { get; set; }    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("screenshots")]
    public List<string>? Screenshots { get; set; }
    [JsonPropertyName("releaseDate")]
    public int? ReleaseDate { get; set; }
    [JsonPropertyName("size")]
    public long? Size { get; set; }    
}