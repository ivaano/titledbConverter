﻿using System.Text.Json.Serialization;

namespace titledbConverter.Models.Dto;

public record TitleDbTitle()
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonPropertyName("otherApplicationId")]
    public string? OtherApplicationId { get; set; }
    
    [JsonPropertyName("ids")]
    public List<string>? Ids { get; set; }
    
    [JsonPropertyName("nsuId")]
    public long NsuId { get; set; }    

    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    
    [JsonPropertyName("isBase")]
    public bool IsBase { get; set; } = false;

    [JsonPropertyName("isDlc")]
    public bool IsDlc { get; set; } = false;
    
    [JsonPropertyName("isUpdate")]
    public bool IsUpdate { get; set; } = false;    
    
    [JsonPropertyName("isDemo")]
    public bool IsDemo { get; set; }
    
    [JsonPropertyName("patchCount")]
    public int? PatchCount { get; set; }
    [JsonPropertyName("dlcCount")]
    public int? DlcCount { get; set; }
    [JsonPropertyName("size")]
    public long? Size { get; set; }
    
    [JsonPropertyName("intro")]
    public string? Intro { get; set; }
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
    
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    [JsonPropertyName("languages")]
    public List<string>? Languages { get; set; }

    [JsonPropertyName("numberOfPlayers")]
    public int? NumberOfPlayers { get; set; }
    
    [JsonPropertyName("publisher")] 
    public string? Publisher { get; set; }
    
    [JsonPropertyName("rating")]
    public int? Rating { get; set; }
    
    [JsonPropertyName("ratingContent")]
    public List<string>? RatingContent { get; set; }
    
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("releaseDate")]
    public int? ReleaseDate { get; set; }
    
    [JsonPropertyName("rightsId")]
    public string? RightsId { get; set; }
    
    [JsonPropertyName("screenshots")]
    public List<string>? Screenshots { get; set; }
    
    [JsonPropertyName("editions")]
    public List<TitleDbEdition>? Editions { get; set; }
    
    [JsonPropertyName("regions")]
    public List<string>? Regions { get; set; }
    
    [JsonPropertyName("versions")]
    public List<TitleDbVersions>? Versions { get; set; }
    
    [JsonPropertyName("cnmts")]
    public List<TitleDbCnmt>? Cnmts { get; set; }
    
    [JsonPropertyName("ncas")]
    public List<TitleDbNca>? Ncas { get; set; }

}
