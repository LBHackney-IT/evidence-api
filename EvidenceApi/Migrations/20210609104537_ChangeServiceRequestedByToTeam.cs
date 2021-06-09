using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class ChangeServiceRequestedByToTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "service_requested_by",
                table: "evidence_requests",
                newName: "team");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "team",
                table: "evidence_requests",
                newName: "service_requested_by");
        }
    }
}
