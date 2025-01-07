﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using titledbConverter.Data;

#nullable disable

namespace titledbConverter.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20250107041336_VersionDate_To_Timestamp_History")]
    partial class VersionDate_To_Timestamp_History
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("titledbConverter.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("titledbConverter.Models.CategoryLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("CategoryLanguages");
                });

            modelBuilder.Entity("titledbConverter.Models.Cnmt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("OtherApplicationId")
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("RequiredApplicationVersion")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TitleId");

                    b.ToTable("Cnmts");
                });

            modelBuilder.Entity("titledbConverter.Models.Edition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("BannerUrl")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Description")
                        .HasMaxLength(5000)
                        .HasColumnType("TEXT");

                    b.Property<long?>("NsuId")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<long?>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TitleName")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.HasIndex("TitleId");

                    b.ToTable("Editions");
                });

            modelBuilder.Entity("titledbConverter.Models.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BaseCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DlcCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<int>("TitleCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UpdateCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionNumber")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("History");
                });

            modelBuilder.Entity("titledbConverter.Models.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Languages");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            LanguageCode = "en"
                        },
                        new
                        {
                            Id = 2,
                            LanguageCode = "pt"
                        },
                        new
                        {
                            Id = 3,
                            LanguageCode = "fr"
                        },
                        new
                        {
                            Id = 4,
                            LanguageCode = "de"
                        },
                        new
                        {
                            Id = 5,
                            LanguageCode = "it"
                        },
                        new
                        {
                            Id = 6,
                            LanguageCode = "es"
                        },
                        new
                        {
                            Id = 7,
                            LanguageCode = "ko"
                        },
                        new
                        {
                            Id = 8,
                            LanguageCode = "zh"
                        },
                        new
                        {
                            Id = 9,
                            LanguageCode = "nl"
                        },
                        new
                        {
                            Id = 10,
                            LanguageCode = "ru"
                        },
                        new
                        {
                            Id = 11,
                            LanguageCode = "ja"
                        });
                });

            modelBuilder.Entity("titledbConverter.Models.RatingContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("RatingContents");
                });

            modelBuilder.Entity("titledbConverter.Models.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Regions");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "BG"
                        },
                        new
                        {
                            Id = 2,
                            Name = "BR"
                        },
                        new
                        {
                            Id = 3,
                            Name = "CH"
                        },
                        new
                        {
                            Id = 4,
                            Name = "CY"
                        },
                        new
                        {
                            Id = 5,
                            Name = "EE"
                        },
                        new
                        {
                            Id = 6,
                            Name = "HR"
                        },
                        new
                        {
                            Id = 7,
                            Name = "IE"
                        },
                        new
                        {
                            Id = 8,
                            Name = "LT"
                        },
                        new
                        {
                            Id = 9,
                            Name = "LU"
                        },
                        new
                        {
                            Id = 10,
                            Name = "LV"
                        },
                        new
                        {
                            Id = 11,
                            Name = "MT"
                        },
                        new
                        {
                            Id = 12,
                            Name = "RO"
                        },
                        new
                        {
                            Id = 13,
                            Name = "SI"
                        },
                        new
                        {
                            Id = 14,
                            Name = "SK"
                        },
                        new
                        {
                            Id = 15,
                            Name = "CO"
                        },
                        new
                        {
                            Id = 16,
                            Name = "AR"
                        },
                        new
                        {
                            Id = 17,
                            Name = "CL"
                        },
                        new
                        {
                            Id = 18,
                            Name = "PE"
                        },
                        new
                        {
                            Id = 19,
                            Name = "KR"
                        },
                        new
                        {
                            Id = 20,
                            Name = "HK"
                        },
                        new
                        {
                            Id = 21,
                            Name = "CN"
                        },
                        new
                        {
                            Id = 22,
                            Name = "NZ"
                        },
                        new
                        {
                            Id = 23,
                            Name = "AT"
                        },
                        new
                        {
                            Id = 24,
                            Name = "BE"
                        },
                        new
                        {
                            Id = 25,
                            Name = "CZ"
                        },
                        new
                        {
                            Id = 26,
                            Name = "DK"
                        },
                        new
                        {
                            Id = 27,
                            Name = "ES"
                        },
                        new
                        {
                            Id = 28,
                            Name = "FI"
                        },
                        new
                        {
                            Id = 29,
                            Name = "GR"
                        },
                        new
                        {
                            Id = 30,
                            Name = "HU"
                        },
                        new
                        {
                            Id = 31,
                            Name = "NL"
                        },
                        new
                        {
                            Id = 32,
                            Name = "NO"
                        },
                        new
                        {
                            Id = 33,
                            Name = "PL"
                        },
                        new
                        {
                            Id = 34,
                            Name = "PT"
                        },
                        new
                        {
                            Id = 35,
                            Name = "RU"
                        },
                        new
                        {
                            Id = 36,
                            Name = "ZA"
                        },
                        new
                        {
                            Id = 37,
                            Name = "SE"
                        },
                        new
                        {
                            Id = 38,
                            Name = "MX"
                        },
                        new
                        {
                            Id = 39,
                            Name = "IT"
                        },
                        new
                        {
                            Id = 40,
                            Name = "CA"
                        },
                        new
                        {
                            Id = 41,
                            Name = "FR"
                        },
                        new
                        {
                            Id = 42,
                            Name = "DE"
                        },
                        new
                        {
                            Id = 43,
                            Name = "JP"
                        },
                        new
                        {
                            Id = 44,
                            Name = "AU"
                        },
                        new
                        {
                            Id = 45,
                            Name = "GB"
                        },
                        new
                        {
                            Id = 46,
                            Name = "US"
                        });
                });

            modelBuilder.Entity("titledbConverter.Models.RegionLanguage", b =>
                {
                    b.Property<int>("LanguageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RegionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LanguageId", "RegionId");

                    b.HasIndex("RegionId");

                    b.ToTable("RegionLanguage");

                    b.HasData(
                        new
                        {
                            LanguageId = 1,
                            RegionId = 1
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 2
                        },
                        new
                        {
                            LanguageId = 2,
                            RegionId = 2
                        },
                        new
                        {
                            LanguageId = 3,
                            RegionId = 3
                        },
                        new
                        {
                            LanguageId = 4,
                            RegionId = 3
                        },
                        new
                        {
                            LanguageId = 5,
                            RegionId = 3
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 4
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 5
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 6
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 7
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 8
                        },
                        new
                        {
                            LanguageId = 3,
                            RegionId = 9
                        },
                        new
                        {
                            LanguageId = 4,
                            RegionId = 9
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 10
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 11
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 12
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 13
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 14
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 15
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 15
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 16
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 16
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 17
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 17
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 18
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 18
                        },
                        new
                        {
                            LanguageId = 7,
                            RegionId = 19
                        },
                        new
                        {
                            LanguageId = 8,
                            RegionId = 20
                        },
                        new
                        {
                            LanguageId = 8,
                            RegionId = 21
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 22
                        },
                        new
                        {
                            LanguageId = 4,
                            RegionId = 23
                        },
                        new
                        {
                            LanguageId = 3,
                            RegionId = 24
                        },
                        new
                        {
                            LanguageId = 9,
                            RegionId = 24
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 25
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 26
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 27
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 28
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 29
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 30
                        },
                        new
                        {
                            LanguageId = 9,
                            RegionId = 31
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 32
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 33
                        },
                        new
                        {
                            LanguageId = 2,
                            RegionId = 34
                        },
                        new
                        {
                            LanguageId = 10,
                            RegionId = 35
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 36
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 37
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 38
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 38
                        },
                        new
                        {
                            LanguageId = 5,
                            RegionId = 39
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 40
                        },
                        new
                        {
                            LanguageId = 3,
                            RegionId = 40
                        },
                        new
                        {
                            LanguageId = 3,
                            RegionId = 41
                        },
                        new
                        {
                            LanguageId = 4,
                            RegionId = 42
                        },
                        new
                        {
                            LanguageId = 11,
                            RegionId = 43
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 44
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 45
                        },
                        new
                        {
                            LanguageId = 1,
                            RegionId = 46
                        },
                        new
                        {
                            LanguageId = 6,
                            RegionId = 46
                        });
                });

            modelBuilder.Entity("titledbConverter.Models.Screenshot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EditionId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.HasIndex("EditionId");

                    b.HasIndex("TitleId");

                    b.ToTable("Screenshots");
                });

            modelBuilder.Entity("titledbConverter.Models.Title", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("BannerUrl")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<byte>("ContentType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(5000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Developer")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("DlcCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("IconUrl")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Intro")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<bool>("IsDemo")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LatestVersion")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("NsuId")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("NumberOfPlayers")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OtherApplicationId")
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Publisher")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("Rating")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Region")
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<long?>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TitleName")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("UpdatesCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Titles");
                });

            modelBuilder.Entity("titledbConverter.Models.TitleCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CategoryId", "TitleId");

                    b.HasIndex("TitleId");

                    b.ToTable("TitleCategory");
                });

            modelBuilder.Entity("titledbConverter.Models.TitleLanguage", b =>
                {
                    b.Property<int>("LanguageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LanguageId", "TitleId");

                    b.HasIndex("TitleId");

                    b.ToTable("TitleLanguages");
                });

            modelBuilder.Entity("titledbConverter.Models.TitleRatingContent", b =>
                {
                    b.Property<int>("RatingContentId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RatingContentId", "TitleId");

                    b.HasIndex("TitleId");

                    b.ToTable("TitleRatingContents");
                });

            modelBuilder.Entity("titledbConverter.Models.TitleRegion", b =>
                {
                    b.Property<int>("RegionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RegionId", "TitleId");

                    b.HasIndex("TitleId");

                    b.ToTable("TitleRegion");
                });

            modelBuilder.Entity("titledbConverter.Models.Version", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionDate")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("VARCHAR");

                    b.Property<int>("VersionNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TitleId");

                    b.ToTable("Versions");
                });

            modelBuilder.Entity("titledbConverter.Models.CategoryLanguage", b =>
                {
                    b.HasOne("titledbConverter.Models.Category", "Category")
                        .WithMany("Languages")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("titledbConverter.Models.Cnmt", b =>
                {
                    b.HasOne("titledbConverter.Models.Title", "Title")
                        .WithMany("Cnmts")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Title");
                });

            modelBuilder.Entity("titledbConverter.Models.Edition", b =>
                {
                    b.HasOne("titledbConverter.Models.Title", "Title")
                        .WithMany("Editions")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Title");
                });

            modelBuilder.Entity("titledbConverter.Models.RegionLanguage", b =>
                {
                    b.HasOne("titledbConverter.Models.Language", null)
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("titledbConverter.Models.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("titledbConverter.Models.Screenshot", b =>
                {
                    b.HasOne("titledbConverter.Models.Edition", "Edition")
                        .WithMany("Screenshots")
                        .HasForeignKey("EditionId");

                    b.HasOne("titledbConverter.Models.Title", "Title")
                        .WithMany("Screenshots")
                        .HasForeignKey("TitleId");

                    b.Navigation("Edition");

                    b.Navigation("Title");
                });

            modelBuilder.Entity("titledbConverter.Models.TitleCategory", b =>
                {
                    b.HasOne("titledbConverter.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("titledbConverter.Models.Title", null)
                        .WithMany()
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("titledbConverter.Models.TitleLanguage", b =>
                {
                    b.HasOne("titledbConverter.Models.Language", null)
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("titledbConverter.Models.Title", null)
                        .WithMany()
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("titledbConverter.Models.TitleRatingContent", b =>
                {
                    b.HasOne("titledbConverter.Models.RatingContent", null)
                        .WithMany()
                        .HasForeignKey("RatingContentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("titledbConverter.Models.Title", null)
                        .WithMany()
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("titledbConverter.Models.TitleRegion", b =>
                {
                    b.HasOne("titledbConverter.Models.Region", null)
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("titledbConverter.Models.Title", null)
                        .WithMany()
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("titledbConverter.Models.Version", b =>
                {
                    b.HasOne("titledbConverter.Models.Title", "Title")
                        .WithMany("Versions")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Title");
                });

            modelBuilder.Entity("titledbConverter.Models.Category", b =>
                {
                    b.Navigation("Languages");
                });

            modelBuilder.Entity("titledbConverter.Models.Edition", b =>
                {
                    b.Navigation("Screenshots");
                });

            modelBuilder.Entity("titledbConverter.Models.Title", b =>
                {
                    b.Navigation("Cnmts");

                    b.Navigation("Editions");

                    b.Navigation("Screenshots");

                    b.Navigation("Versions");
                });
#pragma warning restore 612, 618
        }
    }
}
