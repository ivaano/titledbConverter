using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public class RatingContent
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public required string Name { get; set; }
   
    public ICollection<Title> Titles { get; } = [];
}