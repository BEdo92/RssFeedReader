using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RssFeedReader.Migrations
{
    /// <inheritdoc />
    public partial class MissingForeignKeyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statistics_NewsId",
                table: "Statistics");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_NewsId",
                table: "Statistics",
                column: "NewsId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statistics_NewsId",
                table: "Statistics");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_NewsId",
                table: "Statistics",
                column: "NewsId");
        }
    }
}
