using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class AddAuditEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    user_email = table.Column<string>(nullable: true),
                    url_visited = table.Column<string>(nullable: true),
                    http_method = table.Column<string>(nullable: true),
                    request_body = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events");
        }
    }
}
