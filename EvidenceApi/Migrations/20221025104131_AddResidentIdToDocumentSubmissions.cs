using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class AddResidentIdToDocumentSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "resident_id",
                table: "document_submissions",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "resident_id",
                table: "document_submissions");
        }
    }
}
