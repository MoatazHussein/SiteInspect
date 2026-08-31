using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteInspect.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2InspectionManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "InspectionNumberSequence",
                startValue: 31L);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Inspections",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Inspections",
                type: "datetimeoffset(7)",
                precision: 7,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InspectionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionAttachments_InspectionObservations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "InspectionObservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_ObservationId",
                table: "InspectionAttachments",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_StoredFileName",
                table: "InspectionAttachments",
                column: "StoredFileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionAttachments");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Inspections");

            migrationBuilder.DropSequence(
                name: "InspectionNumberSequence");
        }
    }
}
