﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using titledbConverter.Models;
using Region = titledbConverter.Models.Region;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Data;

public class SqliteDbContext : DbContext
{
   public DbSet<Title> Titles { get; set; }
   public DbSet<Cnmt> Cnmts { get; set; }
   public DbSet<Version> Versions { get; set; }
   public DbSet<Region> Regions { get; set; }
   public DbSet<Category> Categories { get; set; }
   public DbSet<CategoryLanguage> CategoryLanguages { get; set; }
   
   public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
   {
   }
   
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       if (!optionsBuilder.IsConfigured)
       {
           optionsBuilder.UseSqlite("Data Source=titles.db");    
       }
   }
   
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<Title>()
           .HasMany(e => e.Cnmts)
           .WithOne(e => e.Title)
           .HasForeignKey(e => e.TitleId)
           .HasPrincipalKey(e => e.Id);
       
       modelBuilder.Entity<Title>()
           .HasMany(e => e.Versions)
           .WithOne(e => e.Title)
           .HasForeignKey(e => e.TitleId)
           .HasPrincipalKey(e => e.Id);

       modelBuilder.Entity<Title>()
           .HasMany(e => e.Regions)
           .WithMany(e => e.Titles)
           .UsingEntity<RegionTitle>();
      
       modelBuilder.Entity<Title>()
           .HasMany(e => e.Categories)
           .WithMany(e => e.Titles)
           .UsingEntity<CategoryTitle>();

       modelBuilder.Entity<Category>()
           .HasMany(e => e.Languages)
           .WithOne(e => e.Category)
           .HasForeignKey(e => e.CategoryId);

       var countryLanguagesJson = File.ReadAllText("/home/ivan/titledb/languages.json");
       var countryLanguages = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(countryLanguagesJson);
       var regions = countryLanguages.Keys;
       
       //var regions = new string[] {"AR", "AU", "BG", "BR", "CA", "CO", "CH", "CL", "CY",  "DE", "EE", "FR", "HR", "IE", "IT", "LT", "LU", "LV", "MT", "RO", "SI", "SK", "JP", "PE", "KR", "HK", "CN", "NZ", "AT", "BE", "CZ", "DK", "ES", "FI", "GR", "HU", "NL", "NO", "PL", "PT", "RU", "ZA", "SE", "GB", "MX", "US"};
       var regionObjects = regions.OrderBy(r => r)
           .Select((r, i) => new Region { Id = i + 1, Name = r })
           .ToArray();

       modelBuilder.Entity<Region>().HasData(regionObjects);

   }
}