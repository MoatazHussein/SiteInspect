using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteInspect.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3CorrectiveActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_InspectionObservations_InspectionId_Id",
                table: "InspectionObservations",
                columns: new[] { "InspectionId", "Id" });

            migrationBuilder.CreateTable(
                name: "CorrectiveActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    RejectedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_AspNetUsers_AssignedContractorId",
                        column: x => x.AssignedContractorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_InspectionObservations_InspectionId_ObservationId",
                        columns: x => new { x.InspectionId, x.ObservationId },
                        principalTable: "InspectionObservations",
                        principalColumns: new[] { "InspectionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_AssignedContractorId_Status_DueAtUtc",
                table: "CorrectiveActions",
                columns: new[] { "AssignedContractorId", "Status", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_InspectionId_ObservationId",
                table: "CorrectiveActions",
                columns: new[] { "InspectionId", "ObservationId" });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_ObservationId",
                table: "CorrectiveActions",
                column: "ObservationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectiveActions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InspectionObservations_InspectionId_Id",
                table: "InspectionObservations");
        }
    }
}
