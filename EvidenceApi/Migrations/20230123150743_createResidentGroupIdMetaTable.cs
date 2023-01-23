using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceApi.Migrations
{
    /// <inheritdoc />
    public partial class createResidentGroupIdMetaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "residents_team_group_id",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    residentid = table.Column<Guid>(name: "resident_id", type: "uuid", nullable: false),
                    team = table.Column<string>(type: "text", nullable: true),
                    groupid = table.Column<Guid>(name: "group_id", type: "uuid", nullable: true),
                    createdat = table.Column<DateTime>(name: "created_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residents_team_group_id", x => x.id);
                    table.ForeignKey(
                        name: "FK_residents_team_group_id_residents_resident_id",
                        column: x => x.residentid,
                        principalTable: "residents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_residents_team_group_id_resident_id",
                table: "residents_team_group_id",
                column: "resident_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "residents_team_group_id");
        }
    }
}
