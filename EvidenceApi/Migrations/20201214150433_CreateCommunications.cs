using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class CreateCommunications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "communications",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    delivery_method = table.Column<string>(nullable: true),
                    notify_id = table.Column<string>(nullable: true),
                    template_id = table.Column<string>(nullable: true),
                    reason = table.Column<string>(nullable: true),
                    evidence_request_id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communications", x => x.id);
                    table.ForeignKey(
                        name: "FK_communications_evidence_requests_evidence_request_id",
                        column: x => x.evidence_request_id,
                        principalTable: "evidence_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_communications_evidence_request_id",
                table: "communications",
                column: "evidence_request_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "communications");
        }
    }
}
