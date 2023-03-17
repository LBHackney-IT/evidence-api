using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvidenceApi.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDoubleSpaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Update residents SET name= replace(name, '  ', ' ');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
