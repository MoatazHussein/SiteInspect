using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteInspect.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionDraftTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastDraftSavedAtUtc",
                table: "Inspections",
                type: "datetimeoffset(7)",
                precision: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDraftSavedAtUtc",
                table: "Inspections");
        }
    }
}
