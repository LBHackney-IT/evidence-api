using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EvidenceApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evidence_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    resident_id = table.Column<Guid>(nullable: false),
                    delivery_methods = table.Column<List<string>>(nullable: true),
                    document_types = table.Column<List<string>>(nullable: true),
                    service_requested_by = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "residents",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    phone_number = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residents", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_requests");

            migrationBuilder.DropTable(
                name: "residents");
        }
    }
}
