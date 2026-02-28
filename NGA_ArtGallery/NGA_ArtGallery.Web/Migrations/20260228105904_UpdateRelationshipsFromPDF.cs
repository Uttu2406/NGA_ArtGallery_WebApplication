using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGA_ArtGallery.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipsFromPDF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Curator",
                schema: "Gallery",
                table: "Galleries");

            migrationBuilder.AddColumn<int>(
                name: "ArtistID",
                schema: "Gallery",
                table: "Galleries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GalleryID",
                schema: "Gallery",
                table: "Artworks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_ArtistID",
                schema: "Gallery",
                table: "Galleries",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_GalleryID",
                schema: "Gallery",
                table: "Artworks",
                column: "GalleryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Artworks_Galleries_GalleryID",
                schema: "Gallery",
                table: "Artworks",
                column: "GalleryID",
                principalSchema: "Gallery",
                principalTable: "Galleries",
                principalColumn: "GalleryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_Artists_ArtistID",
                schema: "Gallery",
                table: "Galleries",
                column: "ArtistID",
                principalSchema: "Gallery",
                principalTable: "Artists",
                principalColumn: "ArtistID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artworks_Galleries_GalleryID",
                schema: "Gallery",
                table: "Artworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_Artists_ArtistID",
                schema: "Gallery",
                table: "Galleries");

            migrationBuilder.DropIndex(
                name: "IX_Galleries_ArtistID",
                schema: "Gallery",
                table: "Galleries");

            migrationBuilder.DropIndex(
                name: "IX_Artworks_GalleryID",
                schema: "Gallery",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "ArtistID",
                schema: "Gallery",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "GalleryID",
                schema: "Gallery",
                table: "Artworks");

            migrationBuilder.AddColumn<string>(
                name: "Curator",
                schema: "Gallery",
                table: "Galleries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
