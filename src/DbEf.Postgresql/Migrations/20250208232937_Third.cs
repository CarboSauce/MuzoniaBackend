using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatMode",
                table: "PlaybackQueues");

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeat",
                table: "PlaybackQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRepeat",
                table: "PlaybackQueues");

            migrationBuilder.AddColumn<int>(
                name: "RepeatMode",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
