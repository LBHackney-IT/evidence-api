using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToResidentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_hidden",
                table: "residents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_hidden",
                table: "residents");
        }
    }
}
