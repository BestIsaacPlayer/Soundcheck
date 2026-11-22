using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soundcheck.Web.Migrations
{
    /// <inheritdoc />
    public partial class AllowRebookingCancelledSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_AvailabilitySlotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AvailabilitySlotId",
                table: "Bookings",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[Status] <> 'Cancelled'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_AvailabilitySlotId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AvailabilitySlotId",
                table: "Bookings",
                column: "AvailabilitySlotId",
                unique: true);
        }
    }
}
