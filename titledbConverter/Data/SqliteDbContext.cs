using Microsoft.EntityFrameworkCore;
using titledbConverter.Models;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Data;

public class SqliteDbContext : DbContext
{
   public DbSet<Title> Titles { get; set; }
   public DbSet<Cnmt> Cnmts { get; set; }
   public DbSet<Version> Versions { get; set; }
   
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
   }
}