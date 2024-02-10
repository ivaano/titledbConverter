using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace titledbConverter.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NsuId = table.Column<long>(type: "INTEGER", nullable: false),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: true),
                    TitleName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cnmts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OtherApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: true),
                    RequiredApplicationVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleType = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cnmts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cnmts_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionDate = table.Column<string>(type: "TEXT", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Versions_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cnmts_TitleId",
                table: "Cnmts",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_TitleId",
                table: "Versions",
                column: "TitleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cnmts");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
