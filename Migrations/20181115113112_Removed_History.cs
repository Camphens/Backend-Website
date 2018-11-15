using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BackendWebsite.Migrations
{
    public partial class Removed_History : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderHistory_HistoryId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderHistory");

            migrationBuilder.DropIndex(
                name: "IX_Orders_HistoryId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HistoryId",
                table: "Orders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_HistoryId",
                table: "Orders",
                column: "HistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderHistory_HistoryId",
                table: "Orders",
                column: "HistoryId",
                principalTable: "OrderHistory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
