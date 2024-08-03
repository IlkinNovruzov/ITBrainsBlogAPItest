using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITBrainsBlogAPI.Migrations
{
    /// <inheritdoc />
    public partial class migNtfct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9043a39-6be8-4c43-b19b-08a3934b0126", "AQAAAAIAAYagAAAAEC6beDsNNpv8o2R6vosRYaAfJJAdDh7x6nibh1hG5xs4LlGtXUKnIFWM2VU1SaRyRA==", "bcd20c1d-7518-4580-bbb6-dcbbee154e06" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppUserId",
                table: "Notifications",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9f2740b8-aac4-47d8-b024-9e249559b00b", "AQAAAAIAAYagAAAAEDQxfO7ygQuQ0BPPMJFNyzuNcFS/NbsxUFfVuZG/ztxtXLGcLBWT1flbKYPfkijfxw==", "cf1751fa-9b49-4045-b6dc-bf8e8eaa28c9" });
        }
    }
}
