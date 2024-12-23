using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace titledbConverter.Models;

public class Language
{
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public required string LanguageCode { get; set; }
    public ICollection<Region> Regions { get; } = [];
    public ICollection<Title> Titles { get; } = [];
}