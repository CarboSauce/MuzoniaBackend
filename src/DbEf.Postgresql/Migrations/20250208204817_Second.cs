using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Tracks_SongId",
                table: "QueueEntries");

            migrationBuilder.RenameColumn(
                name: "SongId",
                table: "QueueEntries",
                newName: "TrackId");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_SongId",
                table: "QueueEntries",
                newName: "IX_QueueEntries_TrackId");

            migrationBuilder.AlterColumn<int>(
                name: "Index",
                table: "QueueEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Timestamp",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentIndex",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "PlaybackQueues",
                type: "uuid",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlaying",
                table: "PlaybackQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrackCount",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Volume",
                table: "PlaybackQueues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackQueues_DeviceId",
                table: "PlaybackQueues",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ConnectionId",
                table: "Devices",
                column: "ConnectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaybackQueues_Devices_DeviceId",
                table: "PlaybackQueues",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Tracks_TrackId",
                table: "QueueEntries",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaybackQueues_Devices_DeviceId",
                table: "PlaybackQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Tracks_TrackId",
                table: "QueueEntries");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_PlaybackQueues_DeviceId",
                table: "PlaybackQueues");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "PlaybackQueues");

            migrationBuilder.DropColumn(
                name: "IsPlaying",
                table: "PlaybackQueues");

            migrationBuilder.DropColumn(
                name: "TrackCount",
                table: "PlaybackQueues");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "PlaybackQueues");

            migrationBuilder.RenameColumn(
                name: "TrackId",
                table: "QueueEntries",
                newName: "SongId");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_TrackId",
                table: "QueueEntries",
                newName: "IX_QueueEntries_SongId");

            migrationBuilder.AlterColumn<long>(
                name: "Index",
                table: "QueueEntries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "PlaybackQueues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "CurrentIndex",
                table: "PlaybackQueues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Tracks_SongId",
                table: "QueueEntries",
                column: "SongId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
