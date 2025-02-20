using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class QueueSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlaybackQueues_AspNetUsers_UserId",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlaybackQueues_Devices_DeviceId",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropIndex(
                name: "IX_PlaybackQueues_DeviceId",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices"
            );

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "PlaybackQueues"
            );

            migrationBuilder.RenameTable(name: "Devices", newName: "Device");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PlaybackQueues",
                newName: "OwnerId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserId",
                table: "Device",
                newName: "IX_Device_UserId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Devices_ConnectionId",
                table: "Device",
                newName: "IX_Device_ConnectionId"
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsModifiable",
                table: "PlaybackQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "PlaybackQueues",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Device",
                table: "Device",
                column: "Id"
            );

            migrationBuilder.CreateTable(
                name: "QueueUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsBanned = table.Column<bool>(
                        type: "boolean",
                        nullable: false
                    ),
                    CreationDate = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    )
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_QueueUsers_PlaybackQueues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "PlaybackQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackQueues_OwnerId",
                table: "PlaybackQueues",
                column: "OwnerId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_QueueUsers_QueueId",
                table: "QueueUsers",
                column: "QueueId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QueueUsers_UserId",
                table: "QueueUsers",
                column: "UserId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Device_AspNetUsers_UserId",
                table: "Device",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlaybackQueues_AspNetUsers_OwnerId",
                table: "PlaybackQueues",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_AspNetUsers_UserId",
                table: "Device"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_PlaybackQueues_AspNetUsers_OwnerId",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropTable(name: "QueueUsers");

            migrationBuilder.DropIndex(
                name: "IX_PlaybackQueues_OwnerId",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Device", table: "Device");

            migrationBuilder.DropColumn(
                name: "IsModifiable",
                table: "PlaybackQueues"
            );

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "PlaybackQueues"
            );

            migrationBuilder.RenameTable(name: "Device", newName: "Devices");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "PlaybackQueues",
                newName: "UserId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Device_UserId",
                table: "Devices",
                newName: "IX_Devices_UserId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Device_ConnectionId",
                table: "Devices",
                newName: "IX_Devices_ConnectionId"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "PlaybackQueues",
                type: "uuid",
                maxLength: 256,
                nullable: true
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "Id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackQueues_DeviceId",
                table: "PlaybackQueues",
                column: "DeviceId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlaybackQueues_AspNetUsers_UserId",
                table: "PlaybackQueues",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PlaybackQueues_Devices_DeviceId",
                table: "PlaybackQueues",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id"
            );
        }
    }
}
