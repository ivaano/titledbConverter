using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public sealed class Title
{
    public int Id { get; init; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public long? NsuId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? BannerUrl { get; init; }
    
    [Column(TypeName = "TEXT")]
    [StringLength(20)]
    public string? Description { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? Developer { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string? Publisher { get; set; }
    
    public int? ReleaseDate { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? TitleName { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public string? Region { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(15)]
    public string? ContentType { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? OtherApplicationId { get; set; }
    public ICollection<Cnmt>? Cnmts { get; init; }
    public ICollection<Version>? Versions { get; init; }
    public ICollection<Language>? Languages { get; set; }
    public ICollection<Region>? Regions { get; set; }
    public ICollection<Category>? Categories { get; set; }
    public ICollection<RatingContent>? RatingContents { get; init; }
    
}