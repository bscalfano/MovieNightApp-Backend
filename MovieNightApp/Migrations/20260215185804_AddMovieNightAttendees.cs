using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieNightApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieNightAttendees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieNightAttendees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieNightId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RsvpedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieNightAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieNightAttendees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovieNightAttendees_MovieNights_MovieNightId",
                        column: x => x.MovieNightId,
                        principalTable: "MovieNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieNightAttendees_MovieNightId_UserId",
                table: "MovieNightAttendees",
                columns: new[] { "MovieNightId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieNightAttendees_UserId",
                table: "MovieNightAttendees",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieNightAttendees");
        }
    }
}
