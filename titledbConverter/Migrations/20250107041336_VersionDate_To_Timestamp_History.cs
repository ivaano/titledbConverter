using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace titledbConverter.Migrations
{
    /// <inheritdoc />
    public partial class VersionDate_To_Timestamp_History : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersionDate",
                table: "History");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "History",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "History");

            migrationBuilder.AddColumn<string>(
                name: "VersionDate",
                table: "History",
                type: "VARCHAR",
                maxLength: 15,
                nullable: false,
                defaultValue: "");
        }
    }
}
