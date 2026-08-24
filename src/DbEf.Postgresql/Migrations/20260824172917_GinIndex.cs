using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muzonia.DbEf.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class GinIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Title",
                table: "Tracks",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_Description_Name",
                table: "Playlists",
                columns: new[] { "Description", "Name" })
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Description_Name",
                table: "Artists",
                columns: new[] { "Description", "Name" })
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Title",
                table: "Albums",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_Title",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_Description_Name",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Artists_Description_Name",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_Title",
                table: "Albums");
        }
    }
}
