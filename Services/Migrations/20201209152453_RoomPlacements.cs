using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class RoomPlacements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomPlacements",
                columns: table => new
                {
                    RoomPlacementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Positions = table.Column<int>(type: "nvarchar(max)", nullable: true),
                    Rotation = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPlacements", x => x.RoomPlacementId);
                    table.ForeignKey(
                        name: "FK_RoomPlacements_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomPlacements");
        }
    }
}
