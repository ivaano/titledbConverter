using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace titledbConverter.Migrations
{
    /// <inheritdoc />
    public partial class add_NswReleaseTitles_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NswReleaseTitles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    TitleName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: false),
                    Revision = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    Publisher = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: true),
                    Region = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: true),
                    Languages = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    Firmware = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NswReleaseTitles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NswReleaseTitles");
        }
    }
}
