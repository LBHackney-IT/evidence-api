using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceApi.Migrations
{
    /// <inheritdoc />
    public partial class BackfillResidentsGroupIdWithExistingResidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO residents_team_group_id SELECT uuid_in(overlay(overlay(md5(random()::text || ':' || random()::text) placing '4' from 13) placing to_hex(floor(random()*(11-8+1) + 8)::int)::text from 17)::cstring) AS id,
                                           document_submissions.resident_id AS resident_id,
                                           document_submissions.team as team, uuid_in(overlay(overlay(md5(random()::text || ':' || random()::text) placing '4' from 13) placing to_hex(floor(random()*(11-8+1) + 8)::int)::text from 17)::cstring) AS group_id,
                                          now() as created_at
                                    FROM document_submissions
                                    WHERE resident_id not in (SELECT residents_team_group_id.resident_id from residents_team_group_id WHERE residents_team_group_id.team = team)
                                    GROUP BY resident_id, document_submissions.team
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
