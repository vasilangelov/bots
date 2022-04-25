using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOTS.Data.Migrations
{
    public partial class AddTradingWindows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrencyPairs",
                table: "CurrencyPairs");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CurrencyPairs",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrencyPairs",
                table: "CurrencyPairs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TradingWindowOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    BarrierStep = table.Column<decimal>(type: "money", nullable: false),
                    BarrierCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingWindowOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingWindows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CurrencyPairId = table.Column<int>(type: "int", nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    OpeningPrice = table.Column<decimal>(type: "money", nullable: false),
                    ClosingPrice = table.Column<decimal>(type: "money", nullable: true),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingWindows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingWindows_CurrencyPairs_CurrencyPairId",
                        column: x => x.CurrencyPairId,
                        principalTable: "CurrencyPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradingWindows_TradingWindowOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "TradingWindowOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairs_LeftId_RightId",
                table: "CurrencyPairs",
                columns: new[] { "LeftId", "RightId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradingWindows_CurrencyPairId",
                table: "TradingWindows",
                column: "CurrencyPairId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingWindows_OptionId",
                table: "TradingWindows",
                column: "OptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradingWindows");

            migrationBuilder.DropTable(
                name: "TradingWindowOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrencyPairs",
                table: "CurrencyPairs");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyPairs_LeftId_RightId",
                table: "CurrencyPairs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CurrencyPairs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrencyPairs",
                table: "CurrencyPairs",
                columns: new[] { "LeftId", "RightId" });
        }
    }
}
