using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSoTL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeatDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeatOrderDetails_Seats_SeatId",
                table: "SeatOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_SeatOrderDetails_SeatId",
                table: "SeatOrderDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SeatOrderDetails_SeatId",
                table: "SeatOrderDetails",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_SeatOrderDetails_Seats_SeatId",
                table: "SeatOrderDetails",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
