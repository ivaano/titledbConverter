using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public class Category
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public string Name { get; set; }
    [StringLength(2)]
    public string? De { get; set; }
    [StringLength(2)]
    public string? En { get; set; }
    [StringLength(2)]
    public string? Es { get; set; }
    [StringLength(2)]
    public string? Fr { get; set; }
    [StringLength(2)]
    public string? It { get; set; }
    [StringLength(2)]
    public string? Ja { get; set; }
    [StringLength(2)]
    public string? Ko { get; set; }
    [StringLength(2)]
    public string? Nl { get; set; }
    [StringLength(2)]
    public string? Pt { get; set; }
    [StringLength(2)]
    public string? Ru { get; set; }
    [StringLength(2)]
    public string? Zh { get; set; }
    
    public ICollection<Title> Titles { get; } = [];
}