using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteInspect.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceInspectionSequenceWithNumberSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "InspectionNumberSequence");

            migrationBuilder.CreateTable(
                name: "NumberSeriesCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeriesName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastIssuedValue = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSeriesCounters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NumberSeriesCounters_SeriesName_Year",
                table: "NumberSeriesCounters",
                columns: new[] { "SeriesName", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumberSeriesCounters");

            migrationBuilder.CreateSequence(
                name: "InspectionNumberSequence",
                startValue: 31L);
        }
    }
}
