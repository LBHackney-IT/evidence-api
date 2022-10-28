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
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_resident_id",
                table: "document_submissions",
                column: "resident_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_residents_resident_id",
                table: "document_submissions",
                column: "resident_id",
                principalTable: "residents",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_residents_resident_id",
                table: "document_submissions");

            migrationBuilder.DropIndex(
                name: "IX_document_submissions_resident_id",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "resident_id",
                table: "document_submissions");
        }
    }
}
