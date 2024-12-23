using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public sealed class Category
{
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public required string Name { get; set; }
    
    public ICollection<CategoryLanguage> Languages { get; set; } = null!;
    public ICollection<Title> Titles { get; } = [];
}