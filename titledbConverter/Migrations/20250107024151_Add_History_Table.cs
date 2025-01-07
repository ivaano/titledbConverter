using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace titledbConverter.Migrations
{
    /// <inheritdoc />
    public partial class Add_History_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionNumber = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    VersionDate = table.Column<string>(type: "VARCHAR", maxLength: 15, nullable: false),
                    TitleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdateCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DlcCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "History");
        }
    }
}
