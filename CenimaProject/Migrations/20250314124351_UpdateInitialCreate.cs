using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    MovieID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieGenre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieDuration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoviePosterURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieLaguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieSubTitle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.MovieID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
