using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSoTL.Migrations
{
    /// <inheritdoc />
    public partial class Add2005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_Cinemas_CinemaId",
                table: "Screenings");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Rooms_RoomId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_RoomId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_CinemaId",
                table: "Screenings");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "Screenings");

            migrationBuilder.AddColumn<int>(
                name: "ScreeningId",
                table: "Seats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_ScreeningId",
                table: "Seats",
                column: "ScreeningId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Screenings_ScreeningId",
                table: "Seats",
                column: "ScreeningId",
                principalTable: "Screenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Screenings_ScreeningId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_ScreeningId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "ScreeningId",
                table: "Seats");

            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "Screenings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_RoomId",
                table: "Seats",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_CinemaId",
                table: "Screenings",
                column: "CinemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_Cinemas_CinemaId",
                table: "Screenings",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Rooms_RoomId",
                table: "Seats",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
