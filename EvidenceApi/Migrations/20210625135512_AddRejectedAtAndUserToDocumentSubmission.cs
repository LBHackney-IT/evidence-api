using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class AddRejectedAtAndUserToDocumentSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                table: "document_submissions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "user_updated_by",
                table: "document_submissions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "user_updated_by",
                table: "document_submissions");
        }
    }
}
