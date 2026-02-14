using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieNightApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreToMovieNight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "MovieNights",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "MovieNights");
        }
    }
}
