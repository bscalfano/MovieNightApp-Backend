using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieNightApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLetterboxdUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LetterboxdUsername",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LetterboxdUsername",
                table: "AspNetUsers");
        }
    }
}
