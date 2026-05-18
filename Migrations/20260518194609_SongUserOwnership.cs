using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicfyWebApp.Migrations
{
    /// <inheritdoc />
    public partial class SongUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Songs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_UserId",
                table: "Songs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_AspNetUsers_UserId",
                table: "Songs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Songs_AspNetUsers_UserId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Songs_UserId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Songs");
        }
    }
}
