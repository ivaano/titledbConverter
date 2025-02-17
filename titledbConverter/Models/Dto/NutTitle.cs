﻿using System.Text.Json.Serialization;

namespace titledbConverter.Models.Dto;

public class NutTitle
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("ids")]
    public List<string>? Ids { get; set; }
    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }
    [JsonPropertyName("category")]
    public List<string>? Category { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("developer")]
    public string? Developer { get; set; }
    [JsonPropertyName("frontBoxArt")]
    public string? FrontBoxArt { get; set; }
    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }
    [JsonPropertyName("intro")]
    public string? Intro { get; set; }
    [JsonPropertyName("isDemo")]
    public bool? IsDemo { get; set; }
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    [JsonPropertyName("languages")]
    public List<string>? Languages { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("nsuId")]
    public long? NsuId { get; set; }
    [JsonPropertyName("numberOfPlayers")]
    public int? NumberOfPlayers { get; set; }
    [JsonPropertyName("publisher")] 
    public string? Publisher { get; set; }
    [JsonPropertyName("rank")] 
    public int? Rank { get; set; }
    [JsonPropertyName("rating")]
    public int? Rating { get; set; }
    [JsonPropertyName("ratingContent")]
    public List<string>? RatingContent { get; set; }
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    [JsonPropertyName("regions")]
    public List<string>? Regions { get; set; }
    [JsonPropertyName("releaseDate")]
    public int? ReleaseDate { get; set; }
    [JsonPropertyName("rightsId")]
    public string? RightsId { get; set; }
    [JsonPropertyName("screenshots")]
    public List<string>? Screenshots { get; set; }
    [JsonPropertyName("size")]
    public long? Size { get; set; }
    [JsonPropertyName("Version")]
    public string? Version { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
}