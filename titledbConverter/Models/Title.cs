using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public class Title
{
    public int Id { get; set; }
    public long NsuId { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? ApplicationId { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? TitleName { get; set; }
    
    public virtual ICollection<Cnmt>? Cnmts { get; set; }
    public virtual ICollection<Version>? Versions { get; set; }
}