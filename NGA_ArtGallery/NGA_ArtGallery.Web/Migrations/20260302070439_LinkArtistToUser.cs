using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGA_ArtGallery.Web.Migrations
{
    /// <inheritdoc />
    public partial class LinkArtistToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                schema: "Gallery",
                table: "Artists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                schema: "Gallery",
                table: "Artists");
        }
    }
}
