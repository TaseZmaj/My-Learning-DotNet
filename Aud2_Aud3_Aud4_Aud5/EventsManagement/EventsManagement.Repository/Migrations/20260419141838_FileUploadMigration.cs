using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManagement.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FileUploadMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventImageId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventsImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventImageId",
                table: "Events",
                column: "EventImageId",
                unique: true,
                filter: "[EventImageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventsImages_EventImageId",
                table: "Events",
                column: "EventImageId",
                principalTable: "EventsImages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventsImages_EventImageId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "EventsImages");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventImageId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventImageId",
                table: "Events");
        }
    }
}
