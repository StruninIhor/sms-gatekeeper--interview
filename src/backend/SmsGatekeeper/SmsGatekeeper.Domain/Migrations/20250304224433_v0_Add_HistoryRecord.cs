using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmsGatekeeper.Domain.Migrations
{
    /// <inheritdoc />
    public partial class v0_Add_HistoryRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Allowed = table.Column<bool>(type: "bit", nullable: false),
                    AccountRateHit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_AccountId",
                table: "HistoryRecords",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_PhoneNumber",
                table: "HistoryRecords",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryRecords");
        }
    }
}
