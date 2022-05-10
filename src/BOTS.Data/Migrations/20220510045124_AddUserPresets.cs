using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOTS.Data.Migrations
{
    public partial class AddUserPresets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPresets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CurrencyPairId = table.Column<int>(type: "int", nullable: false),
                    ChartType = table.Column<int>(type: "int", nullable: false),
                    Payout = table.Column<decimal>(type: "money", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPresets_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPresets_CurrencyPairs_CurrencyPairId",
                        column: x => x.CurrencyPairId,
                        principalTable: "CurrencyPairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPresets_CurrencyPairId",
                table: "UserPresets",
                column: "CurrencyPairId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPresets_OwnerId",
                table: "UserPresets",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPresets");
        }
    }
}
