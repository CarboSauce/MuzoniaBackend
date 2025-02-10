using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToQueueSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrentQueues",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentQueues", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_CurrentQueues_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CurrentQueues_PlaybackQueues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "PlaybackQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.DropIndex(
                name: "IX_PlaybackQueues_UserId",
                table: "PlaybackQueues"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CurrentQueues_QueueId",
                table: "CurrentQueues",
                column: "QueueId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CurrentQueues");
        }
    }
}
