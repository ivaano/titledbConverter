using System.Text.Json.Serialization;

namespace titledbConverter.Models.Dto;

public class TitleDbNca
{
    [JsonPropertyName("ncaId")]
    public string? NcaId { get; set; }
    
    [JsonPropertyName("buildId")]
    public string? BuildId { get; set; }
    [JsonPropertyName("contentIndex")]
    public int ContentIndex { get; set; }
    [JsonPropertyName("contentType")]
    public int ContentType { get; set; }
    [JsonPropertyName("cryptoType")]
    public int CryptoType { get; set; }
    [JsonPropertyName("cryptoType2")]
    public int CryptoType2 { get; set; }
    [JsonPropertyName("isGameCard")]
    public int IsGameCard { get; set; }
    [JsonPropertyName("keyIndex")]
    public int KeyIndex { get; set; }
    [JsonPropertyName("rightsId")]
    public string? RightsId { get; set; }
    [JsonPropertyName("sdkVersion")]
    public int SdkVersion { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("titleId")]
    public string? TitleId { get; set; }
}


