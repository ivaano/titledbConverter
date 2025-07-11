﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public class CategoryLanguage
{
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public required string Region { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public required string Language { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public required string Name { get; set; }
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }
}