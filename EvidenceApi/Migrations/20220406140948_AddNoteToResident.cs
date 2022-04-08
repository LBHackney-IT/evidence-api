using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class AddNoteToResident : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note_to_resident",
                table: "evidence_requests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note_to_resident",
                table: "evidence_requests");
        }
    }
}
