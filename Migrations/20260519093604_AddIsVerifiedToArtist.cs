using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicfyWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToArtist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Artists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Artists");
        }
    }
}
