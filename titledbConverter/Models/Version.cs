using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public class Version
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(15)]
    public required string VersionDate { get; set; }
    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;
}