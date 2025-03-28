﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using titledbConverter.Models;
using titledbConverter.Settings;
using Region = titledbConverter.Models.Region;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Data;

public class SqliteDbContext : DbContext
{
    private readonly AppSettings _configuration;
    public DbSet<Title> Titles { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }
    public DbSet<Cnmt> Cnmts { get; set; }
    public DbSet<Edition> Editions { get; set; }
    public DbSet<Version> Versions { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<CategoryLanguage> CategoryLanguages { get; set; }
    public DbSet<TitleLanguage> TitleLanguages { get; set; }

    public DbSet<RatingContent> RatingContents { get; set; }
    public DbSet<TitleRatingContent> TitleRatingContents { get; set; }
    public DbSet<History> History { get; set; }
    
    public DbSet<NswReleaseTitle> NswReleaseTitles { get; set; }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options, IOptions<AppSettings> configuration) :
        base(options)
    {
        _configuration = configuration.Value;
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
        modelBuilder.Entity<Category>()
            .HasMany(e => e.Languages)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);
        
        modelBuilder.Entity<Region>()
            .HasMany(e => e.Languages)
            .WithMany(e => e.Regions)
            .UsingEntity<RegionLanguage>();
        
        modelBuilder.Entity<Screenshot>()
            .HasOne<Title>(s => s.Title)
            .WithMany(t => t.Screenshots)
            .HasForeignKey(s => s.TitleId);
        
        modelBuilder.Entity<Screenshot>()
            .HasOne<Edition>(s => s.Edition)
            .WithMany(t => t.Screenshots)
            .HasForeignKey(s => s.EditionId);        
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Categories)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleCategory>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Editions)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Cnmts)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Languages)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleLanguage>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Regions)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleRegion>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Versions)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.RatingContents)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleRatingContent>();
        

        var countryLanguagesJson = File.ReadAllText(Path.Join(_configuration.DownloadPath, "languages.json"));
        var countryLanguages = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(countryLanguagesJson);
        if (countryLanguages is null) throw new InvalidOperationException("Unable to parse languages.json");
        
        var regionId = 0;
        var langId = 0;
        var uniqueLanguages = new Dictionary<string, int>();
        foreach (var region in countryLanguages.Keys)
        {
            regionId++;
            var regionObject = new Region {Id = regionId, Name = region };
            modelBuilder.Entity<Region>().HasData(regionObject);
            
            var languages = countryLanguages[region];
            foreach (var language in languages)
            {
                if (!uniqueLanguages.TryGetValue(language, out var existingLangId))
                {
                    langId++;
                    uniqueLanguages.Add(language, langId);
                    modelBuilder.Entity<Language>().HasData(new Language { Id = langId, LanguageCode = language });
                    existingLangId = langId;
                }
                modelBuilder.Entity<RegionLanguage>().HasData(new RegionLanguage { RegionId = regionObject.Id, LanguageId = existingLangId });
            }

        }
    }
}