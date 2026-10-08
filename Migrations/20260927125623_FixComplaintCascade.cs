using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tagalong.Migrations
{
    /// <inheritdoc />
    public partial class FixComplaintCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Bookings_BookingId",
                table: "Complaints",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
