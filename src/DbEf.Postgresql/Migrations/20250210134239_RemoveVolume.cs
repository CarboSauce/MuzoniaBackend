using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVolume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Volume",
                table: "PlaybackQueues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Volume",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
