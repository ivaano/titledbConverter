﻿using System.Text.Json.Serialization;

namespace titledbConverter.Models.Dto;

public class TitleDbVersions
{
    [JsonPropertyName("versionNumber")]
    public int VersionNumber { get; set; }
    
    [JsonPropertyName("versionDate")]
    public required string VersionDate { get; set; }
}