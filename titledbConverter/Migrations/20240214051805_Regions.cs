using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace titledbConverter.Migrations
{
    /// <inheritdoc />
    public partial class Regions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Titles",
                type: "VARCHAR",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegionTitle",
                columns: table => new
                {
                    RegionsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitlesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionTitle", x => new { x.RegionsId, x.TitlesId });
                    table.ForeignKey(
                        name: "FK_RegionTitle_Regions_RegionsId",
                        column: x => x.RegionsId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionTitle_Titles_TitlesId",
                        column: x => x.TitlesId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "AR" },
                    { 2, "AT" },
                    { 3, "BE" },
                    { 4, "BG" },
                    { 5, "BR" },
                    { 6, "CH" },
                    { 7, "CL" },
                    { 8, "CN" },
                    { 9, "CO" },
                    { 10, "CY" },
                    { 11, "CZ" },
                    { 12, "DK" },
                    { 13, "EE" },
                    { 14, "ES" },
                    { 15, "FI" },
                    { 16, "GR" },
                    { 17, "HK" },
                    { 18, "HR" },
                    { 19, "HU" },
                    { 20, "IE" },
                    { 21, "KR" },
                    { 22, "LT" },
                    { 23, "LU" },
                    { 24, "LV" },
                    { 25, "MT" },
                    { 26, "MX" },
                    { 27, "NL" },
                    { 28, "NO" },
                    { 29, "NZ" },
                    { 30, "PE" },
                    { 31, "PL" },
                    { 32, "PT" },
                    { 33, "RO" },
                    { 34, "RU" },
                    { 35, "SE" },
                    { 36, "SI" },
                    { 37, "SK" },
                    { 38, "US" },
                    { 39, "ZA" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegionTitle_TitlesId",
                table: "RegionTitle",
                column: "TitlesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionTitle");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Titles");
        }
    }
}
