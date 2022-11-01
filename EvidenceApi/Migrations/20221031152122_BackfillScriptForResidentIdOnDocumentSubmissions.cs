using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class BackfillScriptForResidentIdOnDocumentSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE document_submissions 
                SET resident_id = subquery.resident_id
                FROM (
                SELECT evidence_requests.id, evidence_requests.resident_id 
                FROM evidence_requests, document_submissions
                where document_submissions.evidence_request_id = evidence_requests.id
                )  
                AS subquery
                where document_submissions.evidence_request_id = subquery.id;
            ");
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
    }
}
