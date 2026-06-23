using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowAtomicSectionHandoutVersionItemTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_HandoutVersionItems_TargetType",
                table: "HandoutVersionItems");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HandoutVersionItems_TargetType",
                table: "HandoutVersionItems",
                sql: "\"TargetType\" IN (1, 2, 4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_HandoutVersionItems_TargetType",
                table: "HandoutVersionItems");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HandoutVersionItems_TargetType",
                table: "HandoutVersionItems",
                sql: "\"TargetType\" IN (1, 2)");
        }
    }
}
