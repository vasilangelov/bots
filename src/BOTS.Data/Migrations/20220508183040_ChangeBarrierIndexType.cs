using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOTS.Data.Migrations
{
    public partial class ChangeBarrierIndexType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "BarrierIndex",
                table: "Bets",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BarrierIndex",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }
    }
}
