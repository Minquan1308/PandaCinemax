using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSoTL.Migrations
{
    /// <inheritdoc />
    public partial class Reference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_Cinemas_CinemaId",
                table: "Screenings");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_CinemaId",
                table: "Screenings");
        }
    }
}
