using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class MakeEvidenceRequestIdNullableOnDocumentSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_evidence_requests_evidence_request_id",
                table: "document_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_residents_resident_id",
                table: "document_submissions");

            migrationBuilder.AlterColumn<Guid>(
                name: "resident_id",
                table: "document_submissions",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "evidence_request_id",
                table: "document_submissions",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_evidence_requests_evidence_request_id",
                table: "document_submissions",
                column: "evidence_request_id",
                principalTable: "evidence_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_residents_resident_id",
                table: "document_submissions",
                column: "resident_id",
                principalTable: "residents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_evidence_requests_evidence_request_id",
                table: "document_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_residents_resident_id",
                table: "document_submissions");

            migrationBuilder.AlterColumn<Guid>(
                name: "resident_id",
                table: "document_submissions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "evidence_request_id",
                table: "document_submissions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_evidence_requests_evidence_request_id",
                table: "document_submissions",
                column: "evidence_request_id",
                principalTable: "evidence_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
