using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class CreateDocumentSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    claim_id = table.Column<string>(nullable: true),
                    rejection_reason = table.Column<string>(nullable: true),
                    state = table.Column<int>(nullable: false),
                    evidence_request_id = table.Column<Guid>(nullable: false),
                    document_type_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_submissions_evidence_requests_evidence_request_id",
                        column: x => x.evidence_request_id,
                        principalTable: "evidence_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_evidence_request_id",
                table: "document_submissions",
                column: "evidence_request_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_submissions");
        }
    }
}
