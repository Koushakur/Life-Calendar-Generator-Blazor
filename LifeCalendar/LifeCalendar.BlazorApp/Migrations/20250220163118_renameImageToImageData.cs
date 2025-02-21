using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeCalendar.BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class renameImageToImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Images",
                newName: "ImageData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "Images",
                newName: "Image");
        }
    }
}
